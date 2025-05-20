using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StorageSlotUI : MonoBehaviour
{
    public Image Icon;
    public TMP_Text AmountText;
    public int slotIndex;

    public void SetItem(Item item)
    {
        if (item != null && item.amount > 0)
        {
            Icon.sprite = item.icon;
            Icon.enabled = true;
            AmountText.text = item.amount > 1 ? item.amount.ToString() : "";
        }
        else
        {
            Icon.sprite = null;
            Icon.enabled = false;
            AmountText.text = "";
        }
    }
}