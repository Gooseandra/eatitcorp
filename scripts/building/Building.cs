using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour
{
    public GameObject[] buildablePrefabs; // ������ �������� ��� �������������
    public LayerMask buildableSurface; // ����, �� ������� ����� �������
    public float maxBuildDistance = 100f; // ������������ ��������� ��� �������������
    public float rotationSpeed = 100f; // �������� �������� �������

    public GameObject buildingPanel; // ������ � �������� ��� ������ ��������
    public Button[] buildButtons; // ������ ��� ������ ��������

    public Material previewMaterial; // �������������� ����� ��������
    public Material invalidMaterial; // �������������� ������� ��������

    private int selectedPrefabIndex = -1; // ������ ���������� �������
    private GameObject previewObject; // ������ ��� �������������
    private int placedObjectsCount = 0; // ������� �������� � ����� "Placed"

    [SerializeField] Input_manager input; // ������ �� Input_manager
    [SerializeField] Movement playerMovement; // ������ �� ������ Movement

    float rPressedTime = 0f;

    void Start()
    {
        // �������� ������ ��� ������
        buildingPanel.SetActive(false);

        // ��������� ����������� ��� ������ � ��������� �����������
        for (int i = 0; i < buildButtons.Length; i++)
        {
            int index = i; // ��������� ���������� ��� ���������
            buildButtons[i].onClick.AddListener(() => SelectPrefab(index));

            // ��������� ����������� �� ������
            if (buildablePrefabs[index].GetComponent<Image>() != null)
            {
                Image buttonImage = buildButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = buildablePrefabs[index].GetComponent<Image>().sprite;
                }
            }

            // ��������� ����� ������
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
            Cursor.lockState = CursorLockMode.None; // ������������ ������
            Cursor.visible = true;
            playerMovement.lockCamera = true; // ��������� ������
        }
        else
        {
            buildingPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked; // ��������� ������
            Cursor.visible = false;
            playerMovement.lockCamera = false; // ������������ ������
        }

        // ���� ������ ������, ���������� ������������
        if (selectedPrefabIndex != -1)
        {
            HandleBuilding();

            // ������������ ������, ���� ������ ������� R
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
        // �������� ������
        selectedPrefabIndex = index;

        // �������� ������
        input.SetBuildingMode(false);
        buildingPanel.SetActive(false);

        // ��������� ������ � ������������ ������
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerMovement.lockCamera = false;

        // ������� ������ ��� �������������
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
        previewObject = Instantiate(buildablePrefabs[selectedPrefabIndex]);

        // ��������� �������������� ����� �������� � ������� �������������
        ApplyPreviewMaterial(previewObject, previewMaterial);

        // ��������� ���������� � �������� �������� ������-�������
        SetCollidersEnabled(previewObject, false);

        // �������� ������� � ������������� �������
        SetTriggerEnabled(previewObject, true);

        // ��������� ������ ��� ������������ ��������
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

            // ���� ����� Alt � ��������� ����������
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
        // ������� ������ � ����
        GameObject placedObject = Instantiate(buildablePrefabs[selectedPrefabIndex], position, rotation);

        // ����������� ������� ��� "Placed"
        placedObject.tag = "Placed";

        // �������� ���������� � �������� ��������
        SetCollidersEnabled(placedObject, true);

        // ������� ������ ��� ������������ ��������
        RemoveCollisionScriptFromObject(placedObject);

        // ���������� ��������� ������
        //selectedPrefabIndex = -1;

        // ���������� ������ �������������
        //if (previewObject != null)
        //{
        //    Destroy(previewObject);
        //}
    }

    // �������� ��� ��������� ���������� � ������� � ���� ��� �������� ��������
    void SetCollidersEnabled(GameObject obj, bool isEnabled)
    {
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null && !collider.isTrigger) // ���������� ��������
        {
            collider.enabled = isEnabled;
        }

        // ���������� ������������ �������� �������
        foreach (Transform child in obj.transform)
        {
            SetCollidersEnabled(child.gameObject, isEnabled);
        }
    }

    // �������� ��� ��������� ������� � ������������� �������
    void SetTriggerEnabled(GameObject obj, bool isEnabled)
    {
        Collider triggerCollider = obj.GetComponent<Collider>();
        if (triggerCollider != null && triggerCollider.isTrigger)
        {
            triggerCollider.enabled = isEnabled;
        }
    }

    // ��������� �������� � ������� � ���� ��� �������� ��������
    void ApplyPreviewMaterial(GameObject obj, Material material)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }

        // ���������� ������������ �������� �������
        foreach (Transform child in obj.transform)
        {
            ApplyPreviewMaterial(child.gameObject, material);
        }
    }

    // ����������� ������� �������� � ����� "Placed"
    public void IncrementPlacedObjectsCount()
    {
        placedObjectsCount++;
    }

    // ��������� ������� �������� � ����� "Placed"
    public void DecrementPlacedObjectsCount()
    {
        placedObjectsCount--;
    }

    // ��������� ������ ��� ������������ �������� �� ������-������
    private void AddCollisionScriptToPreview()
    {
        if (previewObject != null)
        {
            PreviewObjectCollision collisionScript = previewObject.AddComponent<PreviewObjectCollision>();
            collisionScript.buildingUI = this;
        }
    }

    // ������� ������ ��� ������������ �������� � �������
    private void RemoveCollisionScriptFromObject(GameObject obj)
    {
        PreviewObjectCollision collisionScript = obj.GetComponent<PreviewObjectCollision>();
        if (collisionScript != null)
        {
            Destroy(collisionScript);
        }
    }

    // ������������ ������-������ ��� ������� ������� R � �������� ����
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