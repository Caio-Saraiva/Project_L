using UnityEngine;
using TMPro;

/// <summary>
/// Script do prefab de linha no ScrollView.
/// </summary>
public class LeaderboardEntryUI : MonoBehaviour
{
    public TextMeshProUGUI indexText;
    public TextMeshProUGUI initialsText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;

    /// <summary>
    /// Inicializa a linha: rank (1..N), dados do entry.
    /// </summary>
    public void Initialize(int rank, LeaderboardEntry e)
    {
        indexText.text = rank.ToString();
        initialsText.text = e.initials;
        scoreText.text = e.score.ToString();
        levelText.text = $"LEVEL {e.level.ToString()}";
    }
}
