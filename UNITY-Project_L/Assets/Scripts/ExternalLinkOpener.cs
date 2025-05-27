using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class ButtonLink
{
    [Tooltip("O bot�o que, quando clicado, abre a URL")]
    public Button button;

    [Tooltip("URL a ser aberta")]
    public string url;
}

public class ExternalLinkOpener : MonoBehaviour
{
    [Header("Bot�es e URLs")]
    public List<ButtonLink> links = new List<ButtonLink>();

    void Awake()
    {
        foreach (var bl in links)
        {
            if (bl.button == null || string.IsNullOrEmpty(bl.url))
                continue;

            // capture local para evitar closure problem
            string targetUrl = bl.url;
            bl.button.onClick.AddListener(() => OpenLink(targetUrl));
        }
    }

    /// <summary>
    /// Abre a URL no navegador padr�o.
    /// </summary>
    public void OpenLink(string url)
    {
        Application.OpenURL(url);
    }
}
