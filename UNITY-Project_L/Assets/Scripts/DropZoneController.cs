using UnityEngine;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(RectTransform))]
public class DropZoneController : MonoBehaviour, IDropHandler
{
    [Header("Área de Detecção Extra (px)")]
    [Tooltip("Quanto adicionar à largura total da zona de drop")]
    public float extraWidth = 0f;
    [Tooltip("Quanto adicionar à altura total da zona de drop")]
    public float extraHeight = 0f;

    /// <summary>
    /// Disparado quando um CircuitCardView é solto sobre esta zona.
    /// </summary>
    public event Action<CircuitCardView> OnCardDropped;

    RectTransform _rt;
    Vector2 _originalSize;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        // guarda o tamanho original
        _originalSize = _rt.sizeDelta;
        ApplyPadding();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (_rt == null) _rt = GetComponent<RectTransform>();
        // assume que o sizeDelta atual = original + padding
        _originalSize = _rt.sizeDelta - new Vector2(extraWidth, extraHeight);
        ApplyPadding();
    }
#endif

    /// <summary>
    /// Ajusta o sizeDelta do RectTransform adicionando extraWidth/extraHeight.
    /// </summary>
    void ApplyPadding()
    {
        if (_rt == null) return;
        _rt.sizeDelta = new Vector2(
            _originalSize.x + extraWidth,
            _originalSize.y + extraHeight
        );
    }

    public void OnDrop(PointerEventData eventData)
    {
        var card = eventData.pointerDrag?.GetComponent<CircuitCardView>();
        if (card != null)
            OnCardDropped?.Invoke(card);
    }
}
