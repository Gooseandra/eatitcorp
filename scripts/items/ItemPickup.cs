using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    public int amount = 1;

    private void OnValidate()
    {
        // Автоматически устанавливаем слой Interactable
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }
}