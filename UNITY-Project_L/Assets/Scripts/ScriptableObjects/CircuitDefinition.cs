// CircuitDefinition.cs
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCircuit",
    menuName = "LogicGame/CircuitDefinition",
    order = 10)]
public class CircuitDefinition : ScriptableObject
{
    [Tooltip("Sequ�ncia de portas que comp�em este circuito")]
    public GateType[] gates;

    [Tooltip("Valores iniciais de cada entrada (0 ou 1)")]
    public int[] inputs;

    [Tooltip("Valor esperado de sa�da")]
    public int output;

    [Tooltip("Como exibir os labels: 0/1, True/False ou High/Low")]
    public LabelMode labelMode;
}
