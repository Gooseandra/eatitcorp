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

    // ������� - ������ ���������� �������
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
        // ���������, ���� �� ��� ����� ������� � �������
        OrderItem existing = cart.Find(x => x.itemPrefab == item.itemPrefab);

        int maxStack = item.itemPrefab.GetComponent<ItemPickup>().item.maxStack;

        if (existing != null)
        {
            existing.amount += 1; // ��������� 1 ����� ��� ������ �����
            if (existing.amount > maxStack)
            {
                existing.amount = maxStack; // �� ������ maxStack
                Debug.Log("��������� �������� ����� ��� " + item.itemPrefab.name);
            }
        }
        else
        {
            // ������� ����� ����� � 1 ��.
            cart.Add(new OrderItem
            {
                itemPrefab = item.itemPrefab,
                amount = 1,
                price = item.price
            });
        }

        Debug.Log("��������� � �������: " + item.itemPrefab.name + ", �����: " + (existing != null ? existing.amount : 1));
    }

    // ����� ��� ��������� ����������� ������� (��� UI �������, ���������� ������ � �.�.)
    public List<OrderItem> GetCartItems()
    {
        return cart;
    }

    // ������ ������� ���������� ������, ��������� ������ InitialStorageItem �� �������
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
            Debug.Log("������������ ������� ��� ������");
            return;
        }

        balance -= totalCost;
        UpdateBalanceText();

        // ���������� ��������� ������� � ������� 60 �� ������, ������ 20
        Vector3 randomDirection = Random.insideUnitSphere * 60f;
        randomDirection.y = 0; // �������� ������, ����� �� ��������� �� Y
        Vector3 spawnPosition = player.position + randomDirection;
        spawnPosition.y = 20f; // ���������� ������������� ������

        // ������� ���������
        GameObject storageInstance = Instantiate(storagePrefab, spawnPosition, Quaternion.identity);

        Storage storage = storageInstance.GetComponent<Storage>();
        if (storage == null)
        {
            Debug.LogError("Storage prefab �� �������� ��������� Storage!");
            return;
        }

        storage.initialItems.Clear();

        List<InitialStorageItem> itemsToAdd = GenerateStorageItemsFromCart();

        storage.initialItems.AddRange(itemsToAdd);
        storage.SetInitItems();

        Debug.Log("����� �������� � ��������� �������");

        cart.Clear();
    }



    private void UpdateBalanceText()
    {
        balanceText.text = balance.ToString();
    }
}
