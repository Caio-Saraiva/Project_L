using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverFade : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuração do Fade")]
    [Tooltip("O CanvasGroup do objeto que será mostrado/ocultado")]
    public CanvasGroup targetGroup;
    [Tooltip("Duração do fade (segundos)")]
    public float fadeDuration = 0.3f;

    Coroutine fadeCoroutine;

    void Start()
    {
        if (targetGroup == null)
        {
            Debug.LogError($"[{nameof(UIHoverFade)}] É preciso atribuir um CanvasGroup em targetGroup.");
            enabled = false;
            return;
        }

        // Começa oculto
        targetGroup.alpha = 0f;
        targetGroup.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Para qualquer fade em andamento e inicia o fade-in
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        targetGroup.gameObject.SetActive(true);
        fadeCoroutine = StartCoroutine(Fade(targetGroup, targetGroup.alpha, 1f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Para qualquer fade em andamento e inicia o fade-out
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutAndDisable(targetGroup, targetGroup.alpha, 0f));
    }

    IEnumerator Fade(CanvasGroup cg, float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        cg.alpha = to;
    }

    IEnumerator FadeOutAndDisable(CanvasGroup cg, float from, float to)
    {
        yield return Fade(cg, from, to);
        // Após terminar o fade-out, desativa o objeto
        if (Mathf.Approximately(cg.alpha, 0f))
            cg.gameObject.SetActive(false);
    }
}
