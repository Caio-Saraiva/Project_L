using System;       // para Action
using UnityEngine;
using TMPro;

[RequireComponent(typeof(BoxCollider))]
public class InputNodeView : MonoBehaviour
{
    public TextMeshProUGUI valueText;

    [HideInInspector] public GateNode model;
    private Action onValueChanged;
    private int? currentValue;

    /// <summary>
    /// Configura o callback que deve ser chamado sempre que este input mudar.
    /// </summary>
    public void Initialize(Action onValueChanged)
    {
        this.onValueChanged = onValueChanged;
        currentValue = null;
        if (valueText != null)
            valueText.text = "";
    }

    /// <summary>
    /// Associa o nó de dados a este view.
    /// </summary>
    public void SetModel(GateNode model)
    {
        this.model = model;
    }

    void OnMouseDown()
    {
        // cicla null → 0 → 1 → 0 …
        if (!currentValue.HasValue)
            currentValue = 0;
        else
            currentValue = currentValue.Value == 0 ? 1 : 0;

        if (valueText != null)
            valueText.text = currentValue.ToString();

        // atualiza o modelo de dados
        if (model != null && currentValue.HasValue)
        {
            model.IsStaticValue = true;
            model.StaticValue = currentValue.Value;
        }

        // notifica o GateView / CircuitManager
        onValueChanged?.Invoke();
    }
}
