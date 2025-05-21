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

    [Header("Output Text (opcional)")]
    public TextMeshProUGUI outputText;

    [HideInInspector] public RectTransform collectArea;
    [HideInInspector] public RectTransform stampTableArea;
    [HideInInspector] public float defaultScale;
    [HideInInspector] public float zoomOutScale;

    // estado de drag
    private CanvasGroup cg;
    private RectTransform rect;
    private Canvas rootCanvas;
    private Vector2 pointerOffset;

    // estado de stamp
    private int? stampedValue;
    private LabelMode? stampedMode;

    public event Action<bool> OnSent;

    /// <summary>True se o card já recebeu um carimbo.</summary>
    public bool IsStamped => stampedMode.HasValue;

    /// <summary>Retorna o valor carimbado (0 ou 1).</summary>
    public int StampedValue => stampedValue ?? 0;

    /// <summary>Retorna o LabelMode do carimbo aplicado.</summary>
    public LabelMode? StampedMode => stampedMode;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// Aplica o carimbo com o modo e valor escolhidos, mostra sprite e texto.
    /// </summary>
    public void ApplyStamp(LabelMode mode, int value)
    {
        if (stampedMode.HasValue) return;

        stampedMode = mode;
        stampedValue = value;

        Sprite sprite = null;
        switch (mode)
        {
            case LabelMode.Bit:
                sprite = (value == 0 ? bitZeroStampSprite : bitOneStampSprite);
                break;
            case LabelMode.Bool:
                sprite = (value == 0 ? boolFalseStampSprite : boolTrueStampSprite);
                break;
            case LabelMode.Signal:
                sprite = (value == 0 ? signalLowStampSprite : signalHighStampSprite);
                break;
        }

        if (stampOverlay != null && sprite != null)
        {
            stampOverlay.sprite = sprite;
            stampOverlay.color = Color.white;
        }

        if (outputText != null)
            outputText.text = ValueToString(value, mode);
    }

    /// <summary>
    /// Valida se o carimbo bate com a saída e com o modo definido no setup.
    /// </summary>
    public void Send()
    {
        if (!stampedMode.HasValue)
        {
            Debug.LogWarning("CircuitCardView: nenhum carimbo aplicado!");
            return;
        }

        bool correct = (stampedMode.Value == setup.labelMode)
                       && (stampedValue.Value == setup.expectedOutput);

        OnSent?.Invoke(correct);
    }

    // ---------- Drag & Drop ----------

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
        float newScale = overStamp ? defaultScale
                       : overCollect ? zoomOutScale
                       : oldScale;

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
            case LabelMode.Bool: return v == 1 ? "TRUE" : "FALSE";
            case LabelMode.Signal: return v == 1 ? "HIGH" : "LOW";
            default: return v.ToString();
        }
    }
}
