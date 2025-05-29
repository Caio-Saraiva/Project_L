using UnityEngine;
using System.Collections.Generic;

public class PageCarousel : MonoBehaviour
{
    [Header("P�ginas do Carrossel")]
    [Tooltip("Arraste aqui todos os GameObjects que representam cada p�gina.")]
    public List<GameObject> pages = new List<GameObject>();

    int _currentIndex = 0;

    void Start()
    {
        ShowCurrent();
    }

    /// <summary>
    /// Ativa apenas a p�gina atual e desativa as demais.
    /// </summary>
    void ShowCurrent()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i] != null)
                pages[i].SetActive(i == _currentIndex);
        }
    }

    /// <summary>
    /// Avan�a para a pr�xima p�gina (loopando).
    /// </summary>
    public void NextPage()
    {
        if (pages.Count == 0) return;
        _currentIndex = (_currentIndex + 1) % pages.Count;
        ShowCurrent();
    }

    /// <summary>
    /// Volta para a p�gina anterior (loopando).
    /// </summary>
    public void PreviousPage()
    {
        if (pages.Count == 0) return;
        _currentIndex = (_currentIndex - 1 + pages.Count) % pages.Count;
        ShowCurrent();
    }
}
