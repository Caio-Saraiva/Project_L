// só compila no Editor
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public class WebGLPostProcessor
{
    // este método será chamado AUTOMATICAMENTE após a Build WebGL
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        // só nos interessa WebGL
        if (target != BuildTarget.WebGL) return;

        // ex: caminho para o index.html gerado
        var indexPath = Path.Combine(pathToBuiltProject, "index.html");
        if (!File.Exists(indexPath))
        {
            Debug.LogWarning("WebGLPostProcessor: index.html não encontrado em " + indexPath);
            return;
        }

        // lê o HTML
        var html = File.ReadAllText(indexPath);

        // pesquisa e substitui o placeholder __VERSION__ pelo Application.version
        var version = PlayerSettings.bundleVersion; // ou Application.version
        html = html.Replace("__VERSION__", version);

        // salva de volta
        File.WriteAllText(indexPath, html);

        Debug.Log($"WebGLPostProcessor: index.html atualizado com versão {version}");
    }
}
#endif
