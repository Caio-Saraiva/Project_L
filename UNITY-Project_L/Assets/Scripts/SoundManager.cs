using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Canvas))]
public class SoundManager : MonoBehaviour
{
    [Header("UI Slider de Volume")]
    [Tooltip("Arraste o Slider UI que vai controlar o volume (0–1)")]
    public Slider volumeSlider;

    [Header("Fontes de Áudio (se vazio, será auto-detectado)")]
    public List<AudioSource> audioSources = new List<AudioSource>();

    [Header("PlayerPrefs")]
    [Tooltip("Chave usada para salvar/recuperar o volume")]
    public string prefsKey = "GameVolume";

    void Awake()
    {
        // Se não tiver nada preenchido, busca TODAS as AudioSources da cena
        if (audioSources.Count == 0)
        {
            audioSources.AddRange(FindObjectsOfType<AudioSource>());
            Debug.Log($"[SoundManager] Auto-detectei {audioSources.Count} AudioSources.");
        }
    }

    void Start()
    {
        // Carrega valor salvo (ou 1.0 se não existir)
        float saved = PlayerPrefs.GetFloat(prefsKey, 1f);

        if (volumeSlider != null)
        {
            // garante range 0–1
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = saved;

            // remove listener antigo e adiciona o nosso
            volumeSlider.onValueChanged.RemoveListener(OnSliderChanged);
            volumeSlider.onValueChanged.AddListener(OnSliderChanged);
        }
        else
        {
            Debug.LogWarning("[SoundManager] volumeSlider não atribuído!");
        }

        // já aplica ao início
        ApplyVolume(saved);
    }

    /// <summary>
    /// Chamado sempre que o usuário mexer no Slider.
    /// </summary>
    private void OnSliderChanged(float val)
    {
        ApplyVolume(val);
    }

    /// <summary>
    /// Ajusta o volume de todas as AudioSources e salva no PlayerPrefs.
    /// </summary>
    public void ApplyVolume(float volume)
    {
        foreach (var src in audioSources)
        {
            if (src != null)
                src.volume = volume;
        }

        PlayerPrefs.SetFloat(prefsKey, volume);
        PlayerPrefs.Save();

        Debug.Log($"[SoundManager] Volume setado para {volume:0.00}");
    }
}
