using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour
{
    [Header("References")]
    public Image icon;
    public TextMeshProUGUI amountText;
    public GameObject selectionOutline;

    public void UpdateSlot(bool isSelected, Item item)
    {
        // Обновляем иконку
        if (item != null && item.amount > 0)
        {
            icon.sprite = item.icon;
            icon.enabled = true;

            // Обновляем количество (только для стакаемых предметов)
            amountText.text = item.maxStack > 1 ? item.amount.ToString() : "";
            amountText.enabled = true;
        }
        else
        {
            icon.enabled = false;
            amountText.enabled = false;
        }

        // Обновляем выделение
        selectionOutline.SetActive(isSelected);
    }
}