using System.Collections.Generic;
using System.Linq;

public class GateNode
{
    public enum GateType { AND, NAND, OR, NOR, NOT, XOR, XNOR }
    public GateType Type;
    public List<GateNode> Inputs = new List<GateNode>();
    public List<GateNode> Outputs = new List<GateNode>();

    // Para StaticValueNode
    public bool IsStaticValue = false;
    public int StaticValue = 0;

    /// <summary>
    /// Calcula recursivamente o valor de saída desse nó, cuidando de listas vazias.
    /// </summary>
    public int Evaluate()
    {
        // Nodo estático ou input clicado
        if (IsStaticValue)
            return StaticValue;

        // Recolhe valores das entradas
        var inVals = Inputs.Select(i => i.Evaluate()).ToList();

        switch (Type)
        {
            case GateType.AND:
                // AND de zero inputs = 0 (pode mudar conforme sua regra)
                return (inVals.Count > 0 && inVals.All(v => v == 1)) ? 1 : 0;

            case GateType.NAND:
                return (inVals.Count > 0 && inVals.All(v => v == 1)) ? 0 : 1;

            case GateType.OR:
                // OR de zero inputs = 0
                return inVals.Any(v => v == 1) ? 1 : 0;

            case GateType.NOR:
                return inVals.Any(v => v == 1) ? 0 : 1;

            case GateType.XOR:
                // XOR de zero inputs = 0
                int xr = 0;
                foreach (var v in inVals) xr ^= v;
                return xr;

            case GateType.XNOR:
                // XNOR de zero inputs = 1 (inverso do XOR)
                int xr2 = 0;
                foreach (var v in inVals) xr2 ^= v;
                return xr2 == 1 ? 0 : 1;

            case GateType.NOT:
                // NOT sempre tem exatamente 1 input
                return (inVals.Count > 0 && inVals[0] == 1) ? 0 : 1;

            default:
                return 0;
        }
    }
}
