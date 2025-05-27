using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct LeaderboardEntry
{
    public string initials;
    public int score;
    public int level;
}

[System.Serializable]
class LeaderboardData
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

/// <summary>
/// Gerencia o top N de scores em PlayerPrefs e atualiza o ScrollView.
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    [Header("Configuração")]
    public string prefsKey = "Leaderboard";
    public int maxEntries = 10;

    [Header("UI")]
    public Transform contentParent;  // Content do ScrollView
    public GameObject entryPrefab;   // Prefab com LeaderboardEntryUI

    private LeaderboardData data;

    void Awake()
    {
        // Carrega ao inicializar
        LoadRank();
        UpdateRank();
    }

    /// <summary>Carrega do PlayerPrefs.</summary>
    public void LoadRank()
    {
        if (PlayerPrefs.HasKey(prefsKey))
            data = JsonUtility.FromJson<LeaderboardData>(
                PlayerPrefs.GetString(prefsKey)
            );
        else
            data = new LeaderboardData();
    }

    /// <summary>Grava no PlayerPrefs.</summary>
    private void Save()
    {
        PlayerPrefs.SetString(prefsKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Retorna true se houver espaço ou se este score/level for melhor que o último do top.
    /// </summary>
    public bool CanAddEntry(int score, int level)
    {
        if (data.entries.Count < maxEntries)
            return true;
        var last = data.entries[data.entries.Count - 1];
        if (score > last.score) return true;
        if (score == last.score && level > last.level) return true;
        return false;
    }

    /// <summary>
    /// Tenta inserir um novo registro; retorna true se entrou no ranking.
    /// </summary>
    public bool AddEntry(string initials, int score, int level)
    {
        if (!CanAddEntry(score, level))
            return false;

        data.entries.Add(new LeaderboardEntry
        {
            initials = initials,
            score = score,
            level = level
        });

        // Ordena decrescente por score e level
        data.entries.Sort((a, b) => {
            int c = b.score.CompareTo(a.score);
            return (c != 0) ? c : b.level.CompareTo(a.level);
        });

        // Mantém apenas o top N
        if (data.entries.Count > maxEntries)
            data.entries.RemoveRange(
                maxEntries,
                data.entries.Count - maxEntries
            );

        Save();
        UpdateRank();
        return true;
    }

    /// <summary>
    /// Recria todas as linhas no ScrollView (chame sempre após AddEntry ou LoadRank).
    /// </summary>
    public void UpdateRank()
    {
        // limpa antigas
        foreach (Transform c in contentParent)
            Destroy(c.gameObject);

        // instancia novas com índice
        for (int i = 0; i < data.entries.Count; i++)
        {
            var entry = data.entries[i];
            var go = Instantiate(entryPrefab, contentParent);
            var ui = go.GetComponent<LeaderboardEntryUI>();
            ui.Initialize(i + 1, entry);
        }
    }
}
