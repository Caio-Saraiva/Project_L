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

    // interna: idioma → (chave → valor)
    private Dictionary<string, Dictionary<string, string>> translations;

    void Awake()
    {
        LoadTranslations();
    }

    void LoadTranslations()
    {
        if (localizationJson == null)
        {
            Debug.LogError("LocalizationManager: JSON não atribuído!");
            return;
        }

        LocalizationFile file = JsonUtility.FromJson<LocalizationFile>(localizationJson.text);
        translations = new Dictionary<string, Dictionary<string, string>>();

        foreach (var lang in file.languages)
        {
            var dict = new Dictionary<string, string>();
            foreach (var e in lang.entries)
                dict[e.key] = e.value;

            translations[lang.code] = dict;
        }
    }

    void ApplyLanguage(string code)
    {
        if (translations == null || !translations.ContainsKey(code))
        {
            Debug.LogError($"LocalizationManager: idioma '{code}' não carregado.");
            return;
        }

        var dict = translations[code];
        foreach (var le in entries)
        {
            if (le.target != null && dict.ContainsKey(le.key))
                le.target.text = dict[le.key];
        }
    }

    /// <summary>Chama para trocar para Português e atualizar todos os textos.</summary>
    public void ChangeAndApplyPt()
    {
        ApplyLanguage("pt");
    }

    /// <summary>Chama para trocar para Inglês e atualizar todos os textos.</summary>
    public void ChangeAndApplyEn()
    {
        ApplyLanguage("en");
    }
}
