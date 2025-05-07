using UnityEngine;

[System.Serializable]
public class Item
{
    public string name;
    public int maxStack = 50;
    public int amount;
    public Sprite icon;

    public Item(string name, int amount, Sprite icon)
    {
        this.name = name;
        this.amount = amount;
        this.icon = icon;
    }

    public bool CanStack()
    {
        return amount < maxStack;
    }
}