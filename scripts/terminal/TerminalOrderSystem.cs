using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TerminalOrderSystem : MonoBehaviour
{
    public TextMeshProUGUI balanceText;
    public GameObject storagePrefab;
    public Transform player;
    public int balance = 1000;

    [System.Serializable]
    public class OrderItem
    {
        public GameObject itemPrefab;
        public int amount;
        public int price;
    }

    public List<OrderItem> availableItems = new List<OrderItem>();

    public Transform itemsParent;         // Content из Scroll View
    public GameObject itemCardPrefab;     // Префаб карточки товара

    // Корзина - список заказанных товаров
    public List<OrderItem> cart = new List<OrderItem>();

    private void Start()
    {
        PopulateShopUI();
        UpdateBalanceText();
    }

    void PopulateShopUI()
    {
        foreach (var item in availableItems)
        {
            GameObject card = Instantiate(itemCardPrefab, itemsParent);

            Image icon = card.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI nameText = card.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI priceText = card.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();
            Button buyButton = card.transform.Find("BuyButton").GetComponent<Button>();

            icon.sprite = item.itemPrefab.GetComponent<ItemPickup>().item.icon;

            nameText.text = item.itemPrefab.name;
            priceText.text = "$" + item.price;

            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() =>
            {
                AddToCart(item);
            });
        }
    }

    public void AddToCart(OrderItem item)
    {
        // Проверяем, есть ли уже такой предмет в корзине
        OrderItem existing = cart.Find(x => x.itemPrefab == item.itemPrefab);

        int maxStack = item.itemPrefab.GetComponent<ItemPickup>().item.maxStack;

        if (existing != null)
        {
            existing.amount += 1; // добавляем 1 штуку при каждом клике
            if (existing.amount > maxStack)
            {
                existing.amount = maxStack; // не больше maxStack
                Debug.Log("Достигнут максимум стека для " + item.itemPrefab.name);
            }
        }
        else
        {
            // Создаем новый заказ с 1 шт.
            cart.Add(new OrderItem
            {
                itemPrefab = item.itemPrefab,
                amount = 1,
                price = item.price
            });
        }

        Debug.Log("Добавлено в корзину: " + item.itemPrefab.name + ", всего: " + (existing != null ? existing.amount : 1));
    }

    // Метод для получения содержимого корзины (для UI корзины, оформления заказа и т.д.)
    public List<OrderItem> GetCartItems()
    {
        return cart;
    }

    // Пример функции оформления заказа, создающей список InitialStorageItem из корзины
    public List<InitialStorageItem> GenerateStorageItemsFromCart()
    {
        List<InitialStorageItem> storageItems = new List<InitialStorageItem>();

        foreach (var orderItem in cart)
        {
            int maxStack = orderItem.itemPrefab.GetComponent<ItemPickup>().item.maxStack;
            int remainingAmount = orderItem.amount;

            while (remainingAmount > 0)
            {
                int stackAmount = Mathf.Min(remainingAmount, maxStack);

                storageItems.Add(new InitialStorageItem
                {
                    itemPrefab = orderItem.itemPrefab,
                    stacksCount = 1,
                    amountPerStack = stackAmount
                });

                remainingAmount -= stackAmount;
            }
        }

        return storageItems;
    }

    public void OnPlaceOrderClicked()
    {
        int totalCost = 0;
        foreach (var item in cart)
            totalCost += item.price * item.amount;

        if (balance < totalCost)
        {
            Debug.Log("Недостаточно средств для заказа");
            return;
        }

        balance -= totalCost;
        UpdateBalanceText();

        // Генерируем случайную позицию в радиусе 60 от игрока, высота 20
        Vector3 randomDirection = Random.insideUnitSphere * 60f;
        randomDirection.y = 0; // обнуляем высоту, чтобы не смещаться по Y
        Vector3 spawnPosition = player.position + randomDirection;
        spawnPosition.y = 20f; // выставляем фиксированную высоту

        // Спавним хранилище
        GameObject storageInstance = Instantiate(storagePrefab, spawnPosition, Quaternion.identity);

        Storage storage = storageInstance.GetComponent<Storage>();
        if (storage == null)
        {
            Debug.LogError("Storage prefab не содержит компонент Storage!");
            return;
        }

        storage.initialItems.Clear();

        List<InitialStorageItem> itemsToAdd = GenerateStorageItemsFromCart();

        storage.initialItems.AddRange(itemsToAdd);
        storage.SetInitItems();

        Debug.Log("Заказ оформлен и хранилище создано");

        cart.Clear();
    }



    private void UpdateBalanceText()
    {
        balanceText.text = balance.ToString();
    }
}
