using UnityEngine;

public class MoveInventoryIten : MonoBehaviour
{
    [SerializeField] private HotbarSlotUI[] hotbarSlots;

    private void Start()
    {
        foreach (var slot in hotbarSlots)
        {
            // ������� ��� ���������� ���������, ����� �������� ������������
            slot.button.onClick.RemoveAllListeners();

            // ������ ��������� ����������, ����� �������� ��������� (���� ����������� � �������)
            var currentSlot = slot;

            // ��������� ����������
        }
    }
}
