using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(Graphic))]
public class UIDropShadow : MonoBehaviour
{
    [Header("Target (quem gera a sombra)")]
    [Tooltip("O RectTransform do elemento que queremos sombrear")]
    public RectTransform targetRect;

    [Header("Offset da Sombra")]
    [Tooltip("Offset em X/Y aplicado à posição do alvo")]
    public Vector2 shadowOffset = new Vector2(4, -4);

    [Header("Aparência da Sombra")]
    public Color shadowColor = Color.black;
    [Range(0, 1)] public float shadowAlpha = 0.5f;

    // Freeze control
    private bool _freeze;
    private Vector2 _frozenPos;

    // Internals
    private GameObject _shadowGO;
    private RectTransform _shadowRT;
    private Graphic _shadowGraphic;
    private Graphic _origGraphic;

    void OnEnable()
    {
        _origGraphic = GetComponent<Graphic>();
        if (targetRect == null)
            targetRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (_origGraphic == null || targetRect == null) return;

        if (_shadowGO == null)
            CreateShadow();

        UpdateShadow();
    }

    void OnDisable()
    {
        if (_shadowGO != null)
        {
            if (Application.isPlaying) Destroy(_shadowGO);
            else DestroyImmediate(_shadowGO);
        }
        _shadowGO = null;
    }

    /// <summary>
    /// Chame true ao começar o drag (para fixar sombra no lugar), false ao terminar.
    /// </summary>
    public void SetFreeze(bool freeze)
    {
        if (freeze && !_freeze)
        {
            _freeze = true;
            _frozenPos = targetRect.anchoredPosition;
        }
        else if (!freeze)
        {
            _freeze = false;
        }
    }

    private void CreateShadow()
    {
        _shadowGO = new GameObject(name + "_Shadow", typeof(RectTransform));
        _shadowGO.transform.SetParent(targetRect.parent, false);
        _shadowGO.transform.SetSiblingIndex(targetRect.GetSiblingIndex());

        _shadowRT = _shadowGO.GetComponent<RectTransform>();
        _shadowGO.hideFlags = HideFlags.DontSave;

        if (_origGraphic is Image img)
        {
            var s = _shadowGO.AddComponent<Image>();
            s.sprite = img.sprite;
            s.type = img.type;
            s.preserveAspect = img.preserveAspect;
            s.raycastTarget = false;
            _shadowGraphic = s;
        }
        else if (_origGraphic is TextMeshProUGUI txt)
        {
            var s = _shadowGO.AddComponent<TextMeshProUGUI>();
            s.font = txt.font;
            s.fontSize = txt.fontSize;
            s.fontStyle = txt.fontStyle;
            s.enableAutoSizing = txt.enableAutoSizing;
            s.fontSizeMin = txt.fontSizeMin;
            s.fontSizeMax = txt.fontSizeMax;
            s.alignment = txt.alignment;
            s.richText = txt.richText;
            s.textWrappingMode = txt.textWrappingMode;
            s.raycastTarget = false;
            _shadowGraphic = s;
        }
        else
        {
            Debug.LogWarning($"UIDropShadow: tipo {_origGraphic.GetType().Name} não suportado.");
        }
    }

    private void UpdateShadow()
    {
        Vector2 basePos = _freeze
            ? _frozenPos
            : targetRect.anchoredPosition;

        // replica layout
        _shadowRT.anchorMin = targetRect.anchorMin;
        _shadowRT.anchorMax = targetRect.anchorMax;
        _shadowRT.pivot = targetRect.pivot;
        _shadowRT.sizeDelta = targetRect.sizeDelta;
        _shadowRT.localScale = targetRect.localScale;
        _shadowRT.localRotation = targetRect.localRotation;
        _shadowRT.anchoredPosition = basePos + shadowOffset;

        // cor + alfa
        var c = shadowColor;
        c.a *= shadowAlpha;
        _shadowGraphic.color = c;

        // conteúdo
        if (_origGraphic is Image oi && _shadowGraphic is Image si)
        {
            if (si.sprite != oi.sprite) si.sprite = oi.sprite;
        }
        else if (_origGraphic is TextMeshProUGUI ot && _shadowGraphic is TextMeshProUGUI st)
        {
            if (st.text != ot.text) st.text = ot.text;
        }
    }
}
