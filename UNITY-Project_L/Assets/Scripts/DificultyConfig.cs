using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyConfig", menuName = "LogicGame/DifficultyConfig", order = 1)]
public class DifficultyConfig : ScriptableObject
{
    [Header("General Depth Settings")]
    [Tooltip("Número máximo de camadas (profundidade) que o circuito pode ter")]
    public int maxDepth = 4;

    [Header("Gate Input Settings")]
    [Tooltip("Mínimo de entradas por gate (exceto NOT)")]
    public int minInputs = 2;
    [Tooltip("Máximo de entradas por gate (exceto NOT)")]
    public int maxInputs = 3;

    [Header("Gates per Layer")]
    [Tooltip("Número mínimo de gates em cada camada")]
    public int minGatesPerLayer = 1;
    [Tooltip("Número máximo de gates em cada camada")]
    public int maxGatesPerLayer = 3;

    [Header("Fan-Out (Reuso de Inputs)")]
    [Tooltip("Chance de reaproveitar um mesmo InputNode em vez de criar um novo, por camada")]
    public AnimationCurve fanOutChanceByDepth = new AnimationCurve(
        new Keyframe(1, 0f),
        new Keyframe(2, 0.1f),
        new Keyframe(3, 0.2f),
        new Keyframe(4, 0.4f),
        new Keyframe(5, 0.6f)
    );

    [Header("Shortcuts (Atalhos)")]
    [Tooltip("Chance de ligar um gate a um nó duas camadas abaixo, por camada")]
    public AnimationCurve shortcutChanceByDepth = new AnimationCurve(
        new Keyframe(1, 0f),
        new Keyframe(2, 0.05f),
        new Keyframe(3, 0.1f),
        new Keyframe(4, 0.2f),
        new Keyframe(5, 0.3f)
    );

    [Header("Loops (Feedback)")]
    [Tooltip("Permite criação de ciclos (loops) no circuito")]
    public bool allowLoops = false;
    [Tooltip("Chance de tentar adicionar um loop em cada camada")]
    public AnimationCurve loopChanceByDepth = new AnimationCurve(
        new Keyframe(1, 0f),
        new Keyframe(2, 0.02f),
        new Keyframe(3, 0.05f),
        new Keyframe(4, 0.1f),
        new Keyframe(5, 0.15f)
    );
    [Tooltip("Comprimento máximo permitido de qualquer ciclo (em número de arestas)")]
    public int maxLoopLength = 3;
}
