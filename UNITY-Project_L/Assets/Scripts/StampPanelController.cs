using UnityEngine;
using UnityEngine.UI;
using System;

public class StampPanelController : MonoBehaviour
{
    [Header("Bit Stamps")]
    public Button stampZeroButton;
    public Button stampOneButton;

    [Header("Bool Stamps")]
    public Button stampFalseButton;
    public Button stampTrueButton;

    [Header("Signal Stamps")]
    public Button stampLowButton;
    public Button stampHighButton;

    /// <summary>
    /// Disparado com o LabelMode e o valor 0/1 escolhidos.
    /// </summary>
    public event Action<LabelMode, int> OnStamped;

    void Awake()
    {
        // Liga cada botão ao seu método
        stampZeroButton.onClick.AddListener(StampZero);
        stampOneButton.onClick.AddListener(StampOne);
        stampFalseButton.onClick.AddListener(StampFalse);
        stampTrueButton.onClick.AddListener(StampTrue);
        stampLowButton.onClick.AddListener(StampLow);
        stampHighButton.onClick.AddListener(StampHigh);
    }

    // Cada método dispara o evento e trava todos os botões
    public void StampZero() { InvokeStamp(LabelMode.Bit, 0); }
    public void StampOne() { InvokeStamp(LabelMode.Bit, 1); }
    public void StampFalse() { InvokeStamp(LabelMode.Bool, 0); }
    public void StampTrue() { InvokeStamp(LabelMode.Bool, 1); }
    public void StampLow() { InvokeStamp(LabelMode.Signal, 0); }
    public void StampHigh() { InvokeStamp(LabelMode.Signal, 1); }

    void InvokeStamp(LabelMode mode, int value)
    {
        DisableAll();
        OnStamped?.Invoke(mode, value);
    }

    /// <summary>Reabilita todos os botões (chamar antes de cada novo card).</summary>
    public void EnableAll()
    {
        stampZeroButton.interactable = true;
        stampOneButton.interactable = true;
        stampFalseButton.interactable = true;
        stampTrueButton.interactable = true;
        stampLowButton.interactable = true;
        stampHighButton.interactable = true;
    }

   public void DisableAll()
    {
        stampZeroButton.interactable = false;
        stampOneButton.interactable = false;
        stampFalseButton.interactable = false;
        stampTrueButton.interactable = false;
        stampLowButton.interactable = false;
        stampHighButton.interactable = false;
    }
}
