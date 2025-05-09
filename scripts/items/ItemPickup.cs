using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    public int amount = 1;
    public GameObject thisPrefab;

    private void OnValidate()
    {
        // ������������� ������������� ���� Interactable
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }
}