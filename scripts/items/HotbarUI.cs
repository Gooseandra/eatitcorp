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
    [SerializeField] private InventoryDragController dragController;

    private void Start()
    {
        InitializeUIReferences();
        UpdateAllSlots();
    }

    private void Update()
    {
        // Можно добавить проверку изменений для оптимизации
        UpdateAllSlots();
        AssignSlotClickHandlers();
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

    private void AssignSlotClickHandlers()
    {
        // Проходим по всем слотам (дочерним объектам)
        for (int i = 0; i < transform.childCount; i++)
        {
            int slotIndex = i; // важный момент — копируем индекс для замыкания

            Transform slotTransform = transform.GetChild(i);
            Button button = slotTransform.GetComponent<Button>();
            Image icon = slotTransform.Find("Icon")?.GetComponent<Image>();

            if (button == null || icon == null)
            {
                Debug.LogWarning($"Слот {i} не содержит Button или Icon");
                continue;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                // Берём предмет из инвентаря по индексу
                Item item = inventory.hotbar[slotIndex];

                // Проверяем есть ли предмет и активна ли иконка
                if (item != null && item.amount > 0 && icon.enabled && icon.sprite != null)
                {
                    dragController.StartDrag(icon, item);
                }
            });
        }
    }

    public void UpdateAllSlots()
    {
        if (inventory == null) return;

        int slotCount = Mathf.Min(
            inventory.hotbar.Count,
            slotOutlines.Count,
            slotIcons.Count,
            slotAmountTexts.Count,
            10 // максимум 10 слотов
        );

        for (int i = 0; i < slotCount; i++)
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
            //slotIcons[slotIndex].gameObject.SetActive(hasItem);
            if (hasItem)
            {
                slotIcons[slotIndex].sprite = item.icon;
                slotIcons[slotIndex].preserveAspect = true;
                slotIcons[slotIndex].enabled = item.icon != null;
            }
            else
            {
                slotIcons[slotIndex].enabled = false;
            }
        }

        // Обновляем текст количества
        if (slotIndex < slotAmountTexts.Count && slotAmountTexts[slotIndex] != null)
        {
            slotAmountTexts[slotIndex].text = hasItem && item.maxStack > 1 ? item.amount.ToString() : "";
        }
    }
}