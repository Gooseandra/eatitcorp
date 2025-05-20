using UnityEngine;
using UnityEngine.UI;

public class InventoryDragController : MonoBehaviour
{
    public static InventoryDragController Instance;

    [SerializeField] private Canvas canvas;
    [SerializeField] private Image dragIcon;

    private Image currentSlotIcon;
    private Transform originalParent;
    private bool isDragging = false;
    private Item draggedItem;

    private void Awake()
    {
        dragIcon.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        Instance = this;

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
    }

    private void Update()
    {
        if (isDragging)
        {
            Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

            Vector3 globalMousePos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                cam,
                out globalMousePos))
            {
                dragIcon.rectTransform.position = globalMousePos;
            }

            if (Cursor.lockState == CursorLockMode.Locked || Input.GetKeyDown(KeyCode.Tab))
            {
                CancelDrag();
            }
        }
    }

    public void StartDrag(Image iconToDrag, Item item)
    {
        Debug.Log("start drag");
        Debug.Log(iconToDrag);
        Debug.Log(item);
        if (iconToDrag == null || item == null)
        {
            Debug.Log("в пизду");
            return;
        }


        currentSlotIcon = iconToDrag;
        draggedItem = item;
        originalParent = iconToDrag.transform.parent;

        dragIcon.sprite = iconToDrag.sprite;
        dragIcon.enabled = true;
        isDragging = true;

        iconToDrag.enabled = false;
    }

    public void CancelDrag()
    {
        if (currentSlotIcon != null)
            currentSlotIcon.enabled = true;

        dragIcon.sprite = null;
        dragIcon.enabled = false;
        isDragging = false;
        draggedItem = null;
    }

    public bool IsDragging() => isDragging && draggedItem != null;

    public Item GetDraggedItem() => draggedItem;
}
