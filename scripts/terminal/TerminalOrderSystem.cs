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

    public Transform itemsParent;         // Content �� Scroll View
    public GameObject itemCardPrefab;     // ������ �������� ������

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

            // �������� ������ �� itemPrefab ��� �� item.icon
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
        Debug.Log("��������� � �������: " + item.itemPrefab.name);
        // ���������� � �������
    }

    private void UpdateBalanceText()
    {
        balanceText.text = "������: $" + balance;
    }
}
