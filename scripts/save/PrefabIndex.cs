using UnityEngine;

public class PrefabIndex : MonoBehaviour
{
    public int index;

    private void Start()
    {
        ItemPickup item = GetComponent<ItemPickup>();
        if (item != null)
        {
            item.item.prefabIndex = index;
        }
    }
}