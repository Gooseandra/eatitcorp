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

            // Получаем иконку из itemPrefab или из item.icon
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
        Debug.Log("Добавлено в корзину: " + item.itemPrefab.name);
        // Добавление в корзину
    }

    private void UpdateBalanceText()
    {
        balanceText.text = "Баланс: $" + balance;
    }
}
