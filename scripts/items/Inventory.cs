using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public List<Item> hotbar = new List<Item>();
    public int selectedSlot = 0;
    // public Transform Player;


    bool lastVal = false;

    private void Awake()
    {
        InitializeHotbar();
    }

    private void Update()
    {
        bool val = Input.GetKey(KeyCode.Q);
        if (val && lastVal != val)
        {
            DropItem();
        }
        lastVal = val;
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
        Debug.Log(newItem.thisPrefab);
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
                    hotbar[i].thisPrefab = newItem.thisPrefab;
                    Debug.Log(newItem.thisPrefab);
                    Debug.Log(hotbar[i].thisPrefab);
                    return true;
                }
                else
                {
                    hotbar[i].amount = hotbar[i].maxStack;
                    newItem.amount -= spaceLeft;
                    hotbar[i].thisPrefab = newItem.thisPrefab;
                }
            }
        }

        // Добавляем в пустой слот
        for (int i = 0; i < hotbar.Count; i++)
        {
            if (hotbar[i].thisPrefab == null)
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

    private void DropItem()
    {
        if (hotbar[selectedSlot].amount == 0)
        {
            return;
        }

        GameObject newItem = Instantiate(hotbar[selectedSlot].thisPrefab);
       // Vector3 dropDirection = Player.forward + Vector3.up * 0.3f;
       // newItem.GetComponent<Rigidbody>().AddForce(dropDirection * 100, ForceMode.Impulse);

        hotbar[selectedSlot].amount--;
    }

}