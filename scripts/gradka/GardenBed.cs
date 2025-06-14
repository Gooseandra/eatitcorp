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

        // Удаляем предыдущее растение (если было)
        if (CurrentPlant != null)
        {
            Destroy(CurrentPlant);
            CurrentPlant = null;
        }

        // НЕ создаем растение сразу!
        // Только запускаем таймер роста
        CancelInvoke(nameof(OnPlantReady));
        Invoke(nameof(OnPlantReady), growthTime);

        Debug.Log($"Начали выращивать {plantPrefab.name}. Время роста: {growthTime} сек");
    }

    private void OnPlantReady()
    {
        if (!IsGrowing) return;

        // Создаем растение только когда оно созрело
        CurrentPlant = Instantiate(
            plantPrefab,
            transform.position + Vector3.up * 0.5f, // Небольшое смещение вверх
            Quaternion.identity,
            transform
        );

        // Настраиваем физику
        Rigidbody rb = CurrentPlant.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Collider collider = CurrentPlant.GetComponent<Collider>();
        if (collider != null) collider.isTrigger = true;

        IsReadyToHarvest = true;
        Debug.Log($"Растение {plantPrefab.name} созрело!");
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

        Debug.Log($"Урожай собран с грядки {name}");
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }
}