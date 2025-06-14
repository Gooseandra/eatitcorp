using UnityEngine;

public class GardenBed : MonoBehaviour
{
    public bool IsGrowing { get; private set; }
    public bool IsReadyToHarvest { get; private set; }
    public bool IsReserved { get; set; }
    public GameObject CurrentPlant { get; private set; }

    private GameObject plantPrefab;
    private float growthTime;
    private bool isBeingHarvested;

    public void StartGrowing(GameObject plantPrefab, float growTime)
    {
        if (IsGrowing || isBeingHarvested || IsReserved) return;

        this.plantPrefab = plantPrefab;
        this.growthTime = growTime;

        IsGrowing = true;
        IsReadyToHarvest = false;
        IsReserved = false;
        isBeingHarvested = false;

        // ������� ���������� �������� (���� ����)
        if (CurrentPlant != null)
        {
            Destroy(CurrentPlant);
            CurrentPlant = null;
        }

        // �� ������� �������� �����!
        // ������ ��������� ������ �����
        CancelInvoke(nameof(OnPlantReady));
        Invoke(nameof(OnPlantReady), growthTime);

        Debug.Log($"������ ���������� {plantPrefab.name}. ����� �����: {growthTime} ���");
    }

    private void OnPlantReady()
    {
        if (!IsGrowing) return;

        // ������� �������� ������ ����� ��� �������
        CurrentPlant = Instantiate(
            plantPrefab,
            transform.position + Vector3.up * 0.5f, // ��������� �������� �����
            Quaternion.identity,
            transform
        );

        // ����������� ������
        Rigidbody rb = CurrentPlant.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Collider collider = CurrentPlant.GetComponent<Collider>();
        if (collider != null) collider.isTrigger = true;

        IsReadyToHarvest = true;
        Debug.Log($"�������� {plantPrefab.name} �������!");
    }

    public void Harvest()
    {
        if (!IsReadyToHarvest || isBeingHarvested) return;

        isBeingHarvested = true;

        if (CurrentPlant != null)
        {
            Destroy(CurrentPlant);
            CurrentPlant = null;
        }

        IsReadyToHarvest = false;
        IsGrowing = false;
        IsReserved = false;
        isBeingHarvested = false;

        Debug.Log($"������ ������ � ������ {name}");
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }
}