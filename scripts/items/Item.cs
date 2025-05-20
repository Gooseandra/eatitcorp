using UnityEngine;

[System.Serializable]
public class Item
{
    public string name;
    public int maxStack = 50;
    public int amount;
    public Sprite icon;
    public GameObject thisPrefab;
    public int buildingIndex;

    public Item(string name, int amount, Sprite icon, GameObject prefab, int buildingIndex)
    {
        this.name = name;
        this.amount = amount;
        this.icon = icon;
        this.thisPrefab = prefab;
        this.buildingIndex = buildingIndex;
    }

    public bool CanStack()
    {
        return amount < maxStack;
    }
}