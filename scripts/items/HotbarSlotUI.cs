using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour
{
    [Header("References")]
    public Image icon;
    public TextMeshProUGUI amountText;
    public GameObject selectionOutline;
    public Button button;
    public InventoryDragController dragController;

    private void Start()
    {
        this.gameObject.SetActive(true);
        dragController = this.GetComponent<InventoryDragController>();
    }

    public void UpdateSlot(bool isSelected, Item item)
    {
        // ��������� ������
        if (item != null && item.amount > 0)
        {
           // dragController.SetIcon(item.icon);
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