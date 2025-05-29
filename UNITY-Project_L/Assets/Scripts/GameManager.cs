using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Circuit Prefabs por Level")]
    public List<CircuitPrefabEntry> circuitPrefabs;

    [Header("Card & UI")]
    public Canvas uiCanvas;      // Overlay → cam = null
    public RectTransform cardParent;
    public RectTransform receiveSlot;

    [Header("Spawn Rotation Settings")]
    [Tooltip("Rotação mínima em graus no eixo Z ao instanciar o card")]
    public float spawnRotationMin = -5f;
    [Tooltip("Rotação máxima em graus no eixo Z ao instanciar o card")]
    public float spawnRotationMax = 5f;

    [Header("Drag Zones")]
    public RectTransform collectArea;
    public RectTransform stampTableArea;
    public DropZoneController sendZone;

    [Header("Stamp Panel")]
    public StampPanelController stampPanel;

    [Header("Score Settings")]
    public List<TextMeshProUGUI> scoreTexts;
    [Tooltip("Pontos ganhos base por acerto")]
    public int pointsPerCorrect = 10;
    [Tooltip("Pontos perdidos por erro ou ausência de selo")]
    public int pointsPerWrong = 5;

    [Header("Timer Settings")]
    public TextMeshProUGUI timerText;
    [Tooltip("Tempo inicial (segundos)")]
    public float initialTime = 60f;
    [Tooltip("Bônus de tempo por acerto (segundos)")]
    public float timeBonus = 5f;

    [Header("Level Display")]
    public List<TextMeshProUGUI> levelTexts;

    [Header("Card Scale Settings")]
    [Range(0.1f, 2f)] public float zoomOutScale = 0.5f;
    [Range(0.1f, 2f)] public float defaultScale = 1f;

    [Header("Feedback Events")]
    public UnityEvent onCorrect;
    public UnityEvent onError;
    [Tooltip("Invocado quando o tempo acabar")]
    public UnityEvent onTimeUp;

    [Header("Floating Feedback Texts")]
    public TextMeshProUGUI scoreFeedbackText;
    public TextMeshProUGUI timeFeedbackText;

    [Header("Feedback Colors")]
    public Color scorePositiveColor = Color.green;
    public Color scoreNegativeColor = Color.red;
    public Color timePositiveColor = Color.cyan;

    [Header("Feedback Timings")]
    [Tooltip("Quanto tempo o texto fica totalmente visível antes de sumir")]
    public float feedbackDisplayTime = 0.5f;
    [Tooltip("Duração do fade-out")]
    public float feedbackFadeTime = 0.5f;

    // estado runtime
    private CircuitPrefabEntry currentEntry;
    private CircuitCardView currentCard;
    public int score;
    public int level;
    private float timer;
    private bool gameActive;

    private Coroutine scoreFeedbackRoutine;
    private Coroutine timeFeedbackRoutine;

    void Start()
    {
        sendZone.OnCardDropped += HandleSendZone;
        StartGame();
    }

    void Update()
    {
        if (!gameActive) return;

        // decrementa timer
        timer -= Time.deltaTime;
        if (timer < 0f) timer = 0f;
        UpdateTimerUI();

        if (timer <= 0f)
        {
            EndGame();
            return;
        }

        // gera próximo card se não houver nenhum
        if (currentCard == null)
            SpawnNextCard();
    }

    public void StartGame()
    {
        // limpa qualquer card anterior
        if (currentCard != null)
        {
            Destroy(currentCard.gameObject);
            currentCard = null;
        }

        // (re)assina evento
        sendZone.OnCardDropped -= HandleSendZone;
        sendZone.OnCardDropped += HandleSendZone;

        ResetScore();
        ResetLevel();
        ResetTimer();
        ClearFeedbackText(scoreFeedbackText);
        ClearFeedbackText(timeFeedbackText);
        gameActive = true;
    }

    private void ResetScore()
    {
        score = 0;
        UpdateScoreUI();
    }

    private void ResetLevel()
    {
        level = 1;
        UpdateLevelUI();
    }

    private void ResetTimer()
    {
        timer = initialTime;
        UpdateTimerUI();
    }

    private void SpawnNextCard()
    {
        // filtra por nível e spawnChance>0
        var avail = circuitPrefabs
            .Where(e => level >= e.minLevel && e.spawnChance > 0)
            .ToList();
        if (avail.Count == 0)
            avail = circuitPrefabs.Where(e => e.spawnChance > 0).ToList();

        // sorteio ponderado por spawnChance
        int total = avail.Sum(e => e.spawnChance);
        if (total == 0) total = avail.Count;
        int roll = Random.Range(0, total), cum = 0;
        currentEntry = avail[0];
        foreach (var e in avail)
        {
            cum += e.spawnChance;
            if (roll < cum)
            {
                currentEntry = e;
                break;
            }
        }

        // instancia como filho de cardParent
        currentCard = Instantiate(currentEntry.prefab, cardParent);
        var cardRT = currentCard.GetComponent<RectTransform>();

        // aplica rotação aleatória no Z
        float angleZ = Random.Range(spawnRotationMin, spawnRotationMax);
        cardRT.localRotation = Quaternion.Euler(0f, 0f, angleZ);

        // posiciona sobre receiveSlot → coords locais de cardParent
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            null, receiveSlot.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cardParent, screenPoint, null, out Vector2 localPoint);
        cardRT.anchoredPosition = localPoint;

        // escala
        cardRT.localScale = Vector3.one * zoomOutScale;

        // injeta refs e inicializa
        currentCard.collectArea = collectArea;
        currentCard.stampTableArea = stampTableArea;
        currentCard.zoomOutScale = zoomOutScale;
        currentCard.defaultScale = defaultScale;
        currentCard.setup.Initialize();

        // prepara stamps
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
            Debug.Log("Resposta errada (modo ou ausência de selo).");
            score -= pointsPerWrong;
            onError?.Invoke();
            ShowScoreFeedback(-pointsPerWrong);
        }
        else if (!correctValue)
        {
            Debug.Log($"Valor incorreto! Esperado {card.setup.expectedOutput}.");
            score -= pointsPerWrong;
            onError?.Invoke();
            ShowScoreFeedback(-pointsPerWrong);
        }
        else
        {
            int gained = Mathf.RoundToInt(pointsPerCorrect * currentEntry.scoreMultiplier);
            Debug.Log($"Correto! +{gained} pontos.");
            score += gained;
            timer += timeBonus;
            level += 1;
            onCorrect?.Invoke();
            ShowScoreFeedback(+gained);
            ShowTimeFeedback(+timeBonus);
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
        Debug.Log($"Tempo esgotado! Pontuação final: {score}");
        onTimeUp?.Invoke();
        if (currentCard != null)
            Destroy(currentCard.gameObject);
    }

    private void UpdateScoreUI()
    {
        foreach (var txt in scoreTexts)
            if (txt != null)
                txt.text = $"SCORE: {score}";
    }

    private void UpdateLevelUI()
    {
        foreach (var txt in levelTexts)
            if (txt != null)
                txt.text = $"LEVEL {level}";
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        int centis = Mathf.FloorToInt(timer * 100) % 100;
        int seconds = Mathf.FloorToInt(timer) % 60;
        int minutes = Mathf.FloorToInt(timer / 60);
        timerText.text = $"{minutes:00}:{seconds:00}:{centis:00}";
    }

    private void ShowScoreFeedback(int delta)
    {
        if (scoreFeedbackRoutine != null)
            StopCoroutine(scoreFeedbackRoutine);

        scoreFeedbackText.text = $"{(delta > 0 ? "+" : "")}{delta} pts";
        scoreFeedbackText.color = delta >= 0
            ? scorePositiveColor
            : scoreNegativeColor;
        SetAlpha(scoreFeedbackText, 1f);

        scoreFeedbackRoutine = StartCoroutine(FeedbackCoroutine(scoreFeedbackText));
    }

    private void ShowTimeFeedback(float delta)
    {
        if (timeFeedbackRoutine != null)
            StopCoroutine(timeFeedbackRoutine);

        timeFeedbackText.text = $"{(delta > 0 ? "+" : "")}{Mathf.RoundToInt(delta)} s";
        timeFeedbackText.color = timePositiveColor;
        SetAlpha(timeFeedbackText, 1f);

        timeFeedbackRoutine = StartCoroutine(FeedbackCoroutine(timeFeedbackText));
    }

    private IEnumerator FeedbackCoroutine(TextMeshProUGUI text)
    {
        yield return new WaitForSeconds(feedbackDisplayTime);

        float elapsed = 0f;
        while (elapsed < feedbackFadeTime)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, elapsed / feedbackFadeTime);
            SetAlpha(text, a);
            yield return null;
        }
        ClearFeedbackText(text);
    }

    private void SetAlpha(TextMeshProUGUI text, float alpha)
    {
        var c = text.color;
        c.a = alpha;
        text.color = c;
    }

    private void ClearFeedbackText(TextMeshProUGUI text)
    {
        text.text = "";
    }
}
