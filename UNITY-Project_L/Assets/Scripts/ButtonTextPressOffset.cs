using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class ButtonTextPressOffset : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Alvo do Texto")]
    [Tooltip("O RectTransform do TextMeshProUGUI que será movido")]
    public RectTransform textToMove;

    [Header("Offset de Pressionado")]
    [Tooltip("Quanto o texto deve se deslocar ao pressionar o botão")]
    public Vector2 pressedOffset = new Vector2(0, -20);

    Vector2 _originalPos;

    void Awake()
    {
        // Se não especificado, tenta encontrar o TMP na hierarquia
        if (textToMove == null)
        {
            var tmp = GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                textToMove = tmp.rectTransform;
        }

        // Salva a posição original para restaurar depois
        if (textToMove != null)
            _originalPos = textToMove.anchoredPosition;
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
        // Se o ponteiro sair do botão sem soltar, também restaura
        if (textToMove != null)
            textToMove.anchoredPosition = _originalPos;
    }
}
