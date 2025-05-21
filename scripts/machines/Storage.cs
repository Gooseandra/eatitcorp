using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class InitialStorageItem
{
    public GameObject itemPrefab; // Префаб с ItemPickup
    public int stacksCount = 1; // Сколько таких стеков добавить (кол-во слотов)
    public int amountPerStack = 1; // Количество предметов в каждом стеке
}

public class Storage : MonoBehaviour
{
    public List<Item> storageItems = new List<Item>();
    public int storageSize = 20;
    public GameObject storageUIPrefab;
    public Transform storageUIParent;
    public Inventory playerInventory;
    public float interactionDistance = 3f;

    private GameObject storageUIInstance;
    private List<StorageSlotUI> slotUIs = new List<StorageSlotUI>();
    private bool isUIOpen = false;
    [SerializeField] Transform spawnPos;

    [SerializeField] Movement playerMovement;

    public ConveyorSegment conveyorStartSegment; // первый сегмент конвейера хранилища

    private float conveyorCheckInterval = 1f;
    private float conveyorCheckTimer = 0f;
    [Header("Initial Storage Items")]
    public List<InitialStorageItem> initialItems = new List<InitialStorageItem>();

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = Object.FindAnyObjectByType<Movement>();

        if (playerInventory == null)
            playerInventory = Object.FindAnyObjectByType<Inventory>();

        if (storageUIParent == null)
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
                storageUIParent = canvas.transform;
        }

        InitializeStorage();

        // Добавляем из списка initialItems
        foreach (var initialItem in initialItems)
        {
            if (initialItem.itemPrefab == null) continue;

            ItemPickup pickup = initialItem.itemPrefab.GetComponent<ItemPickup>();
            if (pickup == null || pickup.item == null)
            {
                Debug.LogWarning("Префаб не содержит ItemPickup или Item в нем не назначен!");
                continue;
            }

            for (int i = 0; i < initialItem.stacksCount; i++)
            {
                // Создаем копию Item с указанным количеством
                Item itemCopy = new Item(
                    pickup.item.name,
                    Mathf.Min(initialItem.amountPerStack, pickup.item.maxStack),
                    pickup.item.maxStack,
                    pickup.item.icon,
                    pickup.item.thisPrefab,
                    pickup.item.buildingIndex
                );

                AddItem(itemCopy);
            }
        }
    }

