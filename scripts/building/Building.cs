using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour
{
    public GameObject[] buildablePrefabs;
    public LayerMask buildableSurface;
    public float maxBuildDistance = 100f;
    public float rotationSpeed = 100f;

    public GameObject buildingPanel;
    public Button[] buildButtons;

    public Material previewMaterial;
    public Material invalidMaterial;

    private int selectedPrefabIndex = -1;
    private GameObject previewObject;
    private int placedObjectsCount = 0;

    [SerializeField] Input_manager input;
    [SerializeField] Movement playerMovement;

    private float rPressedTime = 0f;

    // Новая переменная для точек конвейера
    private Transform[] conveyorPoints;
    private int currentPointIndex = 0;

    private bool wasRotationPressed = false; // Флаг для отслеживания предыдущего состояния

    void Start()
    {
        buildingPanel.SetActive(false);

        for (int i = 0; i < buildButtons.Length; i++)
        {
            int index = i;
            buildButtons[i].onClick.AddListener(() => SelectPrefab(index));

            if (buildablePrefabs[index].GetComponent<Image>() != null)
            {
                Image buttonImage = buildButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = buildablePrefabs[index].GetComponent<Image>().sprite;
                }
            }

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
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            playerMovement.lockCamera = true;
        }
        else
        {
            buildingPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerMovement.lockCamera = false;
        }

        if (selectedPrefabIndex != -1)
        {
            HandleBuilding();

            if (input.GetRotation())
            {
                rPressedTime += Time.deltaTime;
                if (rPressedTime > 0.2f)
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
        selectedPrefabIndex = index;

        input.SetBuildingMode(false);
        buildingPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerMovement.lockCamera = false;

        if (previewObject != null)
        {
            Destroy(previewObject);
        }
        previewObject = Instantiate(buildablePrefabs[selectedPrefabIndex]);

        PreviewMaterial(previewObject, previewMaterial);

        SetCollidersEnabled(previewObject, false);

        SetTriggerEnabled(previewObject, true);

        AddCollisionScriptToPreview();

    }

    private void HandleBuilding()
    {
        if (selectedPrefabIndex == 1)
        {
            // Для конвейеров
            HandleConveyorBuilding();
        }
        else
        {
            // Для других объектов
            HandleGeneralBuilding();
        }
    }

    private void HandleConveyorBuilding()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Debug.DrawRay(ray.origin, ray.direction * maxBuildDistance, Color.red);

        RaycastHit[] hits = Physics.RaycastAll(ray, maxBuildDistance);
        RaycastHit? conveyorHit = null; // Для хранения попадания на конвейер
        RaycastHit? groundHit = null; // Для хранения попадания на землю

        // Проверяем, на что направлен луч
        foreach (var hit in hits)
        {
            // Если находим объект с скриптом ConveyorSegment, запоминаем его
            if (hit.collider.GetComponent<ConveyorSegment>() != null)
            {
                conveyorHit = hit;
            }
            // Если попали в землю (проверяем слой Ground), запоминаем это
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                groundHit = hit;
            }
        }

        // Проверяем, что мы попали на конвейер
        if (conveyorHit.HasValue && previewObject != null)
        {
            RaycastHit hit = conveyorHit.Value;

            float height = previewObject.transform.localScale.y / 2;
            Vector3 position = hit.point + Vector3.up * height;


            ConveyorSegment oldConveuor = hit.collider.GetComponent<ConveyorSegment>();
            UpdateConveyorPosition(oldConveuor);

            // Применяем материал для разрешенного размещения
            PreviewMaterial(previewObject, placedObjectsCount > 0 ? invalidMaterial : previewMaterial);

            // Размещение объекта при клике
            if (Input.GetMouseButtonDown(0) && placedObjectsCount == 0)
            {
                PlaceConveyor(previewObject.transform.position, previewObject.transform.rotation, oldConveuor);
            }
        }
        else if (groundHit.HasValue && previewObject != null)
        {
            // Если не смотрим на конвейер, но смотрим на землю, показываем preview на земле
            RaycastHit hit = groundHit.Value;

            float height = previewObject.transform.localScale.y / 2;
            Vector3 position = hit.point + Vector3.up * height;

            // Обновляем позицию конвейера
            previewObject.transform.position = position;

            // Применяем материал, который показывает, что объект нельзя разместить
            PreviewMaterial(previewObject, invalidMaterial);
        }
        else
        {
            // Если не смотрим на землю или конвейер, просто показываем объект в обычной позиции
            previewObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * maxBuildDistance;

            // Применяем материал, который показывает, что объект нельзя разместить
            PreviewMaterial(previewObject, invalidMaterial);
        }
    }

    private void HandleGeneralBuilding()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Debug.DrawRay(ray.origin, ray.direction * maxBuildDistance, Color.red);

        RaycastHit[] hits = Physics.RaycastAll(ray, maxBuildDistance);
        RaycastHit? buildableHit = null;

        // Проверяем, на что направлен луч
        foreach (var hit in hits)
        {
            // Проверка на наличие скрипта ConveyorSegment на объекте или на слой "Buildable"
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

            // Если нажаты клавиши Alt, округляем позицию
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                position.x = Mathf.Round(position.x);
                position.y = Mathf.Round(position.y);
                position.z = Mathf.Round(position.z);
            }

            // Обновляем позицию стандартным способом
            previewObject.transform.position = position;

            // Применяем материал в зависимости от состояния (можно ли разместить объект)
            PreviewMaterial(previewObject, placedObjectsCount > 0 ? invalidMaterial : previewMaterial);

            // Размещение объекта при клике
            if (Input.GetMouseButtonDown(0) && placedObjectsCount == 0)
            {
                PlaceObject(previewObject.transform.position, previewObject.transform.rotation);
            }
        }
    }
    void PlaceConveyor(Vector3 position, Quaternion rotation, ConveyorSegment conveyorSegment)
    {
        GameObject placedObject = Instantiate(buildablePrefabs[selectedPrefabIndex], position, rotation);
        placedObject.tag = "Placed";
        SetCollidersEnabled(placedObject, true);
        RemoveCollisionScriptFromObject(placedObject);

        Transform roadTransform = placedObject.transform.Find("road");

        if (roadTransform != null)
        {
            // Теперь roadTransform указывает на дочерний объект с именем "road"
            ConveyorSegment cs = roadTransform.GetComponent<ConveyorSegment>();
            switch (currentPointIndex)
            {
                case 0:
                    conveyorSegment.SetNextSegment(cs);
                    break;
                case 1:
                    cs.SetNextSegment(conveyorSegment);
                    break;
                case 2:
                    cs.SetNextSegment(conveyorSegment);
                    break;
                case 3:
                    cs.SetNextSegment(conveyorSegment);
                    break;
                default:
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Объект с именем 'road' не найден среди дочерних объектов.");
        }
    }


    void PlaceObject(Vector3 position, Quaternion rotation)
    {
        GameObject placedObject = Instantiate(buildablePrefabs[selectedPrefabIndex], position, rotation);
        placedObject.tag = "Placed";
        SetCollidersEnabled(placedObject, true);
        RemoveCollisionScriptFromObject(placedObject);
    }

    void SetCollidersEnabled(GameObject obj, bool isEnabled)
    {
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
        {
            collider.enabled = isEnabled;
        }

        foreach (Transform child in obj.transform)
        {
            SetCollidersEnabled(child.gameObject, isEnabled);
        }
    }

    void SetTriggerEnabled(GameObject obj, bool isEnabled)
    {
        Collider triggerCollider = obj.GetComponent<Collider>();
        if (triggerCollider != null && triggerCollider.isTrigger)
        {
            triggerCollider.enabled = isEnabled;
        }
    }

    void PreviewMaterial(GameObject obj, Material material)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }

        foreach (Transform child in obj.transform)
        {
            PreviewMaterial(child.gameObject, material);
        }
    }

    private void UpdateConveyorPosition(ConveyorSegment conveyorSegment)
    {
        // Получаем точки конвейера
        conveyorPoints = conveyorSegment.GetNextPoints();

        if (conveyorPoints.Length > 0)
        {
            // Получаем позицию следующей точки
            previewObject.transform.position = conveyorPoints[currentPointIndex].position;

            // Получаем поворот текущего сегмента
            Quaternion currentSegmentRotation = conveyorSegment.transform.rotation;

            // Применяем поворот относительно текущего сегмента
            if (currentPointIndex == 2 || currentPointIndex == 3)
            {
                previewObject.transform.rotation = currentSegmentRotation * Quaternion.Euler(0f, 90f, 0f);
            }
            else
            {
                if (input.GetRotation()) // Если нажата клавиша для поворота
                {
                    // Отслеживаем прокрутку колеса мыши
                    float scrollInput = Input.GetAxis("Mouse ScrollWheel");

                    if (scrollInput != 0f) // Если прокрутка колесика мыши есть
                    {
                        // Поворот объекта на 90 градусов
                        previewObject.transform.Rotate(0f, 90f * Mathf.Sign(scrollInput), 0f, Space.Self);
                    }
                }
                else
                {
                    previewObject.transform.rotation = currentSegmentRotation * Quaternion.Euler(0f, 0f, 0f);
                }
            }
        }

        // Логика для прокрутки колесика мыши
        if (!input.GetRotation() && Input.GetAxis("Mouse ScrollWheel") > 0f) // Прокрутка колесика вверх
        {
            currentPointIndex = (currentPointIndex + 1) % conveyorPoints.Length;
        }
        else if (!input.GetRotation() && Input.GetAxis("Mouse ScrollWheel") < 0f) // Прокрутка колесика вниз
        {
            currentPointIndex = (currentPointIndex - 1 + conveyorPoints.Length) % conveyorPoints.Length;
        }
    }


    public void IncrementPlacedObjectsCount()
    {
        placedObjectsCount++;
    }

    public void DecrementPlacedObjectsCount()
    {
        placedObjectsCount--;
    }

    private void AddCollisionScriptToPreview()
    {
        if (previewObject != null)
        {
            PreviewObjectCollision collisionScript = previewObject.AddComponent<PreviewObjectCollision>();
            collisionScript.buildingUI = this;
        }
    }

    private void RemoveCollisionScriptFromObject(GameObject obj)
    {
        PreviewObjectCollision collisionScript = obj.GetComponent<PreviewObjectCollision>();
        if (collisionScript != null)
        {
            Destroy(collisionScript);
        }
    }

    private void RotatePreviewObject()
    {
        if (previewObject != null && selectedPrefabIndex != 1)
        {
            if (rPressedTime > 0.3f)
            {
                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
                previewObject.transform.Rotate(0, mouseX, 0, Space.World);
            }
        }
    }
}


