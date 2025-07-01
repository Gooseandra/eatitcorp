using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

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

    private Transform[] conveyorPoints;
    private int currentPointIndex = 0;
    [SerializeField] MessageManager ManagerUI;

    public Inventory inventory;

    private bool lastBuildingMode = false;

    [SerializeField] float step = 1f;

    private Transform conveyorBackPoint;
    private ConveyorSegment lastHitConveyor = null;

    private bool isDeleteMode = false;
    private float deleteHoldTime = 1f;
    private float currentDeleteTimer = 0f;
    private GameObject objectToDelete = null;

    [SerializeField] GameObject[] allSavePrefabs;
    [SerializeField] private Material highlightMaterial;

    // Для хранения исходных материалов
    private GameObject lastHighlightedObject = null;
    private readonly Dictionary<Renderer, Material[]> originalMaterials = new();

    [SerializeField] private GameObject aim;
    private Animator aimAnimator;
    private bool isDeletingAnimPlaying = false;


    void Start()
    {
        aimAnimator = aim.GetComponent<Animator>();
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
        if (Input.GetKeyDown(KeyCode.F))
        {
            isDeleteMode = !isDeleteMode;

            if (isDeleteMode)
            {
                Debug.Log("Delete Mode Enabled");
                PlayDeleteAnimation("Delete");
            }
            else
            {
                ResetHighlight();
                Debug.Log("Delete Mode Disabled");
                currentDeleteTimer = 0f;
                objectToDelete = null;
                PlayDeleteAnimation("DeleteDeactivation");
            }
        }


        if (isDeleteMode)
        {
            HandleDeleteMode();
        }

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

        if (input.GetBuildingMode() != lastBuildingMode)
        {
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
        }
        lastBuildingMode = input.GetBuildingMode();

        if (selectedPrefabIndex != -1)
        {
            HandleBuilding();

            if (input.GetRotation())
            {
                rPressedTime += Time.deltaTime;
                if (selectedPrefabIndex == 1 && rPressedTime > 0.1f)
                {
                    Vector3 pivot = new Vector3(conveyorBackPoint.position.x, conveyorBackPoint.position.y, conveyorBackPoint.position.z); // мировая позиция до вращения
                    playerMovement.lockCamera = true;
                    float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
                    if (mouseScroll != 0)
                    {
                        float rotationAngle = mouseScroll * 10f;

                        Vector3 localRotationAxis = previewObject.transform.forward; // Или .up, в зависимости от модели

                        previewObject.transform.RotateAround(pivot, localRotationAxis, rotationAngle);
                        previewObject.transform.position = new Vector3(previewObject.transform.position.x, previewObject.transform.position.y, previewObject.transform.position.z);
                        Debug.Log($"Rotated conveyor around BackPoint by {rotationAngle} degrees, position corrected");
                    }
                    else
                    {
                        Debug.LogWarning("BackPoint not found on previewObject!");
                    }
                    Debug.DrawRay(pivot, Vector3.right * 2f, Color.red, 2f); // Ось вращения (X)
                    Debug.DrawRay(pivot, previewObject.transform.right * 2f, Color.blue, 2f); // Ось объекта
                }
                else if (rPressedTime > 0.2f && selectedPrefabIndex != 1)
                {
                    playerMovement.lockCamera = true;
                    RotatePreviewObject();
                }
            }
            else
            {
                if (rPressedTime <= 0.3f && rPressedTime != 0 && previewObject != null && selectedPrefabIndex != 1)
                {
                    previewObject.transform.Rotate(0, 45f, 0, Space.World);
                }
                rPressedTime = 0;
                playerMovement.lockCamera = false;

                // Поворот конвейера на 90 градусов при прокрутке колесика
                if (selectedPrefabIndex == 1 && previewObject != null)
                {
                    float scroll = Input.GetAxis("Mouse ScrollWheel");
                    if (scroll != 0)
                    {
                        conveyorBackPoint.SetParent(previewObject.transform);

                        Debug.Log($"Mouse scroll: {scroll}");
                        // Фиксированный поворот на 90 градусов
                        float currentY = previewObject.transform.rotation.eulerAngles.y;
                        float newY = Mathf.Round((currentY + Mathf.Sign(scroll) * 90f) / 90f) * 90f;
                        previewObject.transform.rotation = Quaternion.Euler(
                            previewObject.transform.rotation.eulerAngles.x,
                            newY,
                            previewObject.transform.rotation.eulerAngles.z
                        );
                        Debug.Log($"Rotated conveyor to Y angle: {newY}");

                        Vector3 worldPosition = previewObject.transform.TransformPoint(new Vector3(-0.5f, 0, 0));
                        conveyorBackPoint.SetParent(null);
                        conveyorBackPoint.position = worldPosition;
                    }
                }
            }
        }
    }


    void DestroyPreviewObject()
    {
        Destroy(previewObject);
        if (conveyorBackPoint != null)
        {
            Destroy(conveyorBackPoint.gameObject);
        }
        previewObject = null;
        selectedPrefabIndex = -1;
        lastHitConveyor = null;
    }

    void SelectPrefab(int index)
    {
        // Проверяем наличие материалов перед созданием preview
        int hotbarIndex = inventory.FindByBuildingIndex(index);
        if (hotbarIndex == -1 || inventory.GetCountByIndex(hotbarIndex) <= 0)
        {
            ManagerUI.ShowNotEnoughMaterialMessage();
            return;
        }

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

        if (selectedPrefabIndex == 1)
        {
            conveyorBackPoint = previewObject.transform.Find("BackPoint");
            conveyorBackPoint.SetParent(null);
        }

        // Отключаем ВСЕ коллайдеры (обычные и триггеры)
        SetAllCollidersEnabled(previewObject, false);

        // Применяем preview material
        PreviewMaterial(previewObject, previewMaterial);

        // Добавляем скрипт для обработки коллизий (если нужен)
        AddCollisionScriptToPreview();
    }

    private void HandleBuilding()
    {
        // Проверяем наличие материалов перед строительством
        int hotbarIndex = inventory.FindByBuildingIndex(selectedPrefabIndex);
        if (hotbarIndex == -1)
        {
            // Если материалы закончились - уничтожаем preview
            DestroyPreviewObject();
            return;
        }

        int materialsAmount = inventory.GetCountByIndex(hotbarIndex);
        if (materialsAmount <= 0)
        {
            // Если материалы закончились - уничтожаем preview
            DestroyPreviewObject();
            return;
        }

        // Применяем материал preview
        PreviewMaterial(previewObject, previewMaterial);

        if (selectedPrefabIndex == 1)
        {
            HandleConveyorBuilding(hotbarIndex, materialsAmount);
        }
        else
        {
            HandleGeneralBuilding(hotbarIndex, materialsAmount);
        }
    }

    private void HandleConveyorBuilding(int hotbarIndex, int materialsAmount)
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Debug.DrawRay(ray.origin, ray.direction * maxBuildDistance, Color.red);

        RaycastHit[] hits = Physics.RaycastAll(ray, maxBuildDistance);
        RaycastHit? conveyorHit = null;
        RaycastHit? groundHit = null;

        foreach (var hit in hits)
        {
            if (hit.collider.GetComponent<ConveyorSegment>() != null)
            {
                conveyorHit = hit;
            }
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                groundHit = hit;
            }
        }

        if (conveyorHit.HasValue && previewObject != null)
        {
            RaycastHit hit = conveyorHit.Value;
            ConveyorSegment conveyorSegment = hit.collider.GetComponent<ConveyorSegment>();

            // Обновляем позицию конвейера относительно предыдущего сегмента
            UpdateConveyorPosition(conveyorSegment);

            // Размещение объекта только если есть материалы
            if (Input.GetMouseButtonDown(0) && placedObjectsCount == 0 && materialsAmount > 0)
            {
                PlaceConveyor(previewObject.transform.position, previewObject.transform.rotation, conveyorSegment, hotbarIndex);
            }
        }
        else if (groundHit.HasValue && previewObject != null)
        {
            lastHitConveyor = null;
            RaycastHit hit = groundHit.Value;
            float height = previewObject.transform.localScale.y / 2;
            Vector3 position = hit.point + Vector3.up * height;
            previewObject.transform.position = position;
            PreviewMaterial(previewObject, invalidMaterial);
        }
        else
        {
            previewObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * maxBuildDistance;
            PreviewMaterial(previewObject, invalidMaterial);
        }
    }

    private void DeleteWays(GameObject placedObject)
    {
        Transform wayTransform = placedObject.transform.Find("way");
        if (wayTransform != null)
        {
            Destroy(wayTransform.gameObject);
            Debug.Log("Removed 'way' child object after placement");
        }
    }

    private void HandleGeneralBuilding(int hotbarIndex, int materialsAmount)
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        // Рисуем луч для визуализации в сцене (красный цвет)
        Debug.DrawRay(ray.origin, ray.direction * maxBuildDistance, Color.red);

        if (Physics.Raycast(ray, out hit, maxBuildDistance, buildableSurface))
        {
            bool isValidSurface = hit.collider.CompareTag("Buildable") && hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground");

            // Проверка ограничений для selectedPrefabIndex 4 и 5
            Debug.Log(hit.collider.gameObject.layer);
            if ((selectedPrefabIndex == 4 || selectedPrefabIndex == 5) && hit.collider.gameObject.layer != LayerMask.NameToLayer("Gradka"))
            {
                isValidSurface = false;
            }
            else
            {
                isValidSurface = true;
            }

            Vector3 position = hit.point;
            position.y += GetLowestPointOffset(previewObject);
            position.x = Mathf.Round(position.x / step) * step;
            position.z = Mathf.Round(position.z / step) * step;
            previewObject.transform.position = position;

            PreviewMaterial(previewObject, isValidSurface ? previewMaterial : invalidMaterial);

            if (isValidSurface && Input.GetMouseButtonDown(0) && placedObjectsCount == 0 && materialsAmount > 0)
            {
                PlaceObject(position, previewObject.transform.rotation, hotbarIndex);
            }
        }
        else
        {
            // Объект вне допустимой зоны
            previewObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * maxBuildDistance;
            PreviewMaterial(previewObject, invalidMaterial);
        }
    }


    void PlaceConveyor(Vector3 position, Quaternion rotation, ConveyorSegment conveyorSegment, int hotbarIndex)
    {
        GameObject placedObject = Instantiate(buildablePrefabs[selectedPrefabIndex], position, rotation);
        DeleteWays(placedObject);
        placedObject.tag = "Placed";
        SetAllCollidersEnabled(placedObject, true);
        RemoveCollisionScriptFromObject(placedObject);

        Transform roadTransform = placedObject.transform.Find("road");

        if (roadTransform != null)
        {
            ConveyorSegment newSegment = roadTransform.GetComponent<ConveyorSegment>();

            // Устанавливаем связь между сегментами
            if (currentPointIndex == 0)
            {
                conveyorSegment.SetNextSegment(newSegment);
            }
            else
            {
                newSegment.SetNextSegment(conveyorSegment);
            }

            inventory.RemoveByIndex(hotbarIndex);
            SetAllCollidersEnabled(placedObject, true);
            SetTriggerEnabled(placedObject, false);
        }
        else
        {
            Debug.LogWarning("Объект с именем 'road' не найден среди дочерних объектов.");
        }
    }


    void PlaceObject(Vector3 position, Quaternion rotation, int hotbarIndex)
    {
        GameObject placedObject = Instantiate(buildablePrefabs[selectedPrefabIndex], position, rotation);
        placedObject.tag = "Placed";
        DeleteWays(placedObject);
        // Включаем только обычные коллайдеры (триггеры остаются выключенными)
        SetAllCollidersEnabled(placedObject, true); // Обычные коллайдеры
        SetTriggerEnabled(placedObject, false);  // Триггеры (если не нужны)

        // Возвращаем стандартные материалы (убираем preview)

        inventory.RemoveByIndex(hotbarIndex);
    }

    void SetAllCollidersEnabled(GameObject obj, bool isEnabled)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            collider.enabled = isEnabled;
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
        if (conveyorSegment != lastHitConveyor)
        {
            lastHitConveyor = conveyorSegment;

            conveyorPoints = conveyorSegment.GetNextPoints();

            if (conveyorPoints.Length > 0)
            {
                // Сохраняем текущую точку соединения
                Vector3 connectionPoint = conveyorPoints[currentPointIndex].position;

                // Позиционируем preview на выбранной точке
                previewObject.transform.position = connectionPoint;
            }

            // Обновляем позицию BackPoint только один раз при смене сегмента
            if (conveyorBackPoint != null)
            {
                conveyorBackPoint.SetParent(previewObject.transform);
                Vector3 worldPosition = previewObject.transform.TransformPoint(new Vector3(-0.5f, 0, 0));
                conveyorBackPoint.SetParent(null);
                conveyorBackPoint.position = worldPosition;
            }
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

    private float GetLowestPointOffset(GameObject obj)
    {
        float lowestY = float.MaxValue;

        foreach (var filter in obj.GetComponentsInChildren<MeshFilter>())
        {
            Mesh mesh = filter.sharedMesh;
            if (mesh == null) continue;

            foreach (var vertex in mesh.vertices)
            {
                Vector3 worldPoint = filter.transform.TransformPoint(vertex);
                lowestY = Mathf.Min(lowestY, worldPoint.y);
            }
        }

        return obj.transform.position.y - lowestY;
    }

    private void HandleDeleteMode()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, maxBuildDistance))
        {
            GameObject target = hit.collider.gameObject;

            // Если объект под курсором больше не тот, что был подсвечен — сбрасываем старый
            if (target != lastHighlightedObject)
            {
                ResetHighlight();
            }

            // Подсвечиваем только "Placed"
            if (target.CompareTag("Placed"))
            {
                HighlightObject(target);

                if (Input.GetMouseButton(0))
                {
                    if (objectToDelete == target)
                    {
                        currentDeleteTimer += Time.deltaTime;

                        if (currentDeleteTimer >= deleteHoldTime)
                        {
                            if (ReturnItemToInventory(target))
                            {
                                Destroy(target);
                                Debug.Log("Object deleted and item returned to inventory.");
                            }
                            else
                            {
                                Debug.Log("Cannot add item to inventory.");
                            }

                            currentDeleteTimer = 0f;
                            objectToDelete = null;
                            ResetHighlight();
                        }
                    }
                    else
                    {
                        objectToDelete = target;
                        currentDeleteTimer = 0f;
                    }
                }
                else
                {
                    currentDeleteTimer = 0f;
                    objectToDelete = null;
                }
            }
            else
            {
                ResetHighlight(); // <- навели на не-удаляемый объект — сброс
                objectToDelete = null;
                currentDeleteTimer = 0f;
            }
        }
        else
        {
            ResetHighlight(); // <- ничего не попали — сброс
            currentDeleteTimer = 0f;
            objectToDelete = null;
        }
    }



    private bool ReturnItemToInventory(GameObject obj)
    {
        string objName = obj.name.Replace("(Clone)", "").Trim();

        int prefabIndex = -1;
        for (int i = 0; i < buildablePrefabs.Length; i++)
        {
            if (buildablePrefabs[i].name == objName)
            {
                prefabIndex = i;
                break;
            }
        }

        if (prefabIndex != -1)
        {
            return AddByBuildingIndex(prefabIndex, 1);
        }
        else
        {
            Debug.LogWarning($"Couldn't find prefab match for {objName}, not returned to inventory.");
            return false;
        }
    }

    public bool AddByBuildingIndex(int buildingIndex, int amount)
    {
        GameObject go = FindPrefabByBuildingIndex(buildingIndex);
        ItemPickup item = go.GetComponent<ItemPickup>();
        return inventory.AddItem(item.item);
    }

    public GameObject FindPrefabByBuildingIndex(int index)
    {
        foreach (var prefab in allSavePrefabs)
        {
            ItemPickup pickup = prefab.GetComponent<ItemPickup>();
            if (pickup != null && pickup.item != null && pickup.item.buildingIndex == index)
            {
                return prefab;
            }
        }
        return null;
    }

    private void HighlightObject(GameObject target)
    {
        if (target == lastHighlightedObject) return;

        ResetHighlight(); // сбрасываем прошлый

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                originalMaterials[renderer] = renderer.sharedMaterials;

                Material[] highlightMats = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < highlightMats.Length; i++)
                    highlightMats[i] = highlightMaterial;

                renderer.materials = highlightMats;
            }
        }

        lastHighlightedObject = target;
    }

    private void ResetHighlight()
    {
        foreach (var pair in originalMaterials)
        {
            if (pair.Key != null)
                pair.Key.materials = pair.Value;
        }

        originalMaterials.Clear();
        lastHighlightedObject = null;
    }

    private Coroutine deleteAnimCoroutine;

    private void PlayDeleteAnimation(string animationName)
    {
        if (aimAnimator == null) return;

        if (deleteAnimCoroutine != null)
        {
            StopCoroutine(deleteAnimCoroutine);
        }

        deleteAnimCoroutine = StartCoroutine(PlayAnimationCoroutine(animationName));
    }

    private IEnumerator PlayAnimationCoroutine(string animationName)
    {
        AnimationClip clip = null;
        foreach (var c in aimAnimator.runtimeAnimatorController.animationClips)
        {
            if (c.name == animationName)
            {
                clip = c;
                break;
            }
        }

        if (clip == null)
        {
            Debug.LogWarning($"Animation clip '{animationName}' not found!");
            yield break;
        }

        aimAnimator.speed = 1f;
        aimAnimator.Play(animationName, 0, 0f);

        yield return new WaitForSeconds(clip.length);

        aimAnimator.speed = 0f;

        if (animationName == "Delete")
        {
            // Оставляем на последнем кадре
            aimAnimator.Play("DeleteDeactivation", 0, 1f);
        }
        else if (animationName == "DeleteDeactivation")
        {
            // Оставляем на последнем кадре анимации деактивации
            aimAnimator.Play("Delete", 0, 1f);
        }

        deleteAnimCoroutine = null;
    }


}