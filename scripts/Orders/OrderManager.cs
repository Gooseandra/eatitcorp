using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class OrderManager : MonoBehaviour
{
    [System.Serializable]
    public class OrderItem
    {
        public string itemName;
        public int quantity;
        public int pricePerUnit;
    }

    [System.Serializable]
    public class Order
    {
        public List<OrderItem> items = new List<OrderItem>();
        public bool isCompleted = false;
    }

    [System.Serializable]
    public class AllowedProduct
    {
        public ItemPickup itemPickup; // тут хранится и спрайт, и имя
        public int price;
    }

    public TerminalOrderSystem terminalOrderSystem; // Ссылка на систему заказов терминала

    public Storage storage;
    public LiftController liftController;

    [Header("Разрешённые продукты с ценами")]
    public AllowedProduct[] allowedItemPickups;

    [Header("Настройки генерации заказов")]
    public int minItemTypes = 2;
    public int maxItemTypes = 5;
    public int minStackMultiplier = 1;
    public int maxStackMultiplier = 3;
    public float orderInterval = 60f;

    [Header("UI")]
    public TextMeshProUGUI moneyText;
    public Transform ordersContainer;         // Контейнер с Vertical Layout Group
    public GameObject orderCardPrefab;        // Префаб карточки заказа

    public List<Order> activeOrders = new List<Order>();

    private float orderTimer;

    private void Start()
    {
        GenerateRandomOrder();
        orderTimer = orderInterval;
        UpdateMoneyUI(0);
    }

    private void Update()
    {
        orderTimer -= Time.deltaTime;
        if (orderTimer <= 0f)
        {
            GenerateRandomOrder();
            orderTimer = orderInterval;
        }
    }

    public void TrySendOrder()
    {
        if (activeOrders.Count == 0)
        {
            Debug.Log("Нет активных заказов.");
            return;
        }

        var currentOrder = activeOrders[0];

        if (CheckOrderReady(currentOrder))
        {
            RemoveItemsFromStorage(currentOrder);
            currentOrder.isCompleted = true;
            activeOrders.RemoveAt(0);

            liftController.SendLift();

            int orderTotal = 0;
            foreach (var item in currentOrder.items)
            {
                orderTotal += item.quantity * item.pricePerUnit;
            }

            // Получаем баланс из moneyText (только число)
            int currentMoney = 0;
            if (moneyText != null && int.TryParse(moneyText.text, out currentMoney))
            {
                currentMoney += orderTotal;
                moneyText.text = currentMoney.ToString();
            }
            else
            {
                Debug.LogWarning("Невозможно считать баланс из moneyText.");
                moneyText.text = orderTotal.ToString();
            }

            Debug.Log($"Заказ отправлен! Получено {orderTotal} монет.");

            GenerateRandomOrder();

            RefreshOrdersUI();
        }
        else
        {
            Debug.Log("Недостаточно предметов для выполнения заказа.");
        }
    }

    public void GenerateRandomOrder()
    {
        if (allowedItemPickups == null || allowedItemPickups.Length == 0)
        {
            Debug.LogWarning("Нет разрешённых префабов для генерации заказов.");
            return;
        }

        int productCount = Random.Range(minItemTypes, maxItemTypes + 1);
        List<AllowedProduct> pool = new List<AllowedProduct>(allowedItemPickups);
        Order newOrder = new Order();

        for (int i = 0; i < productCount && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            AllowedProduct allowedProduct = pool[index];
            pool.RemoveAt(index);

            if (allowedProduct.itemPickup.item == null)
            {
                Debug.LogWarning("Один из префабов не содержит item.");
                continue;
            }

            int quantity = Random.Range(minStackMultiplier, maxStackMultiplier + 1) * allowedProduct.itemPickup.item.maxStack;

            newOrder.items.Add(new OrderItem
            {
                itemName = allowedProduct.itemPickup.item.name,
                quantity = quantity,
                pricePerUnit = allowedProduct.price
            });
        }

        activeOrders.Add(newOrder);
        Debug.Log("Новый заказ сгенерирован.");

        RefreshOrdersUI();
    }

    private bool CheckOrderReady(Order order)
    {
        foreach (var orderItem in order.items)
        {
            int total = 0;
            foreach (var item in storage.storageItems)
            {
                if (item != null && item.name == orderItem.itemName)
                {
                    total += item.amount;
                }
            }

            if (total < orderItem.quantity)
                return false;
        }
        return true;
    }

    private void RemoveItemsFromStorage(Order order)
    {
        foreach (var orderItem in order.items)
        {
            int remaining = orderItem.quantity;

            for (int i = 0; i < storage.storageItems.Count && remaining > 0; i++)
            {
                var item = storage.storageItems[i];
                if (item != null && item.name == orderItem.itemName)
                {
                    int toRemove = Mathf.Min(item.amount, remaining);
                    item.amount -= toRemove;
                    remaining -= toRemove;

                    if (item.amount <= 0)
                        storage.storageItems[i] = null;
                }
            }
        }

        storage.UpdateStorageUI();
    }

    private void RefreshOrdersUI()
    {
        // Чистим контейнер
        foreach (Transform child in ordersContainer)
        {
            Destroy(child.gameObject);
        }

        // Для каждого заказа и каждого товара создаём карточку
        foreach (var order in activeOrders)
        {
            foreach (var orderItem in order.items)
            {
                GameObject card = Instantiate(orderCardPrefab, ordersContainer);

                // Дочерние элементы
                Transform iconTransform = card.transform.Find("Icon");
                Transform nameTransform = card.transform.Find("Name");
                Transform priceTransform = card.transform.Find("Price");
                Transform countTransform = card.transform.Find("Count");  // Новое поле

                // Найти AllowedProduct по имени, чтобы получить спрайт и имя из ItemPickup
                AllowedProduct product = FindAllowedProductByName(orderItem.itemName);

                // Назначаем имя
                if (nameTransform != null)
                {
                    var nameText = nameTransform.GetComponent<TextMeshProUGUI>();
                    if (nameText != null)
                        nameText.text = product != null ? product.itemPickup.item.name : orderItem.itemName;
                }

                // Назначаем цену (кол-во * цена за ед.)
                if (priceTransform != null)
                {
                    var priceText = priceTransform.GetComponent<TextMeshProUGUI>();
                    if (priceText != null)
                        priceText.text = (orderItem.quantity * orderItem.pricePerUnit).ToString();
                }

                // Назначаем количество
                if (countTransform != null)
                {
                    var countText = countTransform.GetComponent<TextMeshProUGUI>();
                    if (countText != null)
                        countText.text = orderItem.quantity.ToString();
                }

                // Назначаем иконку
                if (iconTransform != null)
                {
                    var iconImage = iconTransform.GetComponent<UnityEngine.UI.Image>();
                    if (iconImage != null && product != null && product.itemPickup.item.icon != null)
                    {
                        iconImage.sprite = product.itemPickup.item.icon;
                    }
                }
            }
        }
    }


    private AllowedProduct FindAllowedProductByName(string itemName)
    {
        foreach (var product in allowedItemPickups)
        {
            if (product.itemPickup != null && product.itemPickup.item != null && product.itemPickup.item.name == itemName)
                return product;
        }
        return null;
    }

    private void UpdateMoneyUI(int money)
    {
          terminalOrderSystem.balance += money;
    }
}
