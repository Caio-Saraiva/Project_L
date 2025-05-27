using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class ButtonLink
{
    [Tooltip("O botão que, quando clicado, abre a URL")]
    public Button button;

    [Tooltip("URL a ser aberta")]
    public string url;
}

public class ExternalLinkOpener : MonoBehaviour
{
    [Header("Botões e URLs")]
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
    /// Abre a URL no navegador padrão.
    /// </summary>
    public void OpenLink(string url)
    {
        Application.OpenURL(url);
    }
}
