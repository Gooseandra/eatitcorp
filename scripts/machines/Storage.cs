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
