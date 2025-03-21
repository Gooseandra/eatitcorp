using UnityEngine;

public class PreviewObjectCollision : MonoBehaviour
{
    public Building buildingUI; // Ссылка на основной скрипт

    private void OnTriggerEnter(Collider other)
    {
        // Если объект имеет тэг "Placed", увеличиваем счётчик
        if (other.CompareTag("Placed"))
        {
            buildingUI.IncrementPlacedObjectsCount();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Если объект имеет тэг "Placed", уменьшаем счётчик
        if (other.CompareTag("Placed"))
        {
            buildingUI.DecrementPlacedObjectsCount();
        }
    }
}