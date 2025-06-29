using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [Header("���������")]
    public GameObject[] prefabsToSpawn; // ������ ��������
    public Transform spawnPoint;        // ����� ������
    public float spawnInterval = 1f;    // �������� ����� �������� (� ��������)

    private void Start()
    {
        InvokeRepeating(nameof(SpawnRandomObject), 0f, spawnInterval);
    }

    private void SpawnRandomObject()
    {
        if (prefabsToSpawn.Length == 0 || spawnPoint == null)
            return;

        // ��������� ����� �������
        int randomIndex = Random.Range(0, prefabsToSpawn.Length);
        GameObject prefab = prefabsToSpawn[randomIndex];

        // ������ ������
        GameObject spawned = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        // ����������� ���
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

