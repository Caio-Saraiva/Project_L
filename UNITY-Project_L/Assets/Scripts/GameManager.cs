using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Níveis e Cards")]
    public List<CircuitLevel> levels;

    [Header("Prefabs & Referências")]
    public CircuitCardView cardPrefab;
    public Transform cardSpawnPoint;
    public StampPanelController stampPanel;
    public DropZoneController dropZone;

    [Header("Stamping Zone & Escalas")]
    public RectTransform stampingZone;
    public Transform smallSlotParent;
    public Vector3 smallScale = Vector3.one * 0.5f;
    public Vector3 stampScale = Vector3.one;

    [Header("Tempo & Pontuação")]
    public float initialTimer = 30f;
    private float timer;
    private int score;

    private int levelIndex = 0;
    private int tutorialIndex = 0;
    private CircuitCardView currentCard;
    private LabelMode currentLabelMode;
    private CircuitLevel CurrentLevel => levels[levelIndex];

    void Start()
    {
        timer = initialTimer;
        dropZone.OnCardDropped += HandleCardDropped;
        SpawnNextCard();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) EndSession();
    }

    void SpawnNextCard()
    {
        // Escolhe tutorial ou procedural
        CircuitDefinition def;
        if (tutorialIndex < CurrentLevel.tutorialCards.Length)
        {
            def = CurrentLevel.tutorialCards[tutorialIndex++];
        }
        else if (CurrentLevel.useProcedural)
        {
            def = ProceduralGenerator.Generate(CurrentLevel);
        }
        else
        {
            EndSession();
            return;
        }

        // Instancia e injeta parâmetros de drag & drop
        currentCard = Instantiate(cardPrefab, cardSpawnPoint);
        var rt = currentCard.GetComponent<RectTransform>();
        rt.localScale = smallScale;

        currentCard.stampingZone = stampingZone;
        currentCard.smallSlotParent = smallSlotParent;
        currentCard.smallScale = smallScale;
        currentCard.stampScale = stampScale;

        // Setup do card
        currentCard.Setup(def);
        currentLabelMode = def.labelMode;

        // Registra eventos
        currentCard.OnSent += HandleSent;
        stampPanel.OnStamped += HandleStamped;

        stampPanel.EnableAllStamps();
    }

    void HandleStamped(LabelMode mode, int value)
    {
        if (mode != currentLabelMode)
        {
            // formato errado → envia como incorreto
            currentCard.Send();
            return;
        }

        currentCard.ApplyStamp(value);
        // Aguarda o drop na zona de coleta
    }

    void HandleCardDropped(CircuitCardView card)
    {
        if (card == currentCard)
            currentCard.Send();
    }

    void HandleSent(bool correct)
    {
        if (correct)
        {
            score += CurrentLevel.basePoints;
            timer += CurrentLevel.bonusTime;
        }
        else
        {
            score += CurrentLevel.penaltyPoints;
            timer -= CurrentLevel.penaltyTime;
        }

        // Cleanup
        currentCard.OnSent -= HandleSent;
        stampPanel.OnStamped -= HandleStamped;

        Destroy(currentCard.gameObject);
        SpawnNextCard();
    }

    void EndSession()
    {
        Debug.Log($"Sessão encerrada! Score final: {score}");
        enabled = false;
    }
}
