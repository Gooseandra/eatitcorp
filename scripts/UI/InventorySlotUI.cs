using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public TMP_Text amountText;
    public Image selectionHighlight;

    private Inventory inventory;
    private int slotIndex;

    private void Awake()
    {
        inventory = GetComponentInParent<Inventory>();
        slotIndex = transform.GetSiblingIndex();
    }

    public void UpdateSlot(Item item, bool selected)
    {
        selectionHighlight.enabled = selected;

        if (item != null && item.amount > 0)
        {
            icon.sprite = item.icon;
            icon.enabled = true;
            amountText.text = item.amount > 1 ? item.amount.ToString() : "";
        }
        else
        {
            icon.sprite = null;
            icon.enabled = false;
            amountText.text = "";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        inventory.SelectSlot(slotIndex);
    }
}