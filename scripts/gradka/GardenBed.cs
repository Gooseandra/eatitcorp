using UnityEngine;

public class GardenBed : MonoBehaviour
{
    public bool IsGrowing { get; private set; }
    public bool IsReadyToHarvest { get; private set; }
    public bool IsReserved { get; set; }
    public GameObject CurrentPlant { get; private set; }

    private GameObject plantPrefab;
    private float growthTime;

    public void StartGrowing(GameObject plantPrefab, float growTime)
    {
        if (IsGrowing || IsReserved) return;

        this.plantPrefab = plantPrefab;
        this.growthTime = growTime;

        IsGrowing = true;
        IsReadyToHarvest = false;
        IsReserved = false;

        CancelInvoke(nameof(OnPlantReady));
        Invoke(nameof(OnPlantReady), growthTime);
    }

    private void OnPlantReady()
    {
        if (!IsGrowing) return;

        CurrentPlant = Instantiate(
            plantPrefab,
            transform.position + Vector3.up * 0.5f,
            Quaternion.identity,
            transform
        );

        // Отключаем физику, чтобы растение не падало
        if (CurrentPlant.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        IsReadyToHarvest = true;
    }

    public GameObject TakePlant()
    {
        if (!IsReadyToHarvest || CurrentPlant == null) return null;

        IsReadyToHarvest = false;
        IsGrowing = false;
        IsReserved = false;

        GameObject plant = CurrentPlant;
        CurrentPlant = null; // Очищаем ссылку, но не удаляем объект!

        return plant;
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }
}