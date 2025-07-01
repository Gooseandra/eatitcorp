using UnityEngine;

[System.Serializable]
public class Item
{
    public GameObject growsTo;
    public float timeToGrow = 0f;
    public string name;
    public int maxStack = 50;
    public int amount;
    public Sprite icon;
    public GameObject thisPrefab;
    public int buildingIndex;
    public int prefabIndex;

    public Item(string name, int amount, int maxStack, Sprite icon, GameObject prefab, int buildingIndex, GameObject growsTo, float timeToGrow)
    {
        this.name = name;
        this.amount = amount;
        this.icon = icon;
        this.thisPrefab = prefab;
        this.buildingIndex = buildingIndex;
        this.maxStack = maxStack;
        this.growsTo = growsTo;
        this.timeToGrow = timeToGrow;
        prefabIndex = prefab.GetComponent<PrefabIndex>().index;
    }
    public bool CanStack()
    {
        return amount < maxStack;
    }

    public static Item CreateFromPrefabIndex(int index, int amount, SaveManager manager)
    {
        GameObject prefab = manager.GetPrefabByIndex(index);
        if (prefab == null)
        {
            Debug.LogWarning($"[Item] Prefab not found for index {index}");
            return null;
        }

        ItemPickup item = prefab.GetComponent<ItemPickup>();
        if (item == null)
        {
            Debug.LogWarning($"[Item] No Item component found on prefab with index {index}");
            return null;
        }

        return new Item(
            item.name,
            amount,
            item.item.maxStack,
            item.item.icon,
            prefab,
            index,
            item.item.growsTo,
            item.item.timeToGrow
        );
    }

[System.Serializable]
    public class SavedInventoryItem
    {
        public int prefabIndex;
        public int amount;
    }

}