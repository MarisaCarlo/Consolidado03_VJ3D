#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering;
using System.IO;

public static class AgentPFinishingTools
{
    const string S = "Assets/AgenteP/Scenes/AgenteP_Episodio1.unity";
    const string T = "Assets/AgenteP/Textures/";

    static void ConfigureTexture(string path, bool normal)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        bool changed=false;
        if (normal && importer.textureType != TextureImporterType.NormalMap) { importer.textureType=TextureImporterType.NormalMap; changed=true; }
        if (!importer.mipmapEnabled) { importer.mipmapEnabled=true; changed=true; }
        if (changed) importer.SaveAndReimport();
    }

    static void MaterialPBR(string name, string map)
    {
        string materialPath="Assets/AgenteP/Materials/"+name+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material==null) { Debug.LogWarning("No se encontro material " + materialPath); return; }
        string basePath=T+map+"_BaseColor.png", normalPath=T+map+"_Normal.png";
        ConfigureTexture(basePath,false); ConfigureTexture(normalPath,true);
        var albedo=AssetDatabase.LoadAssetAtPath<Texture2D>(basePath);
        var bump=AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
        if (albedo && material.HasProperty("_BaseMap")) {
            material.SetTexture("_BaseMap",albedo);
            material.SetTextureScale("_BaseMap",Vector2.one * 2f);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor",Color.white);
        }
        if (bump && material.HasProperty("_BumpMap")) {
            material.SetTexture("_BumpMap",bump);
            material.EnableKeyword("_NORMALMAP");
            if (material.HasProperty("_BumpScale")) material.SetFloat("_BumpScale",.32f);
        }
        EditorUtility.SetDirty(material);
    }

    static void MakeTimeline(GameObject root, AgentPGame game)
    {
        string assetFolder="Assets/AgenteP/Cinematics";
        if (!AssetDatabase.IsValidFolder(assetFolder)) AssetDatabase.CreateFolder("Assets/AgenteP","Cinematics");
        var path=assetFolder+"/Revelacion_Umbra.playable";
        var timeline=AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);
        if (!timeline) {
            timeline=ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline,path);
            var track=timeline.CreateTrack<AnimationTrack>(null,"Luz narrativa sobre expediente");
            var clipAsset=new AnimationClip { name="Luz_Movimiento_Final", frameRate=30 };
            AnimationUtility.SetEditorCurve(clipAsset,EditorCurveBinding.FloatCurve("",typeof(Transform),"m_LocalPosition.x"), AnimationCurve.EaseInOut(0,-1.4f,5f,1.5f));
            AnimationUtility.SetEditorCurve(clipAsset,EditorCurveBinding.FloatCurve("",typeof(Transform),"m_LocalPosition.y"), AnimationCurve.EaseInOut(0,4.1f,5f,3.2f));
            AnimationUtility.SetEditorCurve(clipAsset,EditorCurveBinding.FloatCurve("",typeof(Transform),"m_LocalPosition.z"), AnimationCurve.EaseInOut(0,84f,5f,89f));
            AssetDatabase.AddObjectToAsset(clipAsset,timeline);
            var sequenceClip=track.CreateClip<AnimationPlayableAsset>();
            var ap=sequenceClip.asset as AnimationPlayableAsset;
            ap.clip=clipAsset; sequenceClip.duration=5;
            EditorUtility.SetDirty(timeline);
        }
        var director=root.AddComponent<PlayableDirector>();
        director.playOnAwake=false;
        director.extrapolationMode=DirectorWrapMode.Hold;
        director.playableAsset=timeline;
        var subject=new GameObject("Timeline_Foco_Narrativo");
        subject.transform.SetParent(root.transform);
        subject.transform.localPosition=new Vector3(-1.4f,4.1f,84f);
        subject.AddComponent<Animator>();
        var light=subject.AddComponent<Light>();
        light.type=LightType.Point; light.color=new Color(.85f,.55f,.3f);light.range=9;light.intensity=2f;light.shadows=LightShadows.None;light.enabled=false;
        foreach(var track in timeline.GetOutputTracks()) if (track is AnimationTrack) director.SetGenericBinding(track,subject.GetComponent<Animator>());
        var seq=root.AddComponent<AgentPTimelineSequence>();seq.director=director;seq.revealLight=light;game.cinematic=seq;
    }

    [MenuItem("Tools/Agente P/2 - Completar PBR, Timeline y rendimiento")]
    public static void Apply()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(S)==null) { EditorUtility.DisplayDialog("Agente P","Primero genera el escenario desde Tools / Agente P / 1.","OK");return; }
        EditorSceneManager.OpenScene(S, OpenSceneMode.Single);
        MaterialPBR("Piso_humedo","Piso_humedo");
        MaterialPBR("Piso_humedo_alt","Piso_humedo_alt");
        MaterialPBR("Concreto","Concreto");
        MaterialPBR("Pared_sucia","Pared_sucia");
        MaterialPBR("Pared_oscura","Pared_oscura");
        MaterialPBR("Metal_oxidado","Metal_oxidado");
        MaterialPBR("Madera_vieja","Madera_vieja");
        var manager=Object.FindFirstObjectByType<AgentPGame>();
        if (!manager) { Debug.LogError("No se encontro GameManager en la escena.");return; }
        var old=GameObject.Find("AgenteP_PulidoFinal");
        if (old) Object.DestroyImmediate(old);
        var root = new GameObject("AgenteP_PulidoFinal");
        root.AddComponent<AgentPPerformance>();
        MakeTimeline(root,manager);
        var markers=GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach(var renderer in markers)
        {
            if (renderer.GetComponent<ParticleSystemRenderer>()) continue;
            if (!renderer.gameObject.isStatic) continue;
            var flags=GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
            GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,flags|StaticEditorFlags.OccluderStatic|StaticEditorFlags.OccludeeStatic);
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("AGENTE P: PBR + Timeline + marcadores de occlusion listos. F3 muestra FPS. Falta hornear occlusion y validar build WebGL.");
        EditorUtility.DisplayDialog("Agente P","PBR + Timeline aplicados a la escena existente. F3 muestra FPS. Revisa Console y prueba los finales.","OK");
    }

    [MenuItem("Tools/Agente P/3 - Verificar requisitos")]
    public static void Verify()
    {
        var game=Object.FindFirstObjectByType<AgentPGame>();
        var all=Object.FindObjectsByType<AgentPInteractable>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        int evidence=0,doors=0,finale=0;
        foreach(var c in all) {if(c.kind==AgentPInteractable.Kind.Evidence)evidence++;if(c.kind==AgentPInteractable.Kind.Door)doors++;if(c.kind==AgentPInteractable.Kind.Finale)finale++;}
        string result="Pistas en escena: "+evidence+"/10\\nPuertas: "+doors+"\\nFinal: "+finale+"\\nGameManager: "+(game?"OK":"FALTA")+"\\nTimeline: "+(game && game.cinematic?"OK":"FALTA")+"\\nShader Graph: PENDIENTE DE VERIFICACION\\nBuild WebGL: PENDIENTE DE PRUEBA";
        result=result.Replace("\\n","\n");
        Debug.Log(result); EditorUtility.DisplayDialog("Agente P: Verificacion",result,"Cerrar");
    }
}
#endif
