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

    [Tooltip("Input A deste slot (se for ligado a um input do jogador ou a saída de outro slot)")]
    public GameObject inputSourceA;

    [Tooltip("Input B deste slot")]
    public GameObject inputSourceB;

    [HideInInspector]
    public GateType assignedGate;
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

    /// <summary>
    /// Deve ser chamado pelo GameManager após Instantiate.
    /// </summary>
    public void Initialize()
    {
        // 1) escolhe labelMode
        if (randomLabelMode)
        {
            var modes = System.Enum.GetValues(typeof(LabelMode));
            labelMode = (LabelMode)modes.GetValue(
                Random.Range(0, modes.Length)
            );
        }

        // 2) randomiza valores de entrada e atualiza o texto
        inputValues = new List<int>(inputObjects.Count);
        for (int i = 0; i < inputObjects.Count; i++)
        {
            int v = Random.Range(0, 2);
            inputValues.Add(v);
            var txt = inputObjects[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = FormatValue(v);
        }

        // 3) monta lista de GateType permitidos
        var allowed = new List<GateType>();
        if (allowAND) allowed.Add(GateType.AND);
        if (allowNAND) allowed.Add(GateType.NAND);
        if (allowOR) allowed.Add(GateType.OR);
        if (allowNOR) allowed.Add(GateType.NOR);
        if (allowNOT) allowed.Add(GateType.NOT);
        if (allowXOR) allowed.Add(GateType.XOR);
        if (allowXNOR) allowed.Add(GateType.XNOR);

        // 4) para cada slot, escolhe um gate aleatório e atribui a sprite correta
        foreach (var slot in slots)
        {
            // sorteia o tipo de porta
            var choice = allowed[Random.Range(0, allowed.Count)];
            slot.assignedGate = choice;

            // aplica a sprite no Image do slot
            if (slot.slotImage != null
                && gateSprites != null
                && (int)choice < gateSprites.Length)
            {
                slot.slotImage.sprite = gateSprites[(int)choice];
                slot.slotImage.color = Color.white;
            }

            // opcional: limpa qualquer texto antigo no slotObject
            var txt = slot.slotObject.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = "";
        }

        // 5) pré-calcula saída esperada (implemente seu grafo aqui)
        // Por enquanto, sempre 0:
        expectedOutput = 0;

        // limpa o texto de saída
        var outTxt = outputObject.GetComponentInChildren<TextMeshProUGUI>();
        if (outTxt != null) outTxt.text = "";
    }

    private string FormatValue(int v)
    {
        switch (labelMode)
        {
            case LabelMode.Bool: return v == 1 ? "True" : "False";
            case LabelMode.Signal: return v == 1 ? "High" : "Low";
            default: return v.ToString();
        }
    }
}
