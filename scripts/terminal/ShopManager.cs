using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopItemData
    {
        public GameObject itemPrefab;
        public int price = 100;
    }

    public List<ShopItemData> availableItems; // можно заполнить вручную или автоматически
    public GameObject itemCardPrefab;         // префаб UI-карточки товара
    public Transform itemListContentParent;   // объект Content в ScrollView

    public TerminalOrderSystem orderSystem;   // ссылка на скрипт оформления заказа

    void Start()
    {
        PopulateShop();
    }

    public void PopulateShop()
    {
        foreach (var itemData in availableItems)
        {
            GameObject card = Instantiate(itemCardPrefab, itemListContentParent);

            ItemPickup pickup = itemData.itemPrefab.GetComponent<ItemPickup>();
            if (pickup == null || pickup.item == null)
            {
                Debug.LogWarning("Prefab missing ItemPickup or Item");
                continue;
            }

            int maxStack = pickup.item.maxStack;

            card.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = pickup.item.name;
            card.transform.Find("PriceText").GetComponent<TextMeshProUGUI>().text = "$" + itemData.price;
            card.transform.Find("Icon").GetComponent<Image>().sprite = pickup.item.icon;

            TMP_InputField amountInput = card.transform.Find("AmountInputField").GetComponent<TMP_InputField>();
            amountInput.text = "1"; // стартовое значение количества

            // Добавляем обработчик изменения текста для ограничения maxStack
            amountInput.onValueChanged.AddListener((string val) =>
            {
                int parsedValue;
                if (!int.TryParse(val, out parsedValue) || parsedValue < 1)
                {
                    amountInput.text = "1";
                    return;
                }
                if (parsedValue > maxStack)
                {
                    amountInput.text = maxStack.ToString();
                }
            });

            Button addButton = card.transform.Find("AddButton").GetComponent<Button>();
            addButton.onClick.AddListener(() =>
            {
                int amount = 1;
                if (!int.TryParse(amountInput.text, out amount) || amount < 1)
                {
                    amount = 1; // по умолчанию 1, если неверный ввод
                    amountInput.text = "1";
                }

                if (amount > maxStack)
                {
                    amount = maxStack;
                    amountInput.text = maxStack.ToString();
                }

                TerminalOrderSystem.OrderItem orderItem = new TerminalOrderSystem.OrderItem
                {
                    itemPrefab = itemData.itemPrefab,
                    price = itemData.price,
                    amount = amount
                };

                orderSystem.AddToCart(orderItem);
            });
        }
    }
}
