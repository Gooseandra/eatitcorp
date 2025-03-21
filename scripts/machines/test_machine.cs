using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_machine : MonoBehaviour
{
    [SerializeField] int max_items;                // ������������ ���������� ��������� � ������
    [SerializeField] int items_in_machine = 0;     // ������� ���������� ��������� � ������
    [SerializeField] int item_layer;                // ���� ���������
    [SerializeField] int items_to_craft;           // ���������� ��������� ��� ������
    [SerializeField] float time_to_craft;           // ����� ������ ������ ��������
    [SerializeField] GameObject prodict;            // ������, ������� ����� ������
    [SerializeField] Transform product_spawn_point; // ����� ��������� ��������

    Animator am;
    bool isCrafting = false;                        // ����, �����������, ������ �� ������� ������
    Queue<GameObject> waitingItems = new Queue<GameObject>(); // ������� ��� �������� ��������� ���������

    private void Start()
    {
        am = GetComponent<Animator>();
    }

    private void Craft()
    {
        // ���������, ���������� �� ��������� � �� ��������� �� ���
        if (items_in_machine >= items_to_craft && !isCrafting)
        {
            isCrafting = true; // ������������� ���� ������
            StartCoroutine(ProcessCrafting());
        }
    }

    IEnumerator ProcessCrafting()
    {
        // ��������� �������� ������
        am.Play("Craft");

        // ���� ����� ������
        yield return new WaitForSeconds(time_to_craft);

        // ������� ���� �������
        Instantiate(prodict, product_spawn_point.position, product_spawn_point.rotation);

        // ��������� ���������� ��������� � ������ �� ����������� ����������
        items_in_machine -= items_to_craft;

        // ���������� �������� � ��������� "Idle" ������ ���� ������ �� ���������
        if (!isCrafting)
        {
            am.Play("Idle");
        }

        // ���������� ���� ������
        isCrafting = false;

        // ���������, ����� �� ������� ������� �� �������
        ProcessWaitingItems();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ���������, �������� �� ������ "���������" (�� ����)
        if (collision.gameObject.layer == item_layer)
        {
            if (items_in_machine < max_items) // ���������, ���� �� ����� � ������
            {
                // ���������� �������, ��� ��� �� ���������� � ������
                Destroy(collision.gameObject);
                items_in_machine++; // ����������� ������� ��������� � ������

                // ���������, ����� �� ������ �����
                Craft();
            }
            else
            {
                // ���� ������ �����������, ��������� ������� � ������� ��������
                waitingItems.Enqueue(collision.gameObject);
            }
        }
    }

    private void ProcessWaitingItems()
    {
        // ���������, ����� �� ������� ������� �� �������
        while (waitingItems.Count > 0 && items_in_machine < max_items)
        {
            GameObject waitingItem = waitingItems.Dequeue(); // ��������� ������ ������� �� �������
            Destroy(waitingItem); // ���������� �������, ��� ��� �� ���������� � ������
            items_in_machine++; // ����������� ������� ��������� � ������
        }

        // ����� ��������� ��������� ���������, ���������, ����� �� ������ �����
        Craft(); // ���������, ����� �� ����� ������ �����
    }
}
