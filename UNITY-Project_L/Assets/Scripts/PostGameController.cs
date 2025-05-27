using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PostGameController : MonoBehaviour
{
    [Header("Referências Externas")]
    [Tooltip("Aponta para o GameManager que guarda score e level")]
    public GameManager gameManager;
    [Tooltip("Gerenciador de leaderboard")]
    public LeaderboardManager leaderboardManager;

    [Header("Resultado Final")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalLevelText;

    [Header("Campos de Inicial")]
    [Tooltip("Os 3 TMPs que mostram cada letra (A–Z)")]
    public TextMeshProUGUI[] letterFields = new TextMeshProUGUI[3];

    [Header("Controles de Ciclagem")]
    public Button[] upButtons = new Button[3];
    public Button[] downButtons = new Button[3];

    int[] letterIdx = new int[3];

    void Awake()
    {
        // Inicializa A A A e conecta botões
        for (int i = 0; i < 3; i++)
        {
            letterIdx[i] = 0;
            letterFields[i].text = "A";

            int idx = i;
            upButtons[i].onClick.AddListener(() => LetterUp(idx));
            downButtons[i].onClick.AddListener(() => LetterDown(idx));
        }
    }

    /// <summary>
    /// Deve ser chamado quando o jogo acabar.
    /// Atualiza apenas os textos de score e level, e reseta as iniciais.
    /// </summary>
    public void ShowPostGame()
    {
        finalScoreText.text = $"SCORE: {gameManager.score}";
        finalLevelText.text = $"LEVEL: {gameManager.level}";
        ResetLetters();
    }

    /// <summary>Incrementa A→B→…→Z→A no campo dado.</summary>
    public void LetterUp(int idx)
    {
        letterIdx[idx] = (letterIdx[idx] + 1) % 26;
        letterFields[idx].text = ((char)('A' + letterIdx[idx])).ToString();
    }

    /// <summary>Decrementa A←Z←…←B←A no campo dado.</summary>
    public void LetterDown(int idx)
    {
        letterIdx[idx] = (letterIdx[idx] + 25) % 26;
        letterFields[idx].text = ((char)('A' + letterIdx[idx])).ToString();
    }

    /// <summary>Reseta todas as iniciais de volta para "A".</summary>
    public void ResetLetters()
    {
        for (int i = 0; i < 3; i++)
        {
            letterIdx[i] = 0;
            letterFields[i].text = "A";
        }
    }

    /// <summary>
    /// Concatena as iniciais + score + level e tenta inserir no top-10.
    /// </summary>
    public void RankScore()
    {
        string initials =
            letterFields[0].text +
            letterFields[1].text +
            letterFields[2].text;

        int sc = gameManager.score;
        int lv = gameManager.level;

        bool entered = leaderboardManager.AddEntry(initials, sc, lv);
        if (!entered)
            Debug.Log("Não entrou no top 10.");
        else
            Debug.Log($"Ranked: {initials} {sc}/{lv}");
    }

    /// <summary>
    /// Chama manualmente para recarregar/atualizar o ScrollView.
    /// </summary>
    public void LoadRank()
    {
        leaderboardManager.LoadRank();
        leaderboardManager.UpdateRank();
    }
}
