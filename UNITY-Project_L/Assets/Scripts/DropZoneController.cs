using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class DropZoneController : MonoBehaviour, IDropHandler
{
    /// <summary>
    /// Disparado quando um CircuitCardView for solto sobre esta zona.
    /// </summary>
    public event Action<CircuitCardView> OnCardDropped;

    public void OnDrop(PointerEventData eventData)
    {
        // eventData.pointerDrag é o GameObject sendo arrastado
        var card = eventData.pointerDrag?.GetComponent<CircuitCardView>();
        if (card != null)
            OnCardDropped?.Invoke(card);
    }
}
