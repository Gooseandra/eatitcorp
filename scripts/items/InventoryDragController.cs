using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventoryDragController : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image dragIcon;

    private Image currentSlotIcon; // текущая иконка, которую скрыли
    private Transform originalParent; // куда возвращать
    private bool isDragging = false;

    private void Awake()
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (dragIcon == null)
            dragIcon = transform.Find("DragIcon")?.GetComponent<Image>();

        if (dragIcon != null)
        {
            dragIcon.enabled = false;
            dragIcon.raycastTarget = false;
        }
        else
        {
            Debug.LogError("DragIcon не найден!");
        }

        AssignSlotClickHandlers();
    }

    private void Update()
    {
        // Если курсор заблокирован — отменить перетаскивание и скрыть иконку
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (isDragging)
                CancelDrag();

            return;
        }

        if (isDragging)
        {
            Vector2 mousePos = Input.mousePosition;
            RectTransform dragRect = dragIcon.rectTransform;

            Vector3 worldPos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    canvas.transform as RectTransform,
                    mousePos,
                    canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                    out worldPos))
            {
                dragRect.position = worldPos;
            }

            // Отмена по Tab
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CancelDrag();
            }
        }
    }


    private void AssignSlotClickHandlers()
    {
        foreach (Transform child in transform)
        {
            Button button = child.GetComponent<Button>();
            if (button == null) continue;

            // Копия ссылки на объект (иначе замыкание сломает)
            Transform slotTransform = child;

            button.onClick.AddListener(() =>
            {
                Image icon = slotTransform.Find("Icon")?.GetComponent<Image>();
                if (icon != null && icon.enabled && icon.sprite != null)
                {
                    StartDrag(icon);
                }
            });
        }
    }

    private void StartDrag(Image iconToDrag)
    {
        currentSlotIcon = iconToDrag;
        originalParent = iconToDrag.transform.parent;

        Debug.Log(iconToDrag);
        Debug.Log(iconToDrag.sprite);
        dragIcon.sprite = iconToDrag.sprite;
        dragIcon.enabled = true;
        isDragging = true;

        iconToDrag.enabled = false;
    }

    private void CancelDrag()
    {
        if (currentSlotIcon != null)
            currentSlotIcon.enabled = true;

        dragIcon.sprite = null;
        dragIcon.enabled = false;
        isDragging = false;
    }
}
