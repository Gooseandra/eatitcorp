using UnityEngine;

public class PreviewObjectCollision : MonoBehaviour
{
    public Building buildingUI; // ������ �� �������� ������

    private void OnTriggerEnter(Collider other)
    {
        // ���� ������ ����� ��� "Placed", ����������� �������
        if (other.CompareTag("Placed"))
        {
            buildingUI.IncrementPlacedObjectsCount();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ���� ������ ����� ��� "Placed", ��������� �������
        if (other.CompareTag("Placed"))
        {
            buildingUI.DecrementPlacedObjectsCount();
        }
    }
}