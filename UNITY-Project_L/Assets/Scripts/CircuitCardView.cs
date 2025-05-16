using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class CircuitCardView : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Circuit Setup")]
    public CircuitCardSetup setup;

    [Header("Stamp Overlay")]
    public Image stampOverlay;
    public Sprite zeroStampSprite;
    public Sprite oneStampSprite;

    [Header("Output Text (opcional)")]
    public TextMeshProUGUI outputText;

    [HideInInspector] public RectTransform collectArea;
    [HideInInspector] public RectTransform stampTableArea;
    [HideInInspector] public float defaultScale;
    [HideInInspector] public float zoomOutScale;

    private CanvasGroup cg;
    private RectTransform rect;
    private Canvas rootCanvas;
    private Vector2 pointerOffset;
    private int? stampedValue;

    public event Action<bool> OnSent;

    /// <summary>
    /// True se já houve um carimbo aplicado.
    /// </summary>
    public bool IsStamped => stampedValue.HasValue;

    /// <summary>
    /// Retorna o valor do carimbo (0 ou 1). Usar apenas se IsStamped for true.
    /// </summary>
    public int StampedValue => stampedValue ?? 0;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// Aplica o selo gráfico e atualiza o texto de saída.
    /// </summary>
    public void ApplyStamp(int value)
    {
        if (stampedValue.HasValue) return;
        stampedValue = value;

        if (stampOverlay != null)
        {
            stampOverlay.sprite = (value == 0 ? zeroStampSprite : oneStampSprite);
            stampOverlay.color = Color.white;
        }

        if (outputText != null)
            outputText.text = ValueToString(value, setup.labelMode);
    }

    /// <summary>
    /// Valida contra expectedOutput e dispara OnSent.
    /// </summary>
    public void Send()
    {
        if (!stampedValue.HasValue)
        {
            Debug.LogWarning("CircuitCardView: não há selo aplicado!");
            return;
        }
        bool correct = stampedValue.Value == setup.expectedOutput;
        OnSent?.Invoke(correct);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        cg.blocksRaycasts = false;
        rect.SetParent(rootCanvas.transform, true);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            e.position, e.pressEventCamera,
            out Vector2 local
        );
        pointerOffset = rect.anchoredPosition - local;
    }

    public void OnDrag(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            e.position, e.pressEventCamera,
            out Vector2 local
        );
        rect.anchoredPosition = local + pointerOffset;
    }

    public void OnEndDrag(PointerEventData e)
    {
        cg.blocksRaycasts = true;

        bool overStamp = RectTransformUtility.RectangleContainsScreenPoint(
            stampTableArea, e.position, e.pressEventCamera);
        bool overCollect = RectTransformUtility.RectangleContainsScreenPoint(
            collectArea, e.position, e.pressEventCamera);

        float oldScale = rect.localScale.x;
        float newScale = oldScale;
        if (overStamp) newScale = defaultScale;
        else if (overCollect) newScale = zoomOutScale;

        if (!Mathf.Approximately(newScale, oldScale))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.transform as RectTransform,
                e.position, e.pressEventCamera,
                out Vector2 local
            );
            float ratio = newScale / oldScale;
            Vector2 scaledOffset = pointerOffset * ratio;
            rect.localScale = Vector3.one * newScale;
            rect.anchoredPosition = local + scaledOffset;
            pointerOffset = scaledOffset;
        }
    }

    private string ValueToString(int v, LabelMode mode)
    {
        switch (mode)
        {
            case LabelMode.Bool: return v == 1 ? "True" : "False";
            case LabelMode.Signal: return v == 1 ? "High" : "Low";
            default: return v.ToString();
        }
    }
}
