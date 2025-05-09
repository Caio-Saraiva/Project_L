using System;
using UnityEngine;
using UnityEngine.UI;

public class StampPanelController : MonoBehaviour
{
    [Header("Bit Stamps")]
    public Button bitZeroButton;
    public Button bitOneButton;

    [Header("Bool Stamps")]
    public Button boolFalseButton;
    public Button boolTrueButton;

    [Header("Signal Stamps")]
    public Button signalLowButton;
    public Button signalHighButton;

    /// <summary>
    /// Disparado com o LabelMode e o valor (0 ou 1) escolhidos.
    /// </summary>
    public event Action<LabelMode, int> OnStamped;

    void Awake()
    {
        bitZeroButton.onClick.AddListener(() => ApplyStamp(LabelMode.Bit, 0));
        bitOneButton.onClick.AddListener(() => ApplyStamp(LabelMode.Bit, 1));
        boolFalseButton.onClick.AddListener(() => ApplyStamp(LabelMode.Bool, 0));
        boolTrueButton.onClick.AddListener(() => ApplyStamp(LabelMode.Bool, 1));
        signalLowButton.onClick.AddListener(() => ApplyStamp(LabelMode.Signal, 0));
        signalHighButton.onClick.AddListener(() => ApplyStamp(LabelMode.Signal, 1));
    }

    void ApplyStamp(LabelMode mode, int value)
    {
        DisableAllStamps();
        OnStamped?.Invoke(mode, value);
    }

    void DisableAllStamps()
    {
        bitZeroButton.interactable = false;
        bitOneButton.interactable = false;
        boolFalseButton.interactable = false;
        boolTrueButton.interactable = false;
        signalLowButton.interactable = false;
        signalHighButton.interactable = false;
    }

    /// <summary>
    /// Reabilita todos os botões antes de cada novo card.
    /// </summary>
    public void EnableAllStamps()
    {
        bitZeroButton.interactable = true;
        bitOneButton.interactable = true;
        boolFalseButton.interactable = true;
        boolTrueButton.interactable = true;
        signalLowButton.interactable = true;
        signalHighButton.interactable = true;
    }
}
