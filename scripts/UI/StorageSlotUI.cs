using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class StorageSlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;
    public TextMeshProUGUI amountText;
    private Storage storage;
    private int slotIndex;

    public void Setup(Storage storageRef, int index)
    {
        storage = storageRef;
        slotIndex = index;
    }

    public void SetItem(Item item)
    {
        if (item != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
            amountText.text = item.amount > 1 ? item.amount.ToString() : "";
        }
        else
        {
            iconImage.enabled = false;
            amountText.text = "";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (InventoryDragController.Instance.IsDragging())
            {
                Item dragged = InventoryDragController.Instance.GetDraggedItem();
                if (dragged != null)
                {
                    Item copy = new Item(dragged.name, dragged.amount, dragged.maxStack, dragged.icon, dragged.thisPrefab, dragged.buildingIndex);
                    if (storage.AddItem(copy))
                    {
                        storage.playerInventory.RemoveItem(dragged);
                        InventoryDragController.Instance.CancelDrag();
                        storage.UpdateStorageUI();
                        storage.playerInventory.hotbarUI.UpdateAllSlots();
                    }
                }
            }
            else
            {
                storage.TransferEntireItemToInventory(slotIndex);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            storage.TransferOneItemToInventory(slotIndex);
        }
    }
}
