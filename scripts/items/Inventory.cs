using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public List<Item> hotbar = new List<Item>();
    public int selectedSlot = 0;

    private void Awake()
    {
        InitializeHotbar();
    }

    private void InitializeHotbar()
    {
        // Создаем 10 пустых слотов
        hotbar = new List<Item>();
        for (int i = 0; i < 10; i++)
        {
            hotbar.Add(null);
        }
    }

    public bool AddItem(Item newItem)
    {
        // Попробуем добавить к существующему стеку
        for (int i = 0; i < hotbar.Count; i++)
        {
            if (hotbar[i] != null &&
                hotbar[i].name == newItem.name &&
                hotbar[i].amount < hotbar[i].maxStack)
            {
                int spaceLeft = hotbar[i].maxStack - hotbar[i].amount;
                if (newItem.amount <= spaceLeft)
                {
                    hotbar[i].amount += newItem.amount;
                    return true;
                }
                else
                {
                    hotbar[i].amount = hotbar[i].maxStack;
                    newItem.amount -= spaceLeft;
                }
            }
        }

        // Добавляем в пустой слот
        for (int i = 0; i < hotbar.Count; i++)
        {
            if (hotbar[i] == null)
            {
                hotbar[i] = newItem;
                return true;
            }
        }

        return false; // Нет свободных слотов
    }

    public void SelectSlot(int slotIndex)
    {
        selectedSlot = Mathf.Clamp(slotIndex, 0, hotbar.Count - 1);
    }

}