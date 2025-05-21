using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

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
}

public class GameManager : MonoBehaviour
{
    [Header("Circuit Prefabs por Level")]
    public List<CircuitPrefabEntry> circuitPrefabs;

    [Header("Card & UI")]
    public Canvas uiCanvas;
    public RectTransform cardParent;
    public RectTransform receiveSlot;

    [Header("Drag Zones")]
    public RectTransform collectArea;
    public RectTransform stampTableArea;
    public DropZoneController sendZone;

    [Header("Stamp Panel")]
    public StampPanelController stampPanel;

    [Header("Score Settings")]
    public TextMeshProUGUI scoreText;
    public int pointsPerCorrect = 10;
    public int pointsPerWrong = 5;

    [Header("Timer Settings")]
    public TextMeshProUGUI timerText;
    public float initialTime = 60f;
    public float timeBonus = 5f;

    [Header("Level Display")]
    public TextMeshProUGUI levelText;

    [Header("Card Scale Settings")]
    [Range(0.1f, 2f)] public float zoomOutScale = 0.5f;
    [Range(0.1f, 2f)] public float defaultScale = 1f;

    // runtime state
    private CircuitCardView currentCard;
    private int score;
    private int level;
    private float timer;
    private bool gameActive;

    void Start()
    {
        sendZone.OnCardDropped += HandleSendZone;
        ResetGame();
    }

    void Update()
    {
        if (!gameActive) return;

        timer -= Time.deltaTime;
        if (timer < 0f) timer = 0f;
        UpdateTimerUI();

        if (timer <= 0f) { EndGame(); return; }

        if (currentCard == null) SpawnNextCard();
    }

    public void ResetGame()
    {
        score = 0;
        level = 1;
        timer = initialTime;
        gameActive = true;

        UpdateScoreUI();
        UpdateLevelUI();
        UpdateTimerUI();

        if (currentCard != null)
        {
            Destroy(currentCard.gameObject);
            currentCard = null;
        }
    }

    private void SpawnNextCard()
    {
        // filtra pelos level mínimos
        var available = circuitPrefabs
            .Where(e => level >= e.minLevel && e.spawnChance > 0)
            .ToList();

        // fallback: se nenhum disponível, permite todos com spawnChance > 0
        if (available.Count == 0)
            available = circuitPrefabs.Where(e => e.spawnChance > 0).ToList();

        // soma total de chances
        int totalChance = available.Sum(e => e.spawnChance);
        if (totalChance <= 0)
            totalChance = available.Count; // evita zero → uniform

        // escolhe entrada por peso
        int roll = Random.Range(0, totalChance);
        CircuitPrefabEntry chosen = null;
        int cum = 0;
        foreach (var entry in available)
        {
            cum += entry.spawnChance;
            if (roll < cum)
            {
                chosen = entry;
                break;
            }
        }
        if (chosen == null) chosen = available[Random.Range(0, available.Count)];

        // instancia o prefab escolhido
        currentCard = Instantiate(chosen.prefab, cardParent);
        var rt = currentCard.GetComponent<RectTransform>();
        rt.anchoredPosition = receiveSlot.anchoredPosition;
        rt.localScale = Vector3.one * zoomOutScale;

        currentCard.collectArea = collectArea;
        currentCard.stampTableArea = stampTableArea;
        currentCard.zoomOutScale = zoomOutScale;
        currentCard.defaultScale = defaultScale;

        currentCard.setup.Initialize();

        stampPanel.EnableAll();
        stampPanel.OnStamped += HandleStamped;
    }

    private void HandleStamped(LabelMode mode, int value)
    {
        if (!gameActive || currentCard == null) return;
        currentCard.ApplyStamp(mode, value);
        stampPanel.DisableAll();
    }

    private void HandleSendZone(CircuitCardView card)
    {
        if (!gameActive || card != currentCard) return;

        bool gaveStamp = card.IsStamped;
        bool correctMode = gaveStamp && (card.StampedMode == card.setup.labelMode);
        bool correctValue = correctMode && (card.StampedValue == card.setup.expectedOutput);

        if (!gaveStamp || !correctMode)
        {
            Debug.Log("❌ Resposta errada (modo ou ausência de selo).");
            score -= pointsPerWrong;
        }
        else if (!correctValue)
        {
            Debug.Log($"❌ Valor incorreto! Esperado {card.setup.expectedOutput}.");
            score -= pointsPerWrong;
        }
        else
        {
            Debug.Log("🎉 Parabéns! Modo e valor corretos!");
            score += pointsPerCorrect;
            timer += timeBonus;
            level += 1;
        }

        score = Mathf.Max(0, score);
        UpdateScoreUI();
        UpdateLevelUI();

        stampPanel.OnStamped -= HandleStamped;
        Destroy(currentCard.gameObject);
        currentCard = null;
    }

    private void EndGame()
    {
        gameActive = false;
        Debug.Log($"⏰ Tempo esgotado! Pontuação final: {score}");
        if (currentCard != null)
            Destroy(currentCard.gameObject);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {score}";
    }

    private void UpdateLevelUI()
    {
        if (levelText != null)
            levelText.text = $"LEVEL {level}";
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        int centis = Mathf.FloorToInt(timer * 100) % 100;
        int seconds = Mathf.FloorToInt(timer) % 60;
        int minutes = Mathf.FloorToInt(timer / 60);
        timerText.text = $"{minutes:00}:{seconds:00}:{centis:00}";
    }
}
