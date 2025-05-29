// HoldButton.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(Button))]
public class HoldButton : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Configuração de Hold")]
    [Tooltip("Tempo em segundos que precisa segurar para disparar o evento")]
    public float holdDuration = 3f;

    [Tooltip("Tempo em segundos de espera após o hold antes de invocar")]
    public float preInvokeDelay = 0.2f;

    [Tooltip("Scrollbar interna que mostra o progresso de 0→1")]
    public Scrollbar progressBar;

    [Tooltip("Evento a ser chamado quando o hold completar + delay")]
    public UnityEvent onHoldComplete;

    bool _isHolding;
    bool _invokeScheduled;
    float _holdStartTime;

    void Awake()
    {
        if (progressBar != null)
            progressBar.size = 0f;
    }

    void Update()
    {
        if (!_isHolding || _invokeScheduled)
            return;

        float elapsed = Time.time - _holdStartTime;
        if (progressBar != null)
            progressBar.size = Mathf.Clamp01(elapsed / holdDuration);

        if (elapsed >= holdDuration)
        {
            _invokeScheduled = true;
            StartCoroutine(DelayedInvoke());
        }
    }

    IEnumerator DelayedInvoke()
    {
        yield return new WaitForSeconds(preInvokeDelay);
        onHoldComplete?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isHolding = true;
        _invokeScheduled = false;
        _holdStartTime = Time.time;
        if (progressBar != null)
            progressBar.size = 0f;
    }

    public void OnPointerUp(PointerEventData eventData) => ResetHold();

    public void OnPointerExit(PointerEventData eventData) => ResetHold();

    void ResetHold()
    {
        _isHolding = false;
        _invokeScheduled = false;
        StopAllCoroutines();
        if (progressBar != null)
            progressBar.size = 0f;
    }

    void OnDisable() => ResetHold();
}
