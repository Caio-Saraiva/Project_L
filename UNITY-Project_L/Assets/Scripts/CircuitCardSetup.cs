using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SlotConfig
{
    [Tooltip("GameObject que representa o slot (para posicionamento)")]
    public GameObject slotObject;

    [Tooltip("Image onde vamos pintar a sprite da porta")]
    public Image slotImage;

    [Tooltip("Input A deste slot (pode ser um inputObject ou outro slotObject)")]
    public GameObject inputSourceA;

    [Tooltip("Input B deste slot (pode ser um inputObject ou outro slotObject)")]
    public GameObject inputSourceB;

    [Header("Forçar NOT Gate?")]
    [Tooltip("Se verdadeiro, este slot sempre usará uma porta NOT, ignorando as permitidas")]
    public bool onlyNotGate = false;

    // Dados gerados em runtime
    [HideInInspector] public GateType assignedGate;
    [HideInInspector] public string gateName;
    [HideInInspector] public int outputValue;
    [HideInInspector] public bool isComputed;
}

public class CircuitCardSetup : MonoBehaviour
{
    [Header("Entradas (cada filho deve ter um TextMeshProUGUI)")]
    public List<GameObject> inputObjects;

    [Header("Slots de Gate")]
    public List<SlotConfig> slots;

    [Header("Output Connector (slot que alimenta a saída)")]
    [Tooltip("Arraste aqui o GameObject do slot cuja saída é a saída final")]
    public GameObject outputSource;

    [Header("Saída (filho com TextMeshProUGUI)")]
    [Tooltip("Este campo não deve ser usado para mostrar o resultado esperado.")]
    public GameObject outputObject;

    [Header("Sprites de Portas (AND, NAND, OR, NOR, NOT, XOR, XNOR)")]
    [Tooltip("Arraste 7 sprites na mesma ordem do enum GateType")]
    public Sprite[] gateSprites;

    [Header("Tint da Porta")]
    [Tooltip("Cor aplicada a todas as sprites de gate")]
    public Color gateSpriteColor = Color.white;

    [Header("Portas Permitidas (quando onlyNotGate == false)")]
    public bool allowAND = true;
    public bool allowNAND = true;
    public bool allowOR = true;
    public bool allowNOR = true;
    public bool allowNOT = true;
    public bool allowXOR = true;
    public bool allowXNOR = true;

    [Header("Randomizar Formato de Rótulo (apenas UI)")]
    public bool randomLabelMode = true;
    public LabelMode labelMode = LabelMode.Bit;

    // Valores calculados em runtime
    [HideInInspector] public List<int> inputValues;
    [HideInInspector] public int expectedOutput;

    /// <summary>
    /// Chame este método para gerar um novo circuito:
    /// sorteia modelo, valores, atribui sprites e calcula saída esperada.
    /// </summary>
    public void Initialize()
    {
        // 0) Resetar flags de cálculo
        foreach (var s in slots)
            s.isComputed = false;

        // 1) Escolher labelMode (UI)
        if (randomLabelMode)
        {
            var modes = System.Enum.GetValues(typeof(LabelMode));
            labelMode = (LabelMode)modes.GetValue(
                Random.Range(0, modes.Length)
            );
        }

        // 2) Gerar e exibir valores de entrada
        inputValues = new List<int>();
        for (int i = 0; i < inputObjects.Count; i++)
        {
            int v = Random.Range(0, 2);
            inputValues.Add(v);
            var txt = inputObjects[i]
                      .GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = FormatValue(v);
        }

        // 3) Montar pool de gates permitidos
        var allowed = new List<GateType>();
        if (allowAND) allowed.Add(GateType.AND);
        if (allowNAND) allowed.Add(GateType.NAND);
        if (allowOR) allowed.Add(GateType.OR);
        if (allowNOR) allowed.Add(GateType.NOR);
        if (allowNOT) allowed.Add(GateType.NOT);
        if (allowXOR) allowed.Add(GateType.XOR);
        if (allowXNOR) allowed.Add(GateType.XNOR);

        // 4) Atribuir randomicamente gateType, sprite e cor
        foreach (var slot in slots)
        {
            GateType choice = slot.onlyNotGate
                ? GateType.NOT
                : (allowed.Count > 0
                    ? allowed[Random.Range(0, allowed.Count)]
                    : GateType.NOT);

            slot.assignedGate = choice;
            slot.gateName = choice.ToString();

            if (slot.slotImage != null
             && gateSprites != null
             && (int)choice < gateSprites.Length)
            {
                slot.slotImage.sprite = gateSprites[(int)choice];
                slot.slotImage.color = gateSpriteColor;
            }
        }

        // 5) Avaliar todo o grafo de portas
        foreach (var slot in slots)
            EvaluateSlot(slot);

        // 6) Obter saída esperada
        var outSlot = slots.FirstOrDefault(s => s.slotObject == outputSource);
        expectedOutput = outSlot != null ? outSlot.outputValue : 0;

        // Não atribuímos mais texto ao outputObject aqui!
        // A exibição só deve vir via CircuitCardView após o player carimbar.

        Debug.Log($"[Setup] labelMode={labelMode}, expectedOutput={expectedOutput}");
    }

    /// <summary>
    /// Avalia recursivamente o slot até obter seu outputValue.
    /// </summary>
    private void EvaluateSlot(SlotConfig slot)
    {
        if (slot.isComputed) return;

        int a = GetValueFromSource(slot.inputSourceA);
        int b = GetValueFromSource(slot.inputSourceB);

        // Executa a lógica da porta
        slot.outputValue = GateLogic.Evaluate(slot.assignedGate, a, b);
        slot.isComputed = true;
    }

    /// <summary>
    /// Retorna o valor (0 ou 1) vindo de uma fonte,
    /// que pode ser um inputObjects ou outro slotObject.
    /// </summary>
    private int GetValueFromSource(GameObject source)
    {
        // Se vier de um input inicial
        int idx = inputObjects.IndexOf(source);
        if (idx >= 0) return inputValues[idx];

        // Se vier de um slot de saída
        var slot = slots.FirstOrDefault(s => s.slotObject == source);
        if (slot != null)
        {
            EvaluateSlot(slot);
            return slot.outputValue;
        }

        // Fallback
        return 0;
    }

    /// <summary>
    /// Formata o valor de acordo com o labelMode (Bit, Bool, Signal).
    /// </summary>
    private string FormatValue(int v)
    {
        switch (labelMode)
        {
            case LabelMode.Bool: return v == 1 ? "TRUE" : "FALSE";
            case LabelMode.Signal: return v == 1 ? "HIGH" : "LOW";
            default: return v.ToString();
        }
    }
}
