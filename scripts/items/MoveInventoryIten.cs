using UnityEngine;

public class MoveInventoryIten : MonoBehaviour
{
    [SerializeField] private HotbarSlotUI[] hotbarSlots;

    private void Start()
    {
        foreach (var slot in hotbarSlots)
        {
            // Удаляем все предыдущие слушатели, чтобы избежать дублирования
            slot.button.onClick.RemoveAllListeners();

            // Создаём локальную переменную, чтобы избежать замыкания (если потребуется в будущем)
            var currentSlot = slot;

            // Добавляем обработчик
        }
    }
}
