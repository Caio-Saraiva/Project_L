using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    [Header("UI Slider de Volume")]
    [Tooltip("Arraste aqui o Slider que controla o volume.")]
    public Slider volumeSlider;

    [Header("Fontes de Áudio")]
    [Tooltip("Todos os AudioSources cujo volume será ajustado.")]
    public List<AudioSource> audioSources = new List<AudioSource>();

    [Header("PlayerPrefs")]
    [Tooltip("Chave usada para salvar/recuperar o volume.")]
    public string prefsKey = "GameVolume";

    void Start()
    {
        // Carrega volume salvo (ou 1.0 por padrão)
        float saved = PlayerPrefs.GetFloat(prefsKey, 1f);
        SetVolume(saved);

        if (volumeSlider != null)
        {
            volumeSlider.value = saved;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    /// <summary>
    /// Ajusta o volume de todas as fontes e salva no PlayerPrefs.
    /// </summary>
    public void SetVolume(float volume)
    {
        foreach (var src in audioSources)
            if (src != null)
                src.volume = volume;

        PlayerPrefs.SetFloat(prefsKey, volume);
        PlayerPrefs.Save();
    }
}
