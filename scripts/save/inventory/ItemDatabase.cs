using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    private static Dictionary<int, Item> itemsByIndex = new Dictionary<int, Item>();

    // »нициализаци€ базы Ч нужно вызвать один раз, перед использованием
    public static void Initialize(List<Item> allItems)
    {
        itemsByIndex.Clear();
        foreach (var item in allItems)
        {
            if (!itemsByIndex.ContainsKey(item.buildingIndex))
                itemsByIndex.Add(item.buildingIndex, item);
        }
    }

    public static GameObject GetPrefabByBuildingIndex(int index)
    {
        if (itemsByIndex.TryGetValue(index, out var item))
            return item.thisPrefab;
        return null;
    }

    public static Sprite GetIconByBuildingIndex(int index)
    {
        if (itemsByIndex.TryGetValue(index, out var item))
            return item.icon;
        return null;
    }

    public static Item GetItemByBuildingIndex(int index)
    {
        if (itemsByIndex.TryGetValue(index, out var item))
            return item;
        return null;
    }
}
