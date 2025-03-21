using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour
{
    public GameObject[] buildablePrefabs; // Массив префабов для строительства
    public LayerMask buildableSurface; // Слой, на котором можно строить
    public float maxBuildDistance = 100f; // Максимальная дистанция для строительства
    public float rotationSpeed = 100f; // Скорость поворота объекта

    public GameObject buildingPanel; // Панель с кнопками для выбора объектов
    public Button[] buildButtons; // Кнопки для выбора объектов

    public Material previewMaterial; // Полупрозрачный синий материал
    public Material invalidMaterial; // Полупрозрачный красный материал

    private int selectedPrefabIndex = -1; // Индекс выбранного префаба
    private GameObject previewObject; // Префаб для предпросмотра
    private int placedObjectsCount = 0; // Счётчик объектов с тэгом "Placed"

    [SerializeField] Input_manager input; // Ссылка на Input_manager
    [SerializeField] Movement playerMovement; // Ссылка на скрипт Movement

    float rPressedTime = 0f;

    void Start()
    {
        // Скрываем панель при старте
        buildingPanel.SetActive(false);

        // Назначаем обработчики для кнопок и обновляем изображения
        for (int i = 0; i < buildButtons.Length; i++)
        {
            int index = i; // Локальная переменная для замыкания
            buildButtons[i].onClick.AddListener(() => SelectPrefab(index));

            // Обновляем изображение на кнопке
            if (buildablePrefabs[index].GetComponent<Image>() != null)
            {
                Image buttonImage = buildButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = buildablePrefabs[index].GetComponent<Image>().sprite;
                }
            }

            // Обновляем текст кнопки
            if (buildButtons[i].transform.childCount > 0)
            {
                Text buttonText = buildButtons[i].transform.GetChild(0).GetComponent<Text>();
                if (buttonText != null)
                {
                    buttonText.text = buildablePrefabs[index].name;
                }
            }
        }
    }

    void Update()
    {
        if (previewObject != null && input.GetEscape())
        {
            DestroyPreviewObject();
        }

        if (input.GetOpenBuildingWindow())
        {
            if (previewObject != null)
            {
                DestroyPreviewObject();
            }
        }

        if (input.GetBuildingMode())
        {

            buildingPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None; // Разблокируем курсор
            Cursor.visible = true;
            playerMovement.lockCamera = true; // Блокируем камеру
        }
        else
        {
            buildingPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked; // Блокируем курсор
            Cursor.visible = false;
            playerMovement.lockCamera = false; // Разблокируем камеру
        }

        // Если выбран объект, показываем предпросмотр
        if (selectedPrefabIndex != -1)
        {
            HandleBuilding();

            // Поворачиваем объект, если зажата клавиша R
            if (input.GetRotation())
            {
                rPressedTime += Time.deltaTime;
                if (rPressedTime > 0.3f)
                {
                    playerMovement.lockCamera = true;
                    RotatePreviewObject();
                }
            }
            else
            {
                if (rPressedTime <= 0.3f && rPressedTime != 0 && previewObject != null)
                {
                    if (previewObject.transform.rotation.y % 45 > 0.5f)
                    {
                        previewObject.transform.rotation = new Quaternion(0, 0, 0, 0);
                    }
                    previewObject.transform.Rotate(0, 45f, 0, Space.World);
                }
                rPressedTime = 0;
                playerMovement.lockCamera = false;
            }
        }
    }

    void DestroyPreviewObject()
    {
        Destroy(previewObject);
        previewObject = null;
        selectedPrefabIndex = -1;
    }

    void SelectPrefab(int index)
    {
        // Выбираем префаб
        selectedPrefabIndex = index;

        // Скрываем панель
        input.SetBuildingMode(false);
        buildingPanel.SetActive(false);

        // Блокируем курсор и разблокируем камеру
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerMovement.lockCamera = false;

        // Создаем объект для предпросмотра
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
        previewObject = Instantiate(buildablePrefabs[selectedPrefabIndex]);

        // Применяем полупрозрачный синий материал к объекту предпросмотра
        ApplyPreviewMaterial(previewObject, previewMaterial);

        // Отключаем коллайдеры у дочерних объектов превью-объекта
        SetCollidersEnabled(previewObject, false);

        // Включаем триггер у родительского объекта
        SetTriggerEnabled(previewObject, true);

        // Добавляем скрипт для отслеживания коллизий
        AddCollisionScriptToPreview();
    }

    private void HandleBuilding()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Debug.DrawRay(ray.origin, ray.direction * maxBuildDistance, Color.red);

        RaycastHit[] hits = Physics.RaycastAll(ray, maxBuildDistance);
        RaycastHit? buildableHit = null;

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Buildable"))
            {
                buildableHit = hit;
                break;
            }
        }

        if (buildableHit.HasValue && previewObject != null)
        {
            RaycastHit hit = buildableHit.Value;
            float height = previewObject.transform.localScale.y / 2;
            Vector3 position = hit.point + Vector3.up * height;

            // Если зажат Alt — округляем координаты
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                position.x = Mathf.Round(position.x / 0.25f) * 0.25f;
                position.y = Mathf.Round(position.y / 0.25f) * 0.25f;
                position.z = Mathf.Round(position.z / 0.25f) * 0.25f;
            }

            previewObject.transform.position = position;

            ApplyPreviewMaterial(previewObject, placedObjectsCount > 0 ? invalidMaterial : previewMaterial);

            if (Input.GetMouseButtonDown(0) && placedObjectsCount == 0)
            {
                PlaceObject(position, previewObject.transform.rotation);
            }
        }
    }

    void PlaceObject(Vector3 position, Quaternion rotation)
    {
        // Создаем объект в мире
        GameObject placedObject = Instantiate(buildablePrefabs[selectedPrefabIndex], position, rotation);

        // Присваиваем объекту тэг "Placed"
        placedObject.tag = "Placed";

        // Включаем коллайдеры у дочерних объектов
        SetCollidersEnabled(placedObject, true);

        // Удаляем скрипт для отслеживания коллизий
        RemoveCollisionScriptFromObject(placedObject);

        // Сбрасываем выбранный префаб
        //selectedPrefabIndex = -1;

        // Уничтожаем объект предпросмотра
        //if (previewObject != null)
        //{
        //    Destroy(previewObject);
        //}
    }

    // Включает или отключает коллайдеры у объекта и всех его дочерних объектов
    void SetCollidersEnabled(GameObject obj, bool isEnabled)
    {
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null && !collider.isTrigger) // Игнорируем триггеры
        {
            collider.enabled = isEnabled;
        }

        // Рекурсивно обрабатываем дочерние объекты
        foreach (Transform child in obj.transform)
        {
            SetCollidersEnabled(child.gameObject, isEnabled);
        }
    }

    // Включает или отключает триггер у родительского объекта
    void SetTriggerEnabled(GameObject obj, bool isEnabled)
    {
        Collider triggerCollider = obj.GetComponent<Collider>();
        if (triggerCollider != null && triggerCollider.isTrigger)
        {
            triggerCollider.enabled = isEnabled;
        }
    }

    // Применяет материал к объекту и всем его дочерним объектам
    void ApplyPreviewMaterial(GameObject obj, Material material)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }

        // Рекурсивно обрабатываем дочерние объекты
        foreach (Transform child in obj.transform)
        {
            ApplyPreviewMaterial(child.gameObject, material);
        }
    }

    // Увеличивает счётчик объектов с тэгом "Placed"
    public void IncrementPlacedObjectsCount()
    {
        placedObjectsCount++;
    }

    // Уменьшает счётчик объектов с тэгом "Placed"
    public void DecrementPlacedObjectsCount()
    {
        placedObjectsCount--;
    }

    // Добавляет скрипт для отслеживания коллизий на превью-объект
    private void AddCollisionScriptToPreview()
    {
        if (previewObject != null)
        {
            PreviewObjectCollision collisionScript = previewObject.AddComponent<PreviewObjectCollision>();
            collisionScript.buildingUI = this;
        }
    }

    // Удаляет скрипт для отслеживания коллизий с объекта
    private void RemoveCollisionScriptFromObject(GameObject obj)
    {
        PreviewObjectCollision collisionScript = obj.GetComponent<PreviewObjectCollision>();
        if (collisionScript != null)
        {
            Destroy(collisionScript);
        }
    }

    // Поворачивает превью-объект при зажатии клавиши R и движении мыши
    private void RotatePreviewObject()
    {
        if (previewObject != null) 
        {
            if (rPressedTime > 0.3f)
            {
                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
                previewObject.transform.Rotate(0, mouseX, 0, Space.World);
            }
        }
    }
}