using UnityEngine;

public static class GateLogic
{
    /// <summary>
    /// Avalia a lógica de uma porta para entradas a e b (0 ou 1).
    /// Para NOT, apenas “a” é considerado.
    /// </summary>
    public static int Evaluate(GateType type, int a, int b)
    {
        switch (type)
        {
            case GateType.AND:
                return (a == 1 && b == 1) ? 1 : 0;

            case GateType.NAND:
                return (a == 1 && b == 1) ? 0 : 1;

            case GateType.OR:
                return (a == 1 || b == 1) ? 1 : 0;

            case GateType.NOR:
                return (a == 1 || b == 1) ? 0 : 1;

            case GateType.XOR:
                return (a + b) % 2;  // 1 se exatamente um for 1

            case GateType.XNOR:
                return ((a + b) % 2 == 0) ? 1 : 0;

            case GateType.NOT:
                return (a == 0) ? 1 : 0;

            default:
                Debug.LogWarning($"GateLogic: tipo desconhecido {type}");
                return 0;
        }
    }
}
