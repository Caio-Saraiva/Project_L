using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class DropZoneController : MonoBehaviour, IDropHandler
{
    /// <summary>
    /// Disparado quando um CircuitCardView é solto sobre esta zona.
    /// </summary>
    public event Action<CircuitCardView> OnCardDropped;

    public void OnDrop(PointerEventData eventData)
    {
        var card = eventData.pointerDrag?.GetComponent<CircuitCardView>();
        if (card != null)
            OnCardDropped?.Invoke(card);
    }
}
