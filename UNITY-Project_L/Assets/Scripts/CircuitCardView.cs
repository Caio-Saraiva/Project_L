using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CircuitCardView : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Elements")]
    public TextMeshProUGUI[] inputTexts;    // arraste seus Texts para mostrar as entradas
    public Image gateImage;     // sprite da porta
    public Image stampOverlay;  // overlay do selo
    public Sprite[] gateSprites;   // AND, NAND, OR, NOR, NOT, XOR, XNOR
    public Sprite zeroStampSprite;
    public Sprite oneStampSprite;

    [HideInInspector] public RectTransform stampingZone;
    [HideInInspector] public Transform smallSlotParent;
    [HideInInspector] public Vector3 smallScale;
    [HideInInspector] public Vector3 stampScale;

    private CanvasGroup cg;
    private RectTransform rect;
    private Canvas rootCanvas;
    private Vector2 pointerOffset;
    private Transform originalParent;

    private CircuitDefinition def;
    private int? stampedValue;

    public event Action<bool> OnSent;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void Setup(CircuitDefinition definition)
    {
        def = definition;
        stampedValue = null;

        // Exibe entradas
        for (int i = 0; i < inputTexts.Length; i++)
        {
            inputTexts[i].text = i < def.inputs.Length
                ? def.inputs[i].ToString()
                : "";
        }

        // Exibe porta (assumindo 1 gate)
        gateImage.sprite = gateSprites[(int)def.gates[0]];

        // Esconde selo
        stampOverlay.color = new Color(1, 1, 1, 0);
    }

    public void ApplyStamp(int value)
    {
        if (stampedValue.HasValue) return;
        stampedValue = value;
        stampOverlay.sprite = value == 0
            ? zeroStampSprite
            : oneStampSprite;
        stampOverlay.color = new Color(1, 1, 1, 1);
    }

    public void Send()
    {
        if (!stampedValue.HasValue)
        {
            Debug.LogWarning("CircuitCardView: sem carimbo!");
            return;
        }
        bool correct = stampedValue.Value == def.output;
        OnSent?.Invoke(correct);
    }

    // ---- Drag & Drop ----

    public void OnBeginDrag(PointerEventData ev)
    {
        originalParent = rect.parent;
        rect.SetParent(rootCanvas.transform, true);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect, ev.position, ev.pressEventCamera, out pointerOffset);
        cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData ev)
    {
        Vector2 local;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            ev.position, ev.pressEventCamera, out local))
        {
            rect.localPosition = local - pointerOffset;
        }
    }

    public void OnEndDrag(PointerEventData ev)
    {
        cg.blocksRaycasts = true;

        bool overStamp = RectTransformUtility.RectangleContainsScreenPoint(
            stampingZone, ev.position, ev.pressEventCamera);

        if (overStamp)
        {
            rect.SetParent(stampingZone, false);
            rect.localScale = stampScale;
            rect.anchoredPosition = Vector2.zero;
        }
        else
        {
            rect.SetParent(smallSlotParent, false);
            rect.localScale = smallScale;
            rect.anchoredPosition = Vector2.zero;
        }
    }
}
