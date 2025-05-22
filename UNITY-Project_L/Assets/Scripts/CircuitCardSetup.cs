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

    [Tooltip("Input A deste slot")]
    public GameObject inputSourceA;

    [Tooltip("Input B deste slot)")]
    public GameObject inputSourceB;

    [Header("Forçar NOT Gate?")]
    [Tooltip("Se verdadeiro, este slot sempre usará uma porta NOT")]
    public bool onlyNotGate = false;

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
    public GameObject outputObject;

    [Header("Sprites de Portas (AND, NAND, OR, NOR, NOT, XOR, XNOR)")]
    [Tooltip("Arraste 7 sprites na mesma ordem do enum GateType")]
    public Sprite[] gateSprites;

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

    [HideInInspector] public List<int> inputValues;
    [HideInInspector] public int expectedOutput;

    public void Initialize()
    {
        // 0) Resetar flags
        foreach (var s in slots)
            s.isComputed = false;

        // 1) Escolher labelMode (apenas para UI)
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

        // 4) Sortear tipo + sprite + gateName
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
                slot.slotImage.color = Color.white;
            }
        }

        // 5) Avaliar todo o grafo topologicamente
        var pending = new List<SlotConfig>(slots);
        var map = slots.ToDictionary(s => s.slotObject, s => s);

        while (pending.Count > 0)
        {
            bool didCompute = false;

            for (int i = pending.Count - 1; i >= 0; i--)
            {
                var slot = pending[i];

                bool aReady = inputObjects.Contains(slot.inputSourceA)
                           || (map.TryGetValue(slot.inputSourceA, out var sa) && sa.isComputed);
                bool bReady = slot.assignedGate == GateType.NOT
                           || inputObjects.Contains(slot.inputSourceB)
                           || (map.TryGetValue(slot.inputSourceB, out var sb) && sb.isComputed);

                if (aReady && bReady)
                {
                    int a = GetSourceValue(slot.inputSourceA, map);
                    int b = slot.assignedGate == GateType.NOT
                            ? 0
                            : GetSourceValue(slot.inputSourceB, map);

                    slot.outputValue = EvaluateGate(slot.assignedGate, a, b);
                    slot.isComputed = true;
                    pending.RemoveAt(i);
                    didCompute = true;
                }
            }

            if (!didCompute)
            {
                Debug.LogWarning("CircuitCardSetup: circuito cíclico ou inválido");
                break;
            }
        }

        // 6) Definir expectedOutput a partir do outputSource
        if (outputSource != null && map.TryGetValue(outputSource, out var outSlot))
        {
            expectedOutput = outSlot.outputValue;
        }
        else if (slots.Count > 0)
        {
            expectedOutput = slots[slots.Count - 1].outputValue;
        }
        else
        {
            expectedOutput = 0;
        }

        Debug.Log($"[Setup] labelMode={labelMode}, expectedOutput(bit)={expectedOutput}");

        // **NÃO** preenche o UI de saída aqui: ficará vazio até o jogador carimbar
    }

    private int GetSourceValue(
        GameObject src,
        Dictionary<GameObject, SlotConfig> map)
    {
        int idx = inputObjects.IndexOf(src);
        if (idx >= 0) return inputValues[idx];
        if (map.TryGetValue(src, out var slot)) return slot.outputValue;

        Debug.LogWarning($"CircuitCardSetup: fonte '{src.name}' não mapeada");
        return 0;
    }

    private int EvaluateGate(GateType t, int a, int b)
    {
        switch (t)
        {
            case GateType.AND: return a & b;
            case GateType.NAND: return (a & b) == 1 ? 0 : 1;
            case GateType.OR: return a | b;
            case GateType.NOR: return (a | b) == 1 ? 0 : 1;
            case GateType.XOR: return a ^ b;
            case GateType.XNOR: return (a ^ b) == 1 ? 0 : 1;
            case GateType.NOT: return a == 1 ? 0 : 1;
            default: return 0;
        }
    }

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
