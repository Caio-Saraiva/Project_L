// CircuitDefinition.cs
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCircuit",
    menuName = "LogicGame/CircuitDefinition",
    order = 10)]
public class CircuitDefinition : ScriptableObject
{
    [Tooltip("Sequência de portas que compõem este circuito")]
    public GateType[] gates;

    [Tooltip("Valores iniciais de cada entrada (0 ou 1)")]
    public int[] inputs;

    [Tooltip("Valor esperado de saída")]
    public int output;

    [Tooltip("Como exibir os labels: 0/1, True/False ou High/Low")]
    public LabelMode labelMode;
}
