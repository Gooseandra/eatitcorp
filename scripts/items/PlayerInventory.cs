using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("References")]
    public Inventory inventory;
    public HotbarUI hotbarUI;

    private void Start()
    {
        InitializeComponents();
        SelectSlot(0); // Выбираем первый слот по умолчанию
    }

    private void InitializeComponents()
    {
        if (inventory == null) inventory = GetComponent<Inventory>();
        if (hotbarUI == null) hotbarUI = GetComponent<HotbarUI>();
    }

    private void Update()
    {
        HandleHotbarSelection();
    }

    private void HandleHotbarSelection()
    {
        // Выбор цифрами 1-0
        for (int i = 0; i < 10; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
                return;
            }
        }

        // Прокрутка колесом
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            int direction = scroll > 0 ? -1 : 1;
            SelectNextValidSlot(direction);
        }
    }

    private void SelectNextValidSlot(int direction)
    {
        int attempts = 0;
        int newSlot = inventory.selectedSlot;

        while (attempts < 10)
        {
            newSlot = (newSlot + direction + 10) % 10;
            attempts++;

            if (IsSlotValid(newSlot))
            {
                SelectSlot(newSlot);
                return;
            }
        }
    }

    private bool IsSlotValid(int slotIndex)
    {
        if (inventory == null || slotIndex < 0 || slotIndex >= inventory.hotbar.Count)
            return false;

        Item item = inventory.hotbar[slotIndex];
        return item != null && item.amount > 0;
    }

    public void SelectSlot(int slotIndex)
    {
        if (inventory == null) return;

        inventory.SelectSlot(slotIndex);

        // Принудительное обновление UI
        if (hotbarUI != null) hotbarUI.UpdateAllSlots();
    }
}