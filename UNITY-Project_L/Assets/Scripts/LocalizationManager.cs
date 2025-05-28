using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[Serializable]
public class LocalizationEntry
{
    public string key;
    public string value;
}

[Serializable]
public class LocalizationLanguage
{
    public string code;
    public List<LocalizationEntry> entries;
}

[Serializable]
public class LocalizationFile
{
    public List<LocalizationLanguage> languages;
}

[Serializable]
public class LocalizeEntry
{
    [Tooltip("O TMP que deve ser atualizado")]
    public TextMeshProUGUI target;

    [Tooltip("Chave usada no JSON para este texto")]
    public string key;
}

public class LocalizationManager : MonoBehaviour
{
    [Header("JSON de Traduções")]
    [Tooltip("Arraste aqui o TextAsset com o JSON de idiomas")]
    public TextAsset localizationJson;

    [Header("Textos a Localizar")]
    [Tooltip("Associe cada TMP à chave correspondente no JSON")]
    public List<LocalizeEntry> entries = new List<LocalizeEntry>();

    [Header("PlayerPrefs")]
    [Tooltip("Chave usada para salvar/recuperar o idioma selecionado")]
    public string prefsKey = "language";

    [Tooltip("Idioma padrão se não houver nada salvo")]
    public string defaultLanguage = "pt";

    // interna: idioma → (chave → valor)
    private Dictionary<string, Dictionary<string, string>> translations;

    void Awake()
    {
        LoadTranslations();

        // lê idioma salvo ou usa padrão
        string code = PlayerPrefs.GetString(prefsKey, defaultLanguage);
        ApplyLanguage(code);
    }

    void LoadTranslations()
    {
        if (localizationJson == null)
        {
            Debug.LogError("LocalizationManager: JSON não atribuído!");
            return;
        }

        var file = JsonUtility.FromJson<LocalizationFile>(localizationJson.text);
        translations = new Dictionary<string, Dictionary<string, string>>(file.languages.Count);

        foreach (var lang in file.languages)
        {
            var dict = new Dictionary<string, string>(lang.entries.Count);
            foreach (var e in lang.entries)
                dict[e.key] = e.value;
            translations[lang.code] = dict;
        }
    }

    void ApplyLanguage(string code)
    {
        if (translations == null || !translations.ContainsKey(code))
        {
            Debug.LogError($"LocalizationManager: idioma '{code}' não encontrado.");
            return;
        }

        var dict = translations[code];
        foreach (var le in entries)
        {
            if (le.target != null && dict.TryGetValue(le.key, out var txt))
                le.target.text = txt;
        }
    }

    /// <summary>
    /// Muda para Português e salva a escolha.
    /// </summary>
    public void ChangeAndApplyPt()
    {
        PlayerPrefs.SetString(prefsKey, "pt");
        PlayerPrefs.Save();
        ApplyLanguage("pt");
    }

    /// <summary>
    /// Muda para Inglês e salva a escolha.
    /// </summary>
    public void ChangeAndApplyEn()
    {
        PlayerPrefs.SetString(prefsKey, "en");
        PlayerPrefs.Save();
        ApplyLanguage("en");
    }
}
