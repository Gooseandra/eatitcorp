using UnityEngine;
using System.Collections;

public class MessageManager : MonoBehaviour
{
    private Canvas targetCanvas;

    [SerializeField] private GameObject notEnoughMaterialPrefab; // ������ ���������
    [SerializeField] private RectTransform targetPosition;
    [SerializeField] private float fadeDuration = 2000f; // ������������ ���������
    [SerializeField] private float defaultAlpha = 0.8f; // ��������� ������������

    private void Awake()
    {
        // ������� Canvas � ����� (���� �� ����� �������)
        targetCanvas = Object.FindFirstObjectByType<Canvas>();
        if (targetCanvas == null)
        {
            Debug.LogError("� ����� ��� Canvas!");
            return;
        }
    }

    public void ShowNotEnoughMaterialMessage()
    {
        // ������ ������ ��� �������� � Canvas, � �� � ���������!
        GameObject messageInstance = Instantiate(
            notEnoughMaterialPrefab,
            targetCanvas.transform // �������� � Canvas
        );

        // ����������� RectTransform
        RectTransform messageRect = messageInstance.GetComponent<RectTransform>();
        messageRect.anchoredPosition = targetPosition.anchoredPosition;

        // ����������� CanvasGroup
        CanvasGroup cg = messageInstance.GetComponent<CanvasGroup>();
        if (cg == null) cg = messageInstance.AddComponent<CanvasGroup>();
        cg.alpha = defaultAlpha;

        // ��������� ���������
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
