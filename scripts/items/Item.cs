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
    }

    public bool CanStack()
    {
        return amount < maxStack;
    }
}