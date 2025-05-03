using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GateView : MonoBehaviour
{
    public TextMeshProUGUI identifierText;
    public TextMeshProUGUI outputHintText;
    public Transform socketContainer;
    public GameObject inputSocketPrefab;
    public float inputOffsetX = 1.5f;
    public float socketSpacingZ = 1f;

    [HideInInspector] public GateNode model;
    private Action onValueChanged;

    /// <summary>
    /// Associa o modelo pré-criado pelo CircuitManager.
    /// </summary>
    public void SetModel(GateNode node)
    {
        model = node;
    }

    /// <summary>
    /// Configura apenas a parte visual (sockets, textos, callback),
    /// sem mexer no model.
    /// </summary>
    public void InitializeView(GateNode.GateType type, int inputCount, Action onValueChanged)
    {
        this.onValueChanged = onValueChanged;

        if (identifierText != null)
            identifierText.text = type.ToString();

        // gera sockets dinamicamente à esquerda
        if (socketContainer != null && inputSocketPrefab != null)
        {
            foreach (Transform c in socketContainer)
                Destroy(c.gameObject);

            for (int i = 0; i < inputCount; i++)
            {
                var sock = Instantiate(inputSocketPrefab, socketContainer);
                float z = (i - (inputCount - 1) * 0.5f) * socketSpacingZ;
                sock.transform.localPosition = new Vector3(-inputOffsetX, 0, z);
            }
        }
    }

    public int Evaluate()
    {
        return model.Evaluate();
    }

    public void UpdateOutputHint(int? value)
    {
        if (outputHintText == null) return;
        outputHintText.text = value.HasValue ? value.Value.ToString() : "";
    }

    public Transform[] GetInputSockets()
    {
        if (socketContainer == null) return new Transform[0];
        var arr = new List<Transform>();
        foreach (Transform c in socketContainer)
            arr.Add(c);
        return arr.ToArray();
    }

    public void NotifyValueChanged() => onValueChanged?.Invoke();
}
