using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject frame;
    public Image buttonImage;
    public TMP_Text buttonText;

    private Color normalTextColor = Color.white;
    private Color selectedTextColor = Color.black;
    private Color selectedButtonColor = new Color(1f, 0.5f, 0f); // оранжевый
    private Color transparentColor = new Color(1f, 1f, 1f, 0f);   // прозрачный

    private bool isHovered = false;

    void Start()
    {
        ResetButtonVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        ApplyHoverStyle();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        RemoveHoverStyle();
        if (!IsInvoking(nameof(ResetButtonVisual)))
        {
            ResetButtonVisual();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StopAllCoroutines(); // на случай повторных кликов
        ApplyClickStyle();
        StartCoroutine(ResetAfterDelay(0.2f));
    }

    IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetButtonVisual();
        if (isHovered)
        {
            ApplyHoverStyle(); // Вернуть подчёркивание и рамку, если курсор всё ещё над кнопкой
        }
    }

    private void ApplyClickStyle()
    {
        buttonImage.color = selectedButtonColor;
        buttonText.color = selectedTextColor;
    }

    private void ResetButtonVisual()
    {
        buttonImage.color = transparentColor;
        buttonText.color = normalTextColor;
        RemoveHoverStyle();
        if (isHovered)
        {
            ApplyHoverStyle(); // оставить рамку и подчёркивание, если курсор наведен
        }
    }

    private void ApplyHoverStyle()
    {
        buttonText.fontStyle |= FontStyles.Underline;
        frame.SetActive(true);
    }

    private void RemoveHoverStyle()
    {
        buttonText.fontStyle &= ~FontStyles.Underline;
        frame.SetActive(false);
    }
}
