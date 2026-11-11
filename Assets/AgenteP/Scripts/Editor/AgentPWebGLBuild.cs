#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.IO.Compression;

public static class AgentPWebGLBuild
{
    [MenuItem("Tools/Agente P/4 - Compilar y comprimir WebGL")]
    public static void Build()
    {
        string scene="Assets/AgenteP/Scenes/AgenteP_Episodio1.unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene)==null) {
            EditorUtility.DisplayDialog("Agente P","No existe la escena AgenteP_Episodio1.unity","OK");return;
        }
        var projectRoot=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
        var folder=Path.Combine(projectRoot,"Builds","AgenteP_WebGL");
        Directory.CreateDirectory(folder);
        EditorBuildSettings.scenes=new [] { new EditorBuildSettingsScene(scene,true) };
        BuildReport report=null;
        try {
            report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[] {scene}, locationPathName=folder,
                target=BuildTarget.WebGL, options=BuildOptions.None
            });
        } catch (System.Exception e) {
            Debug.LogError("Fallo WebGL (comprueba que WebGL Build Support este instalado): " + e);
            EditorUtility.DisplayDialog("WebGL","No se pudo compilar. Revisa Console y el modulo WebGL Build Support.","OK"); return;
        }
        if (report==null || report.summary.result!=BuildResult.Succeeded) {
            EditorUtility.DisplayDialog("WebGL","Compilacion no completada. Revisa Console.","OK");return;
        }
        string archive=Path.Combine(projectRoot,"Builds","AgenteP_WebGL_ENTREGA.zip");
        if (File.Exists(archive)) File.Delete(archive);
        using(var fs=File.Create(archive)) using(var zip=new ZipArchive(fs,ZipArchiveMode.Create)) {
            foreach(var file in Directory.GetFiles(folder,"*",SearchOption.AllDirectories)) {
                string relative=file.Substring(folder.Length).TrimStart(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar).Replace('\\','/');
                var entry=zip.CreateEntry(relative,System.IO.Compression.CompressionLevel.Optimal);
                using(var input=File.OpenRead(file)) using(var output=entry.Open()) input.CopyTo(output);
            }
        }
        Debug.Log("AGENTE P: build WebGL generada: "+archive +". TODAVIA DEBE PROBARSE EN EL NAVEGADOR.");
        EditorUtility.RevealInFinder(archive);
        EditorUtility.DisplayDialog("Agente P WebGL","ZIP generado en Builds. Prueba index.html en un servidor o publica en itch.io antes de entregar.","OK");
    }
}
#endif
