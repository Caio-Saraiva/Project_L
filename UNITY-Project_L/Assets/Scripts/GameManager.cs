using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Card Prefab & UI")]
    public CircuitCardView cardPrefab;
    public Canvas uiCanvas;
    public RectTransform cardParent;
    public RectTransform receiveSlot;

    [Header("Drag Zones")]
    public RectTransform collectArea;
    public RectTransform stampTableArea;
    public DropZoneController sendZone;

    [Header("Stamp Panel")]
    public StampPanelController stampPanel;

    [Header("Escalas")]
    [Range(0.1f, 2f)] public float zoomOutScale = 0.5f;
    [Range(0.1f, 2f)] public float defaultScale = 1f;

    private CircuitCardView currentCard;

    void Start()
    {
        sendZone.OnCardDropped += HandleSendZone;
    }

    void Update()
    {
        if (currentCard == null)
            SpawnNextCard();
    }

    void SpawnNextCard()
    {
        currentCard = Instantiate(cardPrefab, cardParent);
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

    void HandleStamped(LabelMode mode, int value)
    {
        // aplica selo usando o modo e valor
        currentCard.ApplyStamp(mode, value);
        // trava todos os botões (caso DisableAll seja privado, torne-o público)
        stampPanel.DisableAll();
    }

    void HandleSendZone(CircuitCardView card)
    {
        if (card != currentCard) return;

        if (!card.IsStamped)
            Debug.LogWarning("⚠️ Atenção: nenhum selo aplicado!");
        else if (card.StampedValue == card.setup.expectedOutput)
            Debug.Log("🎉 Parabéns! Resposta correta!");
        else
            Debug.Log("❌ Resposta errada!");

        stampPanel.OnStamped -= HandleStamped;
        Destroy(currentCard.gameObject);
        currentCard = null;
    }
}
