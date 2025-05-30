using UnityEngine;
using UnityEngine.Events;

public class EnableAndFinishInvoker : MonoBehaviour
{
    [Header("Configurações de Tempo")]
    [Tooltip("Tempo em segundos para aguardar antes de disparar o evento OnFinished")]
    public float delay = 5f;

    [Header("Eventos")]
    [Tooltip("Evento disparado imediatamente ao habilitar este componente")]
    public UnityEvent OnEnableEvent;
    [Tooltip("Evento disparado após o tempo de delay")]
    public UnityEvent OnFinishedEvent;

    private void OnEnable()
    {
        // Dispara o evento de enable imediatamente
        OnEnableEvent?.Invoke();

        // Agenda o disparo do evento final após 'delay' segundos
        Invoke(nameof(InvokeFinished), delay);
    }

    private void InvokeFinished()
    {
        OnFinishedEvent?.Invoke();
    }
}
