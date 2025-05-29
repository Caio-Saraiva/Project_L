using UnityEngine;
using System.Collections.Generic;

public class PageCarousel : MonoBehaviour
{
    [Header("Páginas do Carrossel")]
    [Tooltip("Arraste aqui todos os GameObjects que representam cada página.")]
    public List<GameObject> pages = new List<GameObject>();

    int _currentIndex = 0;

    void Start()
    {
        ShowCurrent();
    }

    /// <summary>
    /// Ativa apenas a página atual e desativa as demais.
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
    /// Avança para a próxima página (loopando).
    /// </summary>
    public void NextPage()
    {
        if (pages.Count == 0) return;
        _currentIndex = (_currentIndex + 1) % pages.Count;
        ShowCurrent();
    }

    /// <summary>
    /// Volta para a página anterior (loopando).
    /// </summary>
    public void PreviousPage()
    {
        if (pages.Count == 0) return;
        _currentIndex = (_currentIndex - 1 + pages.Count) % pages.Count;
        ShowCurrent();
    }
}
