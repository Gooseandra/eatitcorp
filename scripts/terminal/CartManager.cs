//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class CartManager : MonoBehaviour
//{
//    [System.Serializable]
//    public class CartItem
//    {
//        public TerminalOrderSystem.OrderItem orderItem;
//        public GameObject cardObject;
//    }

//    public Transform cartContentParent;  // Контейнер для карточек в корзине (Content)
//    public GameObject cartCardPrefab;    // Префаб карточки для корзины

//    public TextMeshProUGUI totalPriceText; // Итоговая стоимость
//    public Button placeOrderButton;      // Кнопка оформления заказа

//    private List<CartItem> cartItems = new List<CartItem>();

//    public TerminalOrderSystem terminalOrderSystem; // Для взаимодействия с основным заказом

//    private void Start()
//    {
//        placeOrderButton.onClick.AddListener(PlaceOrder);
//    }

//    public void OpenCart()
//    {
//        gameObject.SetActive(true);
//        RefreshCartUI();
//    }

//    public void CloseCart()
//    {
//        gameObject.SetActive(false);
//    }

//    public void RefreshCartUI()
//    {
//        // Удаляем все текущие карточки
//        foreach (var ci in cartItems)
//        {
//            Destroy(ci.cardObject);
//        }
//        cartItems.Clear();

//        int totalCost = 0;

//        // Перебираем все предметы из terminalOrderSystem (корзина основная)
//        foreach (var orderItem in terminalOrderSystem.GetCartItems())
//        {
//            GameObject card = Instantiate(cartCardPrefab, cartContentParent);

//            // Заполнение UI карточки
//            Image iconImage = card.transform.Find("Icon").GetComponent<Image>();
//            TMP_InputField countInput = card.transform.Find("Count InputField").GetComponent<TMP_InputField>();
//            Button deleteButton = card.transform.Find("Delete Button").GetComponent<Button>();
//            TextMeshProUGUI name = card.transform.Find("Name").GetComponent<TextMeshProUGUI>();
//            TextMeshProUGUI priceText = card.transform.Find("Price").GetComponent<TextMeshProUGUI>();
//            TextMeshProUGUI totalPriceCardText = card.transform.Find("TotalPrice").GetComponent<TextMeshProUGUI>();

//            // Получаем ItemPickup для доступа к maxStack и иконке
//            ItemPickup pickup = orderItem.itemPrefab.GetComponent<ItemPickup>();
//            if (pickup == null || pickup.item == null)
//            {
//                Debug.LogWarning("Prefab missing ItemPickup or Item");
//                continue;
//            }

//            iconImage.sprite = pickup.item.icon;
//            priceText.text = "$" + orderItem.price;

//            name.text = orderItem.itemPrefab.name;

//            countInput.text = orderItem.amount.ToString();

//            // Обновление общей стоимости карточки
//            int cardTotal = orderItem.price * orderItem.amount;
//            totalPriceCardText.text = "$" + cardTotal;

//            totalCost += cardTotal;

//            // Лимит количества в поле countInput по maxStack
//            int maxStack = pickup.item.maxStack;

//            countInput.onValueChanged.AddListener((string val) =>
//            {
//                int parsed;
//                if (!int.TryParse(val, out parsed) || parsed < 1)
//                {
//                    parsed = 1;
//                    countInput.text = "1";
//                }
//                else if (parsed > maxStack)
//                {
//                    parsed = maxStack;
//                    countInput.text = maxStack.ToString();
//                }

//                // Обновляем количество в основной корзине и UI карточки
//                orderItem.amount = parsed;
//                totalPriceCardText.text = "$" + (orderItem.price * parsed);

//                UpdateTotalPrice();
//            });

//            deleteButton.onClick.AddListener(() =>
//            {
//                terminalOrderSystem.RemoveFromCart(orderItem);
//                RefreshCartUI();
//            });

//            cartItems.Add(new CartItem { orderItem = orderItem, cardObject = card });
//        }

//        totalPriceText.text = "Итого: $" + totalCost;
//        placeOrderButton.interactable = totalCost > 0;
//    }

//    private void UpdateTotalPrice()
//    {
//        int totalCost = 0;
//        foreach (var ci in cartItems)
//        {
//            totalCost += ci.orderItem.price * ci.orderItem.amount;
//        }
//        totalPriceText.text = "Итого: $" + totalCost;
//        placeOrderButton.interactable = totalCost > 0;
//    }

//    private void PlaceOrder()
//    {
//        terminalOrderSystem.PlaceOrder();
//        RefreshCartUI();
//    }
//}
