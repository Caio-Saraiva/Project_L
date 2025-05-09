// CircuitLevel.cs
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCircuitLevel",
    menuName = "LogicGame/CircuitLevel",
    order = 20)]
public class CircuitLevel : ScriptableObject
{
    [Header("Tutorial Cards")]
    [Tooltip("Cards manuais para os primeiros níveis")]
    public CircuitDefinition[] tutorialCards;

    [Header("Procedural Settings")]
    [Tooltip("Gerar proceduralmente após o tutorial?")]
    public bool useProcedural;

    [Tooltip("Número máximo de gates em cada circuito procedural")]
    public int maxGates = 3;

    [Tooltip("Modos de label permitidos neste nível")]
    public LabelMode[] allowedLabels;

    [Header("Tempo e Pontuação")]
    public float baseTime = 15f;
    public float bonusTime = 5f;
    public float penaltyTime = 5f;

    public int basePoints = 10;
    public int bonusPoints = 5;
    public int penaltyPoints = -5;
}