private void Update()
    {
        if (isUIOpen && (Input.GetKeyDown(KeyCode.Escape) || !IsPlayerLookingAtStorage()))
        {
            CloseStorage();
        }
        // Закрытие UI и другое уже есть
        if (isUIOpen && (Input.GetKeyDown(KeyCode.Escape) || !IsPlayerLookingAtStorage()))
        {
            CloseStorage();
        }

        conveyorCheckTimer -= Time.deltaTime;
        if (conveyorCheckTimer <= 0f)
        {
            conveyorCheckTimer = conveyorCheckInterval;
            TryPlaceItemOnConveyor();
        }
    }

    private void TryPlaceItemOnConveyor()
    {
        if (conveyorStartSegment == null || conveyorStartSegment.nextSegment == null)
            return; // конвейер не настроен или нет следующего сегмента

        // Проверяем, есть ли предметы на первом сегменте (простой способ - проверим объекты с тегом "grabbable" около точки waypoint)
        Collider[] colliders = Physics.OverlapSphere(conveyorStartSegment.waypoint.position, 0.3f);
        foreach (var col in colliders)
        {
            if (col.CompareTag("grabbable"))
            {
                // На конвейере уже есть предмет, не кладём новый
                return;
            }
        }

        // Если предметов нет, берем первый непустой слот в storage
        for (int i = 0; i < storageItems.Count; i++)
        {
            if (storageItems[i] != null && storageItems[i].amount > 0)
            {
                SpawnItemOnConveyor(i);
                break;
            }
        }
    }

    private void SpawnItemOnConveyor(int slotIndex)
    {
        Item item = storageItems[slotIndex];
        if (item == null || item.amount <= 0)
            return;

        if (item.thisPrefab == null)
        {
            Debug.LogError($"Item prefab not set for item {item.name}!");
            return;
        }

        // Создаем объект предмета на позиции waypoint первого сегмента
        GameObject itemObject = Instantiate(item.thisPrefab, new Vector3(spawnPos.position.x, spawnPos.position.y, spawnPos.position.z), Quaternion.identity);
        itemObject.SetActive(true);

        // Настроим Rigidbody и скрипт движения, если необходимо
        Rigidbody rb = itemObject.GetComponent<Rigidbody>();
        if (rb == null)
            rb = itemObject.AddComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.None;

        ConveirMovement conveyorMovement = itemObject.GetComponent<ConveirMovement>();
        if (conveyorMovement == null)
            conveyorMovement = itemObject.AddComponent<ConveirMovement>();

        conveyorMovement.SetStopped(false);

        // Уменьшаем количество предметов в хранилище
        storageItems[slotIndex].amount--;
        if (storageItems[slotIndex].amount <= 0)
            storageItems[slotIndex] = null;

        UpdateStorageUI();
        playerInventory.hotbarUI.UpdateAllSlots();
    }



    private void InitializeStorage()
    {
        storageItems = new List<Item>();
        for (int i = 0; i < storageSize; i++)
        {
            storageItems.Add(null);
        }
    }

    public bool IsPlayerLookingAtStorage()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        return Physics.Raycast(ray, out hit, interactionDistance) && hit.collider.gameObject == this.gameObject;
    }

    public void TryOpenStorage()
    {
        if (IsPlayerLookingAtStorage() && !isUIOpen)
        {
            OpenStorage();
        }
    }

    private void OpenStorage()
    {
        if (storageUIInstance == null)
        {
            storageUIInstance = Instantiate(storageUIPrefab, storageUIParent);
            CreateSlotUIs();
        }

        storageUIInstance.SetActive(true);
        isUIOpen = true;
        UpdateStorageUI();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerMovement.lockCamera = true;
    }

    public void CloseStorage()
    {
        if (storageUIInstance != null)
            storageUIInstance.SetActive(false);

        isUIOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerMovement.lockCamera = false;
    }

    private void CreateSlotUIs()
    {
        slotUIs.Clear();

        foreach (Transform child in storageUIInstance.transform)
        {
            StorageSlotUI slot = child.GetComponent<StorageSlotUI>();
            if (slot != null)
            {
                int index = slotUIs.Count;
                slot.Setup(this, index);
                slotUIs.Add(slot);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);
        // Проверяем, есть ли у объекта компонент ItemPickup
        ItemPickup pickup = other.GetComponent<ItemPickup>();
        if (pickup != null)
        {
            // Получаем предмет из ItemPickup
            Item item = pickup.item;

            // Пробуем добавить предмет в хранилище
            if (AddItem(item))
            {
                // Успешно добавлено — удаляем объект с сцены
                Destroy(other.gameObject);
            }
            else
            {
                // Место в хранилище закончилось — можно сделать что-то (например, уведомление)
                Debug.Log("Хранилище заполнено, предмет не добавлен");
            }
        }
    }


    public void UpdateStorageUI()
    {
        for (int i = 0; i < storageItems.Count && i < slotUIs.Count; i++)
        {
            slotUIs[i].SetItem(storageItems[i]);
        }
    }

    public bool AddItem(Item newItem)
    {
        for (int i = 0; i < storageItems.Count; i++)
        {
            if (storageItems[i] != null && storageItems[i].name == newItem.name && storageItems[i].amount < storageItems[i].maxStack)
            {
                int space = storageItems[i].maxStack - storageItems[i].amount;
                if (newItem.amount <= space)
                {
                    storageItems[i].amount += newItem.amount;
                    UpdateStorageUI();
                    return true;
                }
                else
                {
                    storageItems[i].amount = storageItems[i].maxStack;
                    newItem.amount -= space;
                }
            }
        }

        for (int i = 0; i < storageItems.Count; i++)
        {
            if (storageItems[i] == null || storageItems[i].amount <= 0)
            {
                storageItems[i] = newItem;
                UpdateStorageUI();
                return true;
            }
        }

        return false;
    }

    public void TransferEntireItemToInventory(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < storageItems.Count &&
            storageItems[slotIndex] != null && storageItems[slotIndex].amount > 0)
        {
            Item originalItem = storageItems[slotIndex];
            Item itemToTransfer = new Item(
                originalItem.name,
                originalItem.amount,
                originalItem.maxStack,
                originalItem.icon,
                originalItem.thisPrefab,
                originalItem.buildingIndex
            );

            if (playerInventory.AddItem(itemToTransfer))
            {
                storageItems[slotIndex] = null;
                UpdateStorageUI();
                playerInventory.hotbarUI.UpdateAllSlots();
            }
        }
    }

    public void TransferOneItemToInventory(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < storageItems.Count &&
            storageItems[slotIndex] != null && storageItems[slotIndex].amount > 0)
        {
            Item originalItem = storageItems[slotIndex];

            Item itemToTransfer = new Item(
                originalItem.name,
                1,
                originalItem.maxStack,
                originalItem.icon,
                originalItem.thisPrefab,
                originalItem.buildingIndex
            );

            if (playerInventory.AddItem(itemToTransfer))
            {
                storageItems[slotIndex].amount--;
                if (storageItems[slotIndex].amount <= 0)
                    storageItems[slotIndex] = null;

                UpdateStorageUI();
                playerInventory.hotbarUI.UpdateAllSlots();
            }
        }
    }


}
