using UnityEngine;
using System.Collections;

public class GardenBed : MonoBehaviour
{
    private GameObject growingPlant;
    private float growthTime;

    public bool IsGrowing => growingPlant != null;

    public void StartGrowing(GameObject plantPrefab, float timeToGrow)
    {
        if (IsGrowing)
        {
            Debug.LogWarning("Грядка уже занята растением!");
            return;
        }

        growthTime = timeToGrow;
        StartCoroutine(GrowPlant(plantPrefab));
    }

    private IEnumerator GrowPlant(GameObject plantPrefab)
    {
        // Ждем время роста
        yield return new WaitForSeconds(growthTime);

        // Создаем растение и делаем его дочерним объектом грядки
        growingPlant = Instantiate(plantPrefab, transform.position + Vector3.up * 0.1f, Quaternion.identity);
        growingPlant.transform.SetParent(transform);

        Debug.Log("Растение выросло!");
    }
}
