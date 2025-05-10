using UnityEngine;

public class ItemInteractor : MonoBehaviour
{
    public float pickupDistance = 3f;
    public Inventory inventory;
    public Camera playerCamera;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupItem();
        }
    }

    private void TryPickupItem()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // ”бираем проверку по слою, оставл€€ только рассто€ние
        if (Physics.Raycast(ray, out hit, pickupDistance))
        {
            ItemPickup itemPickup = hit.collider.GetComponent<ItemPickup>();
            if (itemPickup != null)
            {
                Item itemCopy = new Item(itemPickup.item.name, itemPickup.amount, itemPickup.item.icon, itemPickup.thisPrefab);
                bool success = inventory.AddItem(itemCopy);
                Debug.Log(success);
                if (success)
                {
                    itemPickup.gameObject.SetActive(false);
                }
            }
        }
    }
}