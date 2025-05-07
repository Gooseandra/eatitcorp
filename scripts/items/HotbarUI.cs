using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class HotbarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;

    [Header("Slot References")]
    [SerializeField] private List<GameObject> slotOutlines = new List<GameObject>();
    [SerializeField] private List<Image> slotIcons = new List<Image>();
    [SerializeField] private List<TextMeshProUGUI> slotAmountTexts = new List<TextMeshProUGUI>();

    private void Start()
    {
        InitializeUIReferences();
        UpdateAllSlots();
    }

    private void Update()
    {
        // Можно добавить проверку изменений для оптимизации
        UpdateAllSlots();
    }

    private void InitializeUIReferences()
    {
        // Автоматически находим все элементы UI если не заданы вручную
        if (slotOutlines.Count == 0)
        {
            for (int i = 0; i < 10; i++)
            {
                Transform slot = transform.GetChild(i);
                slotOutlines.Add(slot.Find("SelectedOutline").gameObject);
                slotIcons.Add(slot.Find("Icon").GetComponent<Image>());
                slotAmountTexts.Add(slot.Find("AmountText").GetComponent<TextMeshProUGUI>());
            }
        }
    }

    public void UpdateAllSlots()
    {
        if (inventory == null) return;

        for (int i = 0; i < 10; i++)
        {
            UpdateSlotVisuals(i);
        }
    }

    private void UpdateSlotVisuals(int slotIndex)
    {
        // Безопасное обращение к элементам
        if (slotIndex >= slotOutlines.Count ||
            slotIndex >= slotIcons.Count ||
            slotIndex >= slotAmountTexts.Count) return;

        // Получаем информацию о предмете
        Item item = inventory.hotbar[slotIndex];
        bool hasItem = item != null && item.amount > 0;

        // Обновляем обводку выбранного слота
        if (slotIndex < slotOutlines.Count && slotOutlines[slotIndex] != null)
        {
            slotOutlines[slotIndex].SetActive(slotIndex == inventory.selectedSlot);
        }

        // Обновляем иконку
        if (slotIndex < slotIcons.Count && slotIcons[slotIndex] != null)
        {
            slotIcons[slotIndex].gameObject.SetActive(hasItem);
            if (hasItem)
            {
                slotIcons[slotIndex].sprite = item.icon;
                slotIcons[slotIndex].preserveAspect = true;
            }
        }

        // Обновляем текст количества
        if (slotIndex < slotAmountTexts.Count && slotAmountTexts[slotIndex] != null)
        {
            Debug.Log(item.amount);
            slotAmountTexts[slotIndex].text = hasItem && item.maxStack > 1 ? item.amount.ToString() : "";
        }
    }
}