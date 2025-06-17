using UnityEngine;

public class HarvestorPortOut : MonoBehaviour
{
    public Transform spawnPoint;
    public float unloadDelay = 0.5f;
    public float checkRadius = 0.5f; // ������ ��������
    public int blockingLayer = 7; // ����� ���� ��������� ����� (�� ��������� 7)

    private void OnTriggerStay(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.layer == blockingLayer)
        {
            // ���� ����� ������ ������ ������ �������� � ��� �� ����� � ������
            if (IsSpawnPointBlocked())
            {
                Debug.Log("Spawn point is blocked by another object on layer " + blockingLayer);
                return;
            }

            // ����������� ������ �� ������
            HarvestRobot robot = other.transform.parent.GetComponent<HarvestRobot>();
            if (robot != null)
            {
                robot.carriedPlant = null;
            }

            // ��������� ������ � ����� ������ � �������� ������
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


    // ���������, ���� �� � ����� ������ ������ ������� � blockingLayer
    private bool IsSpawnPointBlocked()
    {
        Collider[] colliders = Physics.OverlapSphere(spawnPoint.position, checkRadius);
        foreach (Collider col in colliders)
        {
            // ���� ������ �� ������ ���� � ��� �� ������� ������� � ����� ������
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

            // ���������� ���� ��������
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // ��������� � �������������
            Gizmos.DrawSphere(spawnPoint.position, checkRadius);
        }
    }
}