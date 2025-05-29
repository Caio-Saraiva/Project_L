using UnityEngine;
using TMPro;

public enum ValueDisplayMode
{
    Bit,    // “0” / “1”
    Bool,   // “FALSE” / “TRUE”
    Signal  // “LOW” / “HIGH”
}

public class TruthTableUI : MonoBehaviour
{
    [Header("Seleção de Porta")]
    public GateType selectedGate;

    [Header("Como Exibir Valores")]
    [Tooltip("Define se mostra como bit, bool ou signal")]
    public ValueDisplayMode displayMode = ValueDisplayMode.Bit;

    [Header("Campo de Nome da Porta")]
    public TextMeshProUGUI gateNameText;

    [Header("Tabela 2-Input (4 linhas: 00,01,10,11)")]
    public TextMeshProUGUI[] inputA = new TextMeshProUGUI[4];
    public TextMeshProUGUI[] inputB = new TextMeshProUGUI[4];
    public TextMeshProUGUI[] output = new TextMeshProUGUI[4];

    [Header("Tabela NOT (2 linhas: A=0,A=1)")]
    [Tooltip("2 entradas A para NOT")]
    public TextMeshProUGUI[] notInput = new TextMeshProUGUI[2];
    [Tooltip("2 saídas para NOT")]
    public TextMeshProUGUI[] notOutput = new TextMeshProUGUI[2];

    void OnValidate()
    {
        UpdateTable();
    }

    /// <summary>
    /// Chamada por UI para mostrar valores como bits (“0”/“1”).
    /// </summary>
    public void ShowAsBits()
    {
        displayMode = ValueDisplayMode.Bit;
        UpdateTable();
    }

    /// <summary>
    /// Chamada por UI para mostrar valores como bool (“FALSE”/“TRUE”).
    /// </summary>
    public void ShowAsBool()
    {
        displayMode = ValueDisplayMode.Bool;
        UpdateTable();
    }

    /// <summary>
    /// Chamada por UI para mostrar valores como signal (“LOW”/“HIGH”).
    /// </summary>
    public void ShowAsSignal()
    {
        displayMode = ValueDisplayMode.Signal;
        UpdateTable();
    }

    /// <summary>
    /// Atualiza toda a tabela de verdade conforme a porta e o modo de exibição.
    /// </summary>
    public void UpdateTable()
    {
        // Nome
        if (gateNameText != null)
            gateNameText.text = selectedGate.ToString();

        bool isNot = selectedGate == GateType.NOT;

        // ativa/desativa blocos
        SetActiveArray(inputA, !isNot);
        SetActiveArray(inputB, !isNot);
        SetActiveArray(output, !isNot);
        SetActiveArray(notInput, isNot);
        SetActiveArray(notOutput, isNot);

        if (!isNot)
        {
            // 4 combinações de 2 inputs
            for (int i = 0; i < 4; i++)
            {
                int a = (i >> 1) & 1;
                int b = i & 1;
                int r = GateLogic.Evaluate(selectedGate, a, b);

                inputA[i].text = FormatValue(a);
                inputB[i].text = FormatValue(b);
                output[i].text = FormatValue(r);
            }
        }
        else
        {
            // NOT: apenas 2 linhas
            for (int i = 0; i < 2; i++)
            {
                int a = i;
                int r = GateLogic.Evaluate(GateType.NOT, a, 0);

                notInput[i].text = FormatValue(a);
                notOutput[i].text = FormatValue(r);
            }
        }
    }

    /// <summary>
    /// Converte 0/1 para a string adequada ao modo corrente.
    /// </summary>
    string FormatValue(int v)
    {
        switch (displayMode)
        {
            case ValueDisplayMode.Bool:
                return (v == 1) ? "TRUE" : "FALSE";

            case ValueDisplayMode.Signal:
                return (v == 1) ? "HIGH" : "LOW";

            case ValueDisplayMode.Bit:
            default:
                return v.ToString();
        }
    }

    void SetActiveArray(TextMeshProUGUI[] arr, bool active)
    {
        foreach (var t in arr)
            if (t != null)
                t.gameObject.SetActive(active);
    }
}
