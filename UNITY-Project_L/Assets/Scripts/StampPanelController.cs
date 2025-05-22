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

    public event Action<LabelMode, int> OnStamped;

    void Awake()
    {
        stampZeroButton.onClick.AddListener(StampZero);
        stampOneButton.onClick.AddListener(StampOne);
        stampFalseButton.onClick.AddListener(StampFalse);
        stampTrueButton.onClick.AddListener(StampTrue);
        stampLowButton.onClick.AddListener(StampLow);
        stampHighButton.onClick.AddListener(StampHigh);
    }

    public void StampZero() => InvokeStamp(LabelMode.Bit, 0);
    public void StampOne() => InvokeStamp(LabelMode.Bit, 1);
    public void StampFalse() => InvokeStamp(LabelMode.Bool, 0);
    public void StampTrue() => InvokeStamp(LabelMode.Bool, 1);
    public void StampLow() => InvokeStamp(LabelMode.Signal, 0);
    public void StampHigh() => InvokeStamp(LabelMode.Signal, 1);

    void InvokeStamp(LabelMode mode, int value)
    {
        DisableAll();
        OnStamped?.Invoke(mode, value);
    }

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

    // desabilita botões que não são do formato Bit
    public void DisableBoolAndSignal()
    {
        stampFalseButton.interactable = false;
        stampTrueButton.interactable = false;
        stampLowButton.interactable = false;
        stampHighButton.interactable = false;
    }
    // não-Bit & não-Bool
    public void DisableBitAndSignal()
    {
        stampZeroButton.interactable = false;
        stampOneButton.interactable = false;
        stampLowButton.interactable = false;
        stampHighButton.interactable = false;
    }
    // não-Bit & não-Signal
    public void DisableBitAndBool()
    {
        stampZeroButton.interactable = false;
        stampOneButton.interactable = false;
        stampFalseButton.interactable = false;
        stampTrueButton.interactable = false;
    }
}
