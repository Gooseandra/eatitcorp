using UnityEngine;

public class PlantingController : MonoBehaviour
{
    public Inventory inventory;
    public float rayDistance = 2f;
    public LayerMask gardenLayer;
    public Camera playerCamera;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPlantSeed();
        }
    }

    void TryPlantSeed()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, gardenLayer))
        {
            GardenBed gardenBed = hit.transform.GetComponent<GardenBed>();
            if (gardenBed == null) return;

            int selected = inventory.selectedSlot;
            Item item = inventory.hotbar[selected];

            if (item == null || item.growsTo == null || item.amount <= 0) return;
            if (gardenBed.IsGrowing) return;

            Debug.Log("pc: " + item.timeToGrow);
            gardenBed.StartGrowing(item.growsTo, item.timeToGrow);
            item.amount--;

            if (item.amount <= 0)
            {
                inventory.RemoveItem(item);
            }

            inventory.hotbarUI.UpdateAllSlots();
        }
    }
}