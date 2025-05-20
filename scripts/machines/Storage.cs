using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

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

    [SerializeField] Movement playerMovement;

    private void Awake()
    {
        InitializeStorage();
    }

    private void Update()
    {
        if (isUIOpen && (Input.GetKeyDown(KeyCode.Escape) || !IsPlayerLookingAtStorage()))
        {
            CloseStorage();
        }
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

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            return hit.collider.gameObject == this.gameObject;
        }
        return false;
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
        {
            storageUIInstance.SetActive(false);
        }
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
            if (child.GetComponent<StorageSlotUI>() != null)
            {
                slotUIs.Add(child.GetComponent<StorageSlotUI>());
            }
        }

        for (int i = 0; i < slotUIs.Count; i++)
        {
            int slotIndex = i;
            slotUIs[i].GetComponent<Button>().onClick.AddListener(() => TransferItemToInventory(slotIndex));
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
        // Попробуем добавить к существующему стеку
        for (int i = 0; i < storageItems.Count; i++)
        {
            if (storageItems[i] != null &&
                storageItems[i].name == newItem.name &&
                storageItems[i].amount < storageItems[i].maxStack)
            {
                int spaceLeft = storageItems[i].maxStack - storageItems[i].amount;
                if (newItem.amount <= spaceLeft)
                {
                    storageItems[i].amount += newItem.amount;
                    UpdateStorageUI();
                    return true;
                }
                else
                {
                    storageItems[i].amount = storageItems[i].maxStack;
                    newItem.amount -= spaceLeft;
                }
            }
        }

        // Добавляем в пустой слот
        for (int i = 0; i < storageItems.Count; i++)
        {
            if (storageItems[i] == null || storageItems[i].amount == 0)
            {
                storageItems[i] = newItem;
                UpdateStorageUI();
                return true;
            }
        }

        return false;
    }

    public void TransferItemToInventory(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < storageItems.Count &&
            storageItems[slotIndex] != null && storageItems[slotIndex].amount > 0)
        {
            Item itemToTransfer = storageItems[slotIndex];
            itemToTransfer.amount = 1;

            if (playerInventory.AddItem(itemToTransfer))
            {
                storageItems[slotIndex].amount--;
                if (storageItems[slotIndex].amount <= 0)
                {
                    storageItems[slotIndex] = null;
                }
                UpdateStorageUI();
            }
        }
    }

    public void TransferItemToStorage(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < playerInventory.hotbar.Count &&
            playerInventory.hotbar[slotIndex] != null && playerInventory.hotbar[slotIndex].amount > 0)
        {
            Item itemToTransfer = playerInventory.hotbar[slotIndex];
            itemToTransfer.amount = 1;

            if (AddItem(itemToTransfer))
            {
                playerInventory.hotbar[slotIndex].amount--;
                if (playerInventory.hotbar[slotIndex].amount <= 0)
                {
                    playerInventory.hotbar[slotIndex] = null;
                }
                // Здесь нужно вызвать метод обновления UI инвентаря игрока
            }
        }
    }
}