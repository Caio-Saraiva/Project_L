using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class SlotConfig
{
    [Tooltip("GameObject que representa o slot (para posicionamento)")]
    public GameObject slotObject;

    [Tooltip("Image onde vamos pintar a sprite da porta")]
    public Image slotImage;

    [Tooltip("Input A deste slot")]
    public GameObject inputSourceA;

    [Tooltip("Input B deste slot")]
    public GameObject inputSourceB;

    [HideInInspector] public GateType assignedGate;

    [Header("Debug (preenchido em Initialize)")]
    [Tooltip("Para debug: nome do gate atribuído")]
    public string gateName;

    [HideInInspector] public int outputValue;
}

public class CircuitCardSetup : MonoBehaviour
{
    [Header("Entradas (filhos com TextMeshProUGUI)")]
    public List<GameObject> inputObjects;

    [Header("Slots de Gate")]
    public List<SlotConfig> slots;

    [Header("Saída (filho com TextMeshProUGUI)")]
    public GameObject outputObject;

    [Header("Sprites de Portas (AND, NAND, OR, NOR, NOT, XOR, XNOR)")]
    [Tooltip("Arraste 7 sprites na ordem do enum GateType")]
    public Sprite[] gateSprites;

    [Header("Portas Permitidas")]
    public bool allowAND = true;
    public bool allowNAND = true;
    public bool allowOR = true;
    public bool allowNOR = true;
    public bool allowNOT = true;
    public bool allowXOR = true;
    public bool allowXNOR = true;

    [Header("Randomizar Formato de Rótulo")]
    public bool randomLabelMode = true;
    public LabelMode labelMode = LabelMode.Bit;

    [HideInInspector] public List<int> inputValues;
    [HideInInspector] public int expectedOutput;

    public void Initialize()
    {
        // 1) Escolhe labelMode
        if (randomLabelMode)
        {
            var modes = System.Enum.GetValues(typeof(LabelMode));
            labelMode = (LabelMode)modes.GetValue(
                Random.Range(0, modes.Length)
            );
        }

        // 2) Randomiza valores de entrada e atualiza o texto
        inputValues = new List<int>();
        for (int i = 0; i < inputObjects.Count; i++)
        {
            int v = Random.Range(0, 2);
            inputValues.Add(v);
            var txt = inputObjects[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = FormatValue(v);
        }

        // 3) Prepara lista de tipos permitidos
        var allowed = new List<GateType>();
        if (allowAND) allowed.Add(GateType.AND);
        if (allowNAND) allowed.Add(GateType.NAND);
        if (allowOR) allowed.Add(GateType.OR);
        if (allowNOR) allowed.Add(GateType.NOR);
        if (allowNOT) allowed.Add(GateType.NOT);
        if (allowXOR) allowed.Add(GateType.XOR);
        if (allowXNOR) allowed.Add(GateType.XNOR);

        // 4) Cria mapeamento slotObject → SlotConfig
        var slotMap = new Dictionary<GameObject, SlotConfig>();
        foreach (var slot in slots)
            slotMap[slot.slotObject] = slot;

        // 5) Para cada slot: escolhe gate, sprite, nome e calcula outputValue
        foreach (var slot in slots)
        {
            // Sorteia o tipo de porta
            var choice = allowed[Random.Range(0, allowed.Count)];
            slot.assignedGate = choice;
            slot.gateName = choice.ToString();

            // Atribui sprite no Image do slot
            if (slot.slotImage != null
             && gateSprites != null
             && (int)choice < gateSprites.Length)
            {
                slot.slotImage.sprite = gateSprites[(int)choice];
                slot.slotImage.color = Color.white;
            }

            // Limpa texto antigo se houver
            var txt = slot.slotObject.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = "";

            // Obtém valores de entrada A e B
            int inA = GetSourceValue(slot.inputSourceA, slotMap);
            int inB = (slot.assignedGate == GateType.NOT)
                    ? 0
                    : GetSourceValue(slot.inputSourceB, slotMap);

            // Avalia a porta
            slot.outputValue = EvaluateGate(choice, inA, inB);
        }

        // 6) A saída esperada é o outputValue do último slot
        if (slots.Count > 0)
            expectedOutput = slots[slots.Count - 1].outputValue;
        else
            expectedOutput = 0;

        // Limpa o texto de saída
        var outTxt = outputObject.GetComponentInChildren<TextMeshProUGUI>();
        if (outTxt != null) outTxt.text = "";
    }

    // Retorna 0/1 para um GameObject de input ou slot
    private int GetSourceValue(GameObject src, Dictionary<GameObject, SlotConfig> slotMap)
    {
        int idx = inputObjects.IndexOf(src);
        if (idx != -1) return inputValues[idx];
        if (slotMap.TryGetValue(src, out var slot))
            return slot.outputValue;
        Debug.LogWarning($"CircuitCardSetup: fonte desconhecida {src.name}");
        return 0;
    }

    // Lógica das portas
    private int EvaluateGate(GateType type, int a, int b)
    {
        switch (type)
        {
            case GateType.AND: return (a & b) == 1 ? 1 : 0;
            case GateType.NAND: return (a & b) == 1 ? 0 : 1;
            case GateType.OR: return (a | b) == 1 ? 1 : 0;
            case GateType.NOR: return (a | b) == 1 ? 0 : 1;
            case GateType.XOR: return (a ^ b);
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
