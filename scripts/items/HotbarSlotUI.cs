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
        // ��������� ������
        if (item != null && item.amount > 0)
        {
            icon.sprite = item.icon;
            icon.enabled = true;

            // ��������� ���������� (������ ��� ��������� ���������)
            amountText.text = item.maxStack > 1 ? item.amount.ToString() : "";
            amountText.enabled = true;
        }
        else
        {
            icon.enabled = false;
            amountText.enabled = false;
        }

        // ��������� ���������
        selectionOutline.SetActive(isSelected);
    }
}