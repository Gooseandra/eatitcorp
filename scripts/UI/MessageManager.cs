using UnityEngine;
using System.Collections;

public class MessageManager : MonoBehaviour
{
    private Canvas targetCanvas;

    [SerializeField] private GameObject notEnoughMaterialPrefab; // Префаб сообщения
    [SerializeField] private RectTransform targetPosition;
    [SerializeField] private float fadeDuration = 2000f; // Длительность затухания
    [SerializeField] private float defaultAlpha = 0.8f; // Начальная прозрачность

    private void Awake()
    {
        // Находим Canvas в сцене (если не задан вручную)
        targetCanvas = Object.FindFirstObjectByType<Canvas>();
        if (targetCanvas == null)
        {
            Debug.LogError("В сцене нет Canvas!");
            return;
        }
    }

    public void ShowNotEnoughMaterialMessage()
    {
        // Создаём объект как дочерний у Canvas, а не у менеджера!
        GameObject messageInstance = Instantiate(
            notEnoughMaterialPrefab,
            targetCanvas.transform // Родитель — Canvas
        );

        // Настраиваем RectTransform
        RectTransform messageRect = messageInstance.GetComponent<RectTransform>();
        messageRect.anchoredPosition = targetPosition.anchoredPosition;

        // Настраиваем CanvasGroup
        CanvasGroup cg = messageInstance.GetComponent<CanvasGroup>();
        if (cg == null) cg = messageInstance.AddComponent<CanvasGroup>();
        cg.alpha = defaultAlpha;

        // Запускаем затухание
        StartCoroutine(FadeOutAndDestroy(messageInstance, cg));
    }

    private IEnumerator FadeOutAndDestroy(GameObject target, CanvasGroup cg)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            cg.alpha = Mathf.Lerp(defaultAlpha, 0f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(target);
    }
}
