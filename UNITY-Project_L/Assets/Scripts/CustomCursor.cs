using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CustomCursor : MonoBehaviour
{
    [Header("Cursor Canvas")]
    [Tooltip("O Canvas dedicado ao cursor, deve permanecer sempre ativo.")]
    public Canvas cursorCanvas;

    [Header("Sprites de cursor")]
    public Sprite normalSprite;
    public Sprite pressedSprite;

    [Header("Cursor Size (px)")]
    public float cursorWidth = 32f;
    public float cursorHeight = 32f;

    [Header("Offset (hotspot)")]
    public Vector2 hotspotOffset;

    private Image _cursorImage;
    private RectTransform _rt;

    void Awake()
    {
        // Esconde o cursor padrão
        Cursor.visible = false;

        // Referências
        _cursorImage = GetComponent<Image>();
        _rt = GetComponent<RectTransform>();

        // Parent fixo no Canvas do cursor
        if (cursorCanvas != null)
            transform.SetParent(cursorCanvas.transform, false);
        else
            Debug.LogWarning("CustomCursor: atribua o CursorCanvas no Inspector!");

        // Sprite inicial e tamanho
        _cursorImage.sprite = normalSprite;
        ApplySize();
    }

    void Update()
    {
        // Ajusta tamanho (caso mude em runtime)
        ApplySize();

        // Move para acompanhar o mouse
        Vector2 screenPos = (Vector2)Input.mousePosition + hotspotOffset;
        _rt.position = screenPos;

        // Troca de sprite no clique
        if (Input.GetMouseButtonDown(0))
        {
            _cursorImage.sprite = pressedSprite;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _cursorImage.sprite = normalSprite;
        }
    }

    void OnDisable()
    {
        Cursor.visible = true;
    }

    private void ApplySize()
    {
        _rt.sizeDelta = new Vector2(cursorWidth, cursorHeight);
    }
}
