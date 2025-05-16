using UnityEngine;

/// <summary>
/// Tipos de portas l�gicas suportadas no circuito.
/// A ordem deve corresponder � posi��o dos prefabs de porta, se voc� usar um array.
/// </summary>
public enum GateType
{
    AND,
    NAND,
    OR,
    NOR,
    NOT,
    XOR,
    XNOR
}
