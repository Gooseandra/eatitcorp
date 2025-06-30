using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class Inventory : MonoBehaviour
{
    public List<Item> hotbar = new List<Item>();
    public int selectedSlot = 0;
    public Transform Player;
    public HotbarUI hotbarUI;

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
        hotbar = new List<Item>();
        for (int i = 0; i < 10; i++)
        {
            hotbar.Add(null);
        }
    }

    public bool AddItem(Item newItem)
    {
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

        for (int i = 0; i < hotbar.Count; i++)
        {
            if (hotbar[i] == null || hotbar[i].thisPrefab == null)
            {
                hotbar[i] = newItem;
                return true;
            }
        }

        return false;
    }

    public void SelectSlot(int slotIndex)
    {
        selectedSlot = Mathf.Clamp(slotIndex, 0, hotbar.Count - 1);
    }

    private void DropItem()
    {
        Item currentItem = hotbar[selectedSlot];
        if (currentItem == null || currentItem.amount == 0) return;

        GameObject newItem = Instantiate(currentItem.thisPrefab, Player.position, Quaternion.identity);
        newItem.SetActive(true);
        Vector3 dropDirection = Player.forward + Vector3.up;
        Rigidbody rb = newItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.AddForce(dropDirection * 0.2f * Time.deltaTime, ForceMode.Impulse);
        }

        currentItem.amount--;
    }

    public int FindByBuildingIndex(int index)
    {
        for (int i = 0; i < 10; i++)
        {
            if (hotbar[i] != null && hotbar[i].thisPrefab != null && hotbar[i].buildingIndex == index)
            {
                return i;
            }
        }
        return -1;
    }

    public int GetCountByIndex(int index)
    {
        return hotbar[index].amount;
    }

    public void RemoveByIndex(int index)
    {
        hotbar[index].amount--;
    }

    public void RemoveItem(Item itemToRemove)
    {
        for (int i = 0; i < hotbar.Count; i++)
        {
            if (hotbar[i] == itemToRemove)
            {
                hotbar[i] = null;
                hotbarUI.UpdateAllSlots();
                return;
            }
        }
    }

    [System.Serializable]
    public class SavedInventoryItem
    {
        public int prefabIndex;
        public int amount;
    }

    [System.Serializable]
    public class SavedInventoryWrapper
    {
        public List<SavedInventoryItem> items;
    }

    public void SaveInventory(SaveManager manager)
    {
        List<SavedInventoryItem> saved = new List<SavedInventoryItem>();
        foreach (var item in hotbar)
        {
            if (item != null && item.amount > 0)
            {
                saved.Add(new SavedInventoryItem
                {
                    prefabIndex = item.prefabIndex,
                    amount = item.amount
                });
            }
            else
            {
                saved.Add(null);
            }
        }

        string json = JsonUtility.ToJson(new SavedInventoryWrapper { items = saved });
        File.WriteAllText(Application.persistentDataPath + "/inventory.json", json);
        Debug.Log("Inventory saved to " + Application.persistentDataPath + "/inventory.json");
    }

    public void LoadInventory(SaveManager manager)
    {
        string path = Application.persistentDataPath + "/inventory.json";
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        SavedInventoryWrapper wrapper = JsonUtility.FromJson<SavedInventoryWrapper>(json);

        hotbar.Clear();
        foreach (var saved in wrapper.items)
        {
            if (saved == null)
            {
                hotbar.Add(null);
            }
            else
            {
                hotbar.Add(Item.CreateFromPrefabIndex(saved.prefabIndex, saved.amount, manager));
            }
        }

        if (hotbarUI != null)
            hotbarUI.UpdateAllSlots();
    }
}
