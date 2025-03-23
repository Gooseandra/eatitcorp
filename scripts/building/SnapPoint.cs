using UnityEngine;

public class SnapPoint : MonoBehaviour
{
    // ������, ������� ����� ������ ��� ����
    public GameObject entrancePrefab;

    // ��� ��� ����������
    private const int conveyorLayer = 6;

    [SerializeField] private int ingredientLayer;

    // ����� ��� ��������� �����
    private Vector3 snapPosition;

    // ���� ��� �������� ����������� ���������
    private bool isConveyorConnected = false;

    private Mixer mixer; // ������ �� ������������ ������

    private void Start()
    {
        // ������������� ������� ������������ ������
        mixer = GetComponentInParent<Mixer>();
        if (mixer == null)
        {
            Debug.LogError("������������ ������ �� ������!", this);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.layer);
        Debug.Log(ingredientLayer);
        // ���������, ��� ������ �� ���� Conveyor � ��� �� ��������� ��������
        if (other.gameObject.layer == conveyorLayer && !isConveyorConnected)
        {
            snapPosition = other.transform.position;
            CreateEntrance(other);
            isConveyorConnected = true; // ������������� ����, ��� �������� ���������
        }
        else if (mixer != null && (other.gameObject.layer == ingredientLayer))
        {
            mixer.HandleIngredient(other);
        }
    }

    private void CreateEntrance(Collider conveyor)
    {
        if (entrancePrefab != null)
        {
            // ��������� ������ ����������� �� ���������� � ���������
            Vector3 directionToConveyor = (conveyor.transform.position - transform.position).normalized;

            // ������� ������ "����"
            GameObject entrance = Instantiate(entrancePrefab, transform.position, Quaternion.identity);

            // ������ ���� �������� �������� ����������
            entrance.transform.SetParent(transform);

            // ��������� Y ���������� � ������� ������ X � Z �� 0.7 ������ � ������� ���������
            entrance.transform.localPosition = new Vector3(
                entrance.transform.localPosition.x + directionToConveyor.x * 0.7f,
                entrance.transform.localPosition.y, // ��������� y ��� ���������
                entrance.transform.localPosition.z + directionToConveyor.z * 0.7f
            );
        }
        else
        {
            Debug.LogError("Entrance prefab is not assigned!");
        }
    }

    // ����� ��� ������������ ���������, ���� ��� ���������� (��������, ��� ������ �����)
    public void DisconnectConveyor()
    {
        isConveyorConnected = false;
    }
}
