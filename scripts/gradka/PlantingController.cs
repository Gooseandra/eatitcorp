using UnityEngine;

public class PlantingController : MonoBehaviour
{
    public Inventory inventory;
    public float rayDistance = 2f;
    public LayerMask gardenLayer;
    public Camera playerCamera; // ������ �� ������, ������� � ���������� ��� ����� ���

    void Update()
    {
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        Debug.DrawRay(origin, direction * rayDistance, Color.green);

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPlantSeed(origin, direction);
        }
    }

    void TryPlantSeed(Vector3 origin, Vector3 direction)
    {
        Ray ray = new Ray(origin, direction);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, gardenLayer))
        {
            Debug.Log("������ � ������: " + hit.transform.name);
            GardenBed gardenBed = hit.transform.GetComponent<GardenBed>();
            if (gardenBed == null)
            {
                Debug.LogWarning("������ �� ������!");
                return;
            }

            int selected = inventory.selectedSlot;
            Item item = inventory.hotbar[selected];

            if (item == null || item.growsTo == null || item.amount <= 0)
            {
                Debug.Log("��� ����� � ��������� �����.");
                return;
            }

            if (gardenBed.IsGrowing)
            {
                Debug.Log("������ ��� ������.");
                return;
            }

            // ��������� ���� �������� �� ������
            gardenBed.StartGrowing(item.growsTo, item.timeToGrow);

            // ��������� ���������� �����
            item.amount--;
            if (item.amount <= 0)
            {
                inventory.RemoveItem(item);
            }

            inventory.hotbarUI.UpdateAllSlots();
        }
    }

}
