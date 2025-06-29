using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject[] prefabsToSpawn; // массив префабов
    public Transform spawnPoint;        // точка спавна
    public float spawnInterval = 1f;    // интервал между спавнами (в секундах)

    private void Start()
    {
        InvokeRepeating(nameof(SpawnRandomObject), 0f, spawnInterval);
    }

    private void SpawnRandomObject()
    {
        if (prefabsToSpawn.Length == 0 || spawnPoint == null)
            return;

        // случайный выбор префаба
        int randomIndex = Random.Range(0, prefabsToSpawn.Length);
        GameObject prefab = prefabsToSpawn[randomIndex];

        // создаём объект
        GameObject spawned = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        // присваиваем тег
        spawned.tag = "grabbable";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("grabbable"))
        {
            Destroy(other.gameObject);
        }
    }
}

