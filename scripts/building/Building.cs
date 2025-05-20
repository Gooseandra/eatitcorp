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

    // ����� ���������� ��� ����� ���������
    private Transform[] conveyorPoints;
    private int currentPointIndex = 0;
    [SerializeField] UIManager ManagerUI;

    public Inventory inventory;

    private bool lastBuildingMode = false;

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
        // ��������� ������� ���������� ����� ��������� preview
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

        // ��������� ��� ���������� (������� � ��������)
        SetAllCollidersEnabled(previewObject, false);

        // ��������� preview material
        PreviewMaterial(previewObject, previewMaterial);

        // ��������� ������ ��� ��������� �������� (���� �����)
        AddCollisionScriptToPreview();
    }

    private void HandleBuilding()
    {
        // ��������� ������� ���������� ����� ��������������
        int hotbarIndex = inventory.FindByBuildingIndex(selectedPrefabIndex);
        if (hotbarIndex == -1)
        {
            // ���� ��������� ����������� - ���������� preview
            DestroyPreviewObject();
            return;
        }

        int materialsAmount = inventory.GetCountByIndex(hotbarIndex);
        if (materialsAmount <= 0)
        {
            // ���� ��������� ����������� - ���������� preview
            DestroyPreviewObject();
            return;
        }

        // ��������� �������� preview
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
            float height = previewObject.transform.localScale.y / 2;
            Vector3 position = hit.point + Vector3.up * height;

            ConveyorSegment oldConveuor = hit.collider.GetComponent<ConveyorSegment>();
            UpdateConveyorPosition(oldConveuor);

            // ���������� ������� ������ ���� ���� ���������
            if (Input.GetMouseButtonDown(0) && placedObjectsCount == 0 && materialsAmount > 0)
            {
                PlaceConveyor(previewObject.transform.position, previewObject.transform.rotation, oldConveuor, hotbarIndex);
            }
        }
        else if (groundHit.HasValue && previewObject != null)
        {
            // ���� �� ������� �� ��������, �� ������� �� �����, ���������� preview �� �����
            RaycastHit hit = groundHit.Value;

            float height = previewObject.transform.localScale.y / 2;
            Vector3 position = hit.point + Vector3.up * height;

            // ��������� ������� ���������
            previewObject.transform.position = position;

            // ��������� ��������, ������� ����������, ��� ������ ������ ����������
            PreviewMaterial(previewObject, invalidMaterial);
        }
        else
        {
            // ���� �� ������� �� ����� ��� ��������, ������ ���������� ������ � ������� �������
            previewObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * maxBuildDistance;

            // ��������� ��������, ������� ����������, ��� ������ ������ ����������
            PreviewMaterial(previewObject, invalidMaterial);
        }
    }

    private void HandleGeneralBuilding(int hotbarIndex, int materialsAmount)
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

            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                position.x = Mathf.Round(position.x);
                position.y = Mathf.Round(position.y);
                position.z = Mathf.Round(position.z);
            }

            previewObject.transform.position = position;

            // ���������� ������� ������ ���� ���� ���������
            if (Input.GetMouseButtonDown(0) && placedObjectsCount == 0 && materialsAmount > 0)
            {
                PlaceObject(previewObject.transform.position, previewObject.transform.rotation, hotbarIndex);
            }
        }
    }
    void PlaceConveyor(Vector3 position, Quaternion rotation, ConveyorSegment conveyorSegment, int hotbarIndex)
    {
        GameObject placedObject = Instantiate(buildablePrefabs[selectedPrefabIndex], position, rotation);
        placedObject.tag = "Placed";
        SetAllCollidersEnabled(placedObject, true);
        RemoveCollisionScriptFromObject(placedObject);

        Transform roadTransform = placedObject.transform.Find("road");

        if (roadTransform != null)
        {
            // ������ roadTransform ��������� �� �������� ������ � ������ "road"
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
            inventory.RemoveByIndex(hotbarIndex);
            SetAllCollidersEnabled(placedObject, true);
            SetTriggerEnabled(placedObject, false);

            // ���������� ����������� ���������
        }
        else
        {
            Debug.LogWarning("������ � ������ 'road' �� ������ ����� �������� ��������.");
        }
    }


    void PlaceObject(Vector3 position, Quaternion rotation, int hotbarIndex)
    {
        GameObject placedObject = Instantiate(buildablePrefabs[selectedPrefabIndex], position, rotation);
        placedObject.tag = "Placed";

        // �������� ������ ������� ���������� (�������� �������� ������������)
        SetAllCollidersEnabled(placedObject, true); // ������� ����������
        SetTriggerEnabled(placedObject, false);  // �������� (���� �� �����)

        // ���������� ����������� ��������� (������� preview)

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
        // �������� ����� ���������
        conveyorPoints = conveyorSegment.GetNextPoints();

        if (conveyorPoints.Length > 0)
        {
            // �������� ������� ��������� �����
            previewObject.transform.position = conveyorPoints[currentPointIndex].position;

            // �������� ������� �������� ��������
            Quaternion currentSegmentRotation = conveyorSegment.transform.rotation;

            // ��������� ������� ������������ �������� ��������
            if (currentPointIndex == 2 || currentPointIndex == 3)
            {
                previewObject.transform.rotation = currentSegmentRotation * Quaternion.Euler(0f, 90f, 0f);
            }
            else
            {
                if (input.GetRotation()) // ���� ������ ������� ��� ��������
                {
                    // ����������� ��������� ������ ����
                    float scrollInput = Input.GetAxis("Mouse ScrollWheel");

                    if (scrollInput != 0f) // ���� ��������� �������� ���� ����
                    {
                        // ������� ������� �� 90 ��������
                        previewObject.transform.Rotate(0f, 90f * Mathf.Sign(scrollInput), 0f, Space.Self);
                    }
                }
                else
                {
                    previewObject.transform.rotation = currentSegmentRotation * Quaternion.Euler(0f, 0f, 0f);
                }
            }
        }

        // ������ ��� ��������� �������� ����
        if (!input.GetRotation() && Input.GetAxis("Mouse ScrollWheel") > 0f) // ��������� �������� �����
        {
            currentPointIndex = (currentPointIndex + 1) % conveyorPoints.Length;
        }
        else if (!input.GetRotation() && Input.GetAxis("Mouse ScrollWheel") < 0f) // ��������� �������� ����
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


