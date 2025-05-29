// ButtonTextPressOffset.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class ButtonTextPressOffset : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Alvo do Texto")]
    [Tooltip("O RectTransform do TextMeshProUGUI que será movido")]
    public RectTransform textToMove;

    [Header("Offset de Pressionado")]
    [Tooltip("Quanto o texto deve se deslocar ao pressionar o botão")]
    public Vector2 pressedOffset = new Vector2(0, -2);

    private Vector2 _originalPos;

    void Awake()
    {
        if (textToMove == null)
        {
            var tmp = GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                textToMove = tmp.rectTransform;
        }
        if (textToMove != null)
            _originalPos = textToMove.anchoredPosition;
    }

    void OnEnable()
    {
        if (textToMove != null)
            textToMove.anchoredPosition = _originalPos;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (textToMove != null)
            textToMove.anchoredPosition = _originalPos + pressedOffset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (textToMove != null)
            textToMove.anchoredPosition = _originalPos;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (textToMove != null)
            textToMove.anchoredPosition = _originalPos;
    }
}
