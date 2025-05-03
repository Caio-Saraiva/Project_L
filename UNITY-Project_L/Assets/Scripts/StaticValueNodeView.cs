using UnityEngine;
using TMPro;

public class StaticValueNodeView : MonoBehaviour
{
    public TextMeshProUGUI valueText;

    [HideInInspector] public GateNode model;

    /// <summary>
    /// Associa o nó de dados a este view.
    /// </summary>
    public void SetModel(GateNode model)
    {
        this.model = model;
    }

    /// <summary>
    /// Inicializa este nó com um valor fixo (0 ou 1).
    /// </summary>
    public void Initialize(int value)
    {
        // Se ainda não tiver modelo, cria um
        if (model == null)
            model = new GateNode();

        model.IsStaticValue = true;
        model.StaticValue = value;

        if (valueText != null)
            valueText.text = value.ToString();
    }

    /// <summary>
    /// Destaca sucesso quando o puzzle for resolvido corretamente.
    /// </summary>
    public void HighlightSuccess()
    {
        if (valueText != null)
            valueText.color = Color.green;
    }
}
