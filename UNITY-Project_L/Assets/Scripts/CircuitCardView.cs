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

    [Header("Bit Stamps")]
    public Sprite bitZeroStampSprite;
    public Sprite bitOneStampSprite;

    [Header("Bool Stamps")]
    public Sprite boolFalseStampSprite;
    public Sprite boolTrueStampSprite;

    [Header("Signal Stamps")]
    public Sprite signalLowStampSprite;
    public Sprite signalHighStampSprite;

    [Header("Stamp Colors")]
    [Tooltip("Cor para valores 1, TRUE e HIGH")]
    public Color stampPositiveColor = Color.green;
    [Tooltip("Cor para valores 0, FALSE e LOW")]
    public Color stampNegativeColor = Color.red;

    [Header("Output Text (apenas após carimbar)")]
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
    private LabelMode? stampedMode;

    public bool IsStamped => stampedMode.HasValue;
    public int StampedValue => stampedValue ?? 0;
    public LabelMode? StampedMode => stampedMode;

    public event Action<bool> OnSent;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();

        if (outputText != null)
            outputText.text = "";
    }

    public void ApplyStamp(LabelMode mode, int value)
    {
        if (stampedMode.HasValue) return;

        stampedMode = mode;
        stampedValue = value;

        Sprite sprite = null;
        switch (mode)
        {
            case LabelMode.Bit:
                sprite = (value == 0
                    ? bitZeroStampSprite
                    : bitOneStampSprite);
                break;
            case LabelMode.Bool:
                sprite = (value == 0
                    ? boolFalseStampSprite
                    : boolTrueStampSprite);
                break;
            case LabelMode.Signal:
                sprite = (value == 0
                    ? signalLowStampSprite
                    : signalHighStampSprite);
                break;
        }

        if (stampOverlay != null && sprite != null)
        {
            stampOverlay.sprite = sprite;
            // aqui usamos as cores novas
            stampOverlay.color = (value == 0
                ? stampNegativeColor
                : stampPositiveColor);
        }

        if (outputText != null)
            outputText.text = ValueToString(value, mode);
    }

    public void Send()
    {
        if (!stampedMode.HasValue)
        {
            Debug.LogWarning("CircuitCardView: nenhum carimbo aplicado!");
            return;
        }
        bool correct = stampedMode.Value == setup.labelMode
                    && stampedValue.Value == setup.expectedOutput;
        OnSent?.Invoke(correct);
    }

    // ─── Drag & Drop ───

    public void OnBeginDrag(PointerEventData eventData)
    {
        cg.blocksRaycasts = false;
        rect.SetParent(rootCanvas.transform, true);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPointer
        );
        pointerOffset = rect.anchoredPosition - localPointer;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPointer
        );
        rect.anchoredPosition = localPointer + pointerOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        cg.blocksRaycasts = true;

        bool overStamp = RectTransformUtility.RectangleContainsScreenPoint(
            stampTableArea, eventData.position, eventData.pressEventCamera);
        bool overCollect = RectTransformUtility.RectangleContainsScreenPoint(
            collectArea, eventData.position, eventData.pressEventCamera);

        float oldScale = rect.localScale.x;
        float newScale = overStamp ? defaultScale
                       : overCollect ? zoomOutScale
                       : oldScale;

        if (!Mathf.Approximately(newScale, oldScale))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPointer
            );
            float ratio = newScale / oldScale;
            Vector2 scaledOffset = pointerOffset * ratio;

            rect.localScale = Vector3.one * newScale;
            rect.anchoredPosition = localPointer + scaledOffset;
            pointerOffset = scaledOffset;
        }
    }

    // ─── Utilitário ───

    private string ValueToString(int v, LabelMode mode)
    {
        switch (mode)
        {
            case LabelMode.Bool: return v == 1 ? "TRUE" : "FALSE";
            case LabelMode.Signal: return v == 1 ? "HIGH" : "LOW";
            default: return v.ToString();
        }
    }
}
