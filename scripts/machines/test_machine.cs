using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_machine : MonoBehaviour
{
    [SerializeField] int max_items;                // Максимальное количество предметов в машине
    [SerializeField] int items_in_machine = 0;     // Текущее количество предметов в машине
    [SerializeField] int item_layer;                // Слой предметов
    [SerializeField] int items_to_craft;           // Количество предметов для крафта
    [SerializeField] float time_to_craft;           // Время крафта одного предмета
    [SerializeField] GameObject prodict;            // Объект, который будет создан
    [SerializeField] Transform product_spawn_point; // Точка появления продукта

    Animator am;
    bool isCrafting = false;                        // Флаг, указывающий, ведётся ли процесс крафта
    Queue<GameObject> waitingItems = new Queue<GameObject>(); // Очередь для хранения ожидающих предметов

    private void Start()
    {
        am = GetComponent<Animator>();
    }

    private void Craft()
    {
        // Проверяем, достаточно ли предметов и не крафтится ли уже
        if (items_in_machine >= items_to_craft && !isCrafting)
        {
            isCrafting = true; // Устанавливаем флаг крафта
            StartCoroutine(ProcessCrafting());
        }
    }

    IEnumerator ProcessCrafting()
    {
        // Запускаем анимацию крафта
        am.Play("Craft");

        // Ждем время крафта
        yield return new WaitForSeconds(time_to_craft);

        // Создаем один предмет
        Instantiate(prodict, product_spawn_point.position, product_spawn_point.rotation);

        // Уменьшаем количество предметов в машине на необходимое количество
        items_in_machine -= items_to_craft;

        // Возвращаем анимацию в состояние "Idle" только если больше не крафтится
        if (!isCrafting)
        {
            am.Play("Idle");
        }

        // Сбрасываем флаг крафта
        isCrafting = false;

        // Проверяем, можем ли забрать предмет из очереди
        ProcessWaitingItems();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Проверяем, является ли объект "предметом" (по слою)
        if (collision.gameObject.layer == item_layer)
        {
            if (items_in_machine < max_items) // Проверяем, есть ли место в машине
            {
                // Уничтожаем предмет, так как он забирается в машину
                Destroy(collision.gameObject);
                items_in_machine++; // Увеличиваем счетчик предметов в машине

                // Проверяем, нужно ли начать крафт
                Craft();
            }
            else
            {
                // Если машина переполнена, добавляем предмет в очередь ожидания
                waitingItems.Enqueue(collision.gameObject);
            }
        }
    }

    private void ProcessWaitingItems()
    {
        // Проверяем, можем ли забрать предмет из очереди
        while (waitingItems.Count > 0 && items_in_machine < max_items)
        {
            GameObject waitingItem = waitingItems.Dequeue(); // Извлекаем первый предмет из очереди
            Destroy(waitingItem); // Уничтожаем предмет, так как он забирается в машину
            items_in_machine++; // Увеличиваем счетчик предметов в машине
        }

        // После обработки ожидающих предметов, проверяем, нужно ли начать крафт
        Craft(); // Проверяем, нужно ли снова начать крафт
    }
}
