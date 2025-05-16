using UnityEngine;

/// <summary>
/// Tipos de portas lógicas suportadas no circuito.
/// A ordem deve corresponder à posição dos prefabs de porta, se você usar um array.
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
