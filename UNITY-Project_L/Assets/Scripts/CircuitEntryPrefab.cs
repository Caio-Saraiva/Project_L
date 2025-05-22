using UnityEngine;

[System.Serializable]
public class CircuitPrefabEntry
{
    [Tooltip("Prefab de CircuitCardView a ser instanciado")]
    public CircuitCardView prefab;

    [Tooltip("Level mínimo para que este prefab possa aparecer")]
    public int minLevel = 1;

    [Header("Chance de Spawn (%)")]
    [Range(0, 100)]
    public int spawnChance = 100;

    [Header("Peso de Pontuação")]
    [Tooltip("Multiplicador de pontos ganhos ao resolver esta fase")]
    [Min(0.1f)]
    public float scoreMultiplier = 1f;
}
