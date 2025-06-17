using UnityEngine;

public class HarvestorPortOut : MonoBehaviour
{
    public Transform spawnPoint;
    public float unloadDelay = 0.5f;
    public float checkRadius = 0.5f; // Радиус проверки
    public int blockingLayer = 7; // Какой слой блокирует спавн (по умолчанию 7)

    private void OnTriggerStay(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.layer == blockingLayer)
        {
            // Если точка спавна занята другим объектом с тем же слоем — отмена
            if (IsSpawnPointBlocked())
            {
                Debug.Log("Spawn point is blocked by another object on layer " + blockingLayer);
                return;
            }

            // Освобождаем объект от робота
            HarvestRobot robot = other.transform.parent.GetComponent<HarvestRobot>();
            if (robot != null)
            {
                robot.carriedPlant = null;
            }

            // Переносим объект в точку спавна и включаем физику
            other.transform.parent = null;
            other.transform.position = spawnPoint.position;

            CapsuleCollider col = other.GetComponent<CapsuleCollider>();
            if (col != null) col.isTrigger = false;

            MeshRenderer renderer = other.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = true;

            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }
    }


    // Проверяет, есть ли в точке спавна другие объекты с blockingLayer
    private bool IsSpawnPointBlocked()
    {
        Collider[] colliders = Physics.OverlapSphere(spawnPoint.position, checkRadius);
        foreach (Collider col in colliders)
        {
            // Если объект на нужном слое и это не текущий триггер — точка занята
            if (col.gameObject.layer == blockingLayer && col.gameObject != this.gameObject)
            {
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(spawnPoint.position, 0.2f);
            Gizmos.DrawLine(transform.position, spawnPoint.position);

            // Отображаем зону проверки
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Оранжевый с прозрачностью
            Gizmos.DrawSphere(spawnPoint.position, checkRadius);
        }
    }
}