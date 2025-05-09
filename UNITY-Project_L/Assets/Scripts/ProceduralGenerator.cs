using UnityEngine;

public static class ProceduralGenerator
{
    /// <summary>
    /// Gera um CircuitDefinition procedurally com base nos par�metros do level.
    /// No momento, retorna apenas o primeiro tutorial card (stub).
    /// </summary>
    public static CircuitDefinition Generate(CircuitLevel level)
    {
        if (level.tutorialCards != null && level.tutorialCards.Length > 0)
            return level.tutorialCards[0];

        Debug.LogWarning("ProceduralGenerator: sem tutorialCards, retornando inst�ncia vazia.");
        return ScriptableObject.CreateInstance<CircuitDefinition>();
    }
}
