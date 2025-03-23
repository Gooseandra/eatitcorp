using UnityEngine;

public class SnapPoint : MonoBehaviour
{
    // Объект, который будет создан как вход
    public GameObject entrancePrefab;

    // Тег для конвейеров
    private const int conveyorLayer = 6;

    [SerializeField] private int ingredientLayer;

    // Точка для появления входа
    private Vector3 snapPosition;

    // Флаг для проверки подключения конвейера
    private bool isConveyorConnected = false;

    private Mixer mixer; // Ссылка на родительский миксер

    private void Start()
    {
        // Автоматически находим родительский миксер
        mixer = GetComponentInParent<Mixer>();
        if (mixer == null)
        {
            Debug.LogError("Родительский миксер не найден!", this);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.layer);
        Debug.Log(ingredientLayer);
        // Проверяем, что объект на слое Conveyor и еще не подключен конвейер
        if (other.gameObject.layer == conveyorLayer && !isConveyorConnected)
        {
            snapPosition = other.transform.position;
            CreateEntrance(other);
            isConveyorConnected = true; // Устанавливаем флаг, что конвейер подключен
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
            // Вычисляем вектор направления от коннектора к конвейеру
            Vector3 directionToConveyor = (conveyor.transform.position - transform.position).normalized;

            // Создаем объект "вход"
            GameObject entrance = Instantiate(entrancePrefab, transform.position, Quaternion.identity);

            // Делаем вход дочерним объектом коннектора
            entrance.transform.SetParent(transform);

            // Сохраняем Y координату и смещаем только X и Z на 0.7 единиц в сторону конвейера
            entrance.transform.localPosition = new Vector3(
                entrance.transform.localPosition.x + directionToConveyor.x * 0.7f,
                entrance.transform.localPosition.y, // оставляем y без изменений
                entrance.transform.localPosition.z + directionToConveyor.z * 0.7f
            );
        }
        else
        {
            Debug.LogError("Entrance prefab is not assigned!");
        }
    }

    // Метод для отсоединения конвейера, если это необходимо (например, для сброса флага)
    public void DisconnectConveyor()
    {
        isConveyorConnected = false;
    }
}
