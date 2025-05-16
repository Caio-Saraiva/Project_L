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
        // só se inscreve uma vez aqui, não mexe mais depois  
        sendZone.OnCardDropped += HandleSendZone;
    }

    void Update()
    {
        if (currentCard == null)
            SpawnNextCard();
    }

    void SpawnNextCard()
    {
        // instancia o card sob o canvas
        currentCard = Instantiate(cardPrefab, cardParent);

        // posiciona no receiveSlot
        var rt = currentCard.GetComponent<RectTransform>();
        rt.anchoredPosition = receiveSlot.anchoredPosition;
        rt.localScale = Vector3.one * zoomOutScale;

        // injeta referências de zonas e escalas
        currentCard.collectArea = collectArea;
        currentCard.stampTableArea = stampTableArea;
        currentCard.zoomOutScale = zoomOutScale;
        currentCard.defaultScale = defaultScale;

        // gera o circuito
        currentCard.setup.Initialize();

        // prepara o painel de stamps
        stampPanel.EnableAll();
        stampPanel.OnStamped += HandleStamped;
    }

    void HandleStamped(LabelMode mode, int value)
    {
        // **não** validar aqui — apenas aplicar o selo
        currentCard.ApplyStamp(value);
        // trava os botões pra não trocar de selo
        stampPanel.DisableAll();
    }

    void HandleSendZone(CircuitCardView card)
    {
        if (card != currentCard) return;

        // depois de largar no sendZone é que valida:
        if (!card.IsStamped)
        {
            Debug.LogWarning("⚠️ Atenção: nenhum selo aplicado!");
        }
        else if (card.StampedValue == card.setup.expectedOutput)
        {
            Debug.Log("🎉 Parabéns! Resposta correta!");
        }
        else
        {
            Debug.Log("❌ Resposta errada!");
        }

        // cleanup dos listeners
        stampPanel.OnStamped -= HandleStamped;

        // destrói e libera próximo card
        Destroy(currentCard.gameObject);
        currentCard = null;
    }
}
