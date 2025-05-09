using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    public Item item;
    public int amount;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;
        spriteRenderer.sprite = item.icon;
    }
}