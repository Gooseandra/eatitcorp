using UnityEngine;

public class PrefabIndex : MonoBehaviour
{
    public int index;

    private void Start()
    {
        GetComponent<ItemPickup>().item.prefabIndex = index;
    }
}