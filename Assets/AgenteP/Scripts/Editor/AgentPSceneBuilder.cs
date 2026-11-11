
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class AgentPSceneBuilder
{
    const string Root = "Assets/AgenteP";
    static Transform parent;
    static Material dirtyFloor, floorAlt, floorEdge, wall, wallDark, oldWood, rustMetal, paper, blood, tape, neon, gold, puddle, skin, coat, shirt, blackCloth, hair, glass, concrete;

    static Material Mat(string name, Color color, float metallic = 0, float smoothness = .25f, bool emission = false, float emissionMul = 2.2f)
    {
        string path = Root + "/Materials/" + name + ".mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing) return existing;
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        var m = new Material(s) { name = name, color = color };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
        if (emission) {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", color * emissionMul);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        AssetDatabase.CreateAsset(m,path);
        return m;
    }

    static GameObject Box(string name, Vector3 pos, Vector3 scale, Material m, Transform p = null, bool isStatic=true)
    {
        var o = GameObject.CreatePrimitive(PrimitiveType.Cube); o.name=name;
        o.transform.position=pos; o.transform.localScale=scale; o.transform.SetParent(p ? p : parent, true);
        o.GetComponent<Renderer>().sharedMaterial=m;
        o.isStatic=isStatic;
        return o;
    }
    static GameObject RotBox(string name, Vector3 pos, Vector3 scale, Vector3 rot, Material m, Transform p = null, bool isStatic=true)
    {
        var o=Box(name,pos,scale,m,p,isStatic); o.transform.rotation=Quaternion.Euler(rot); return o;
    }
    static GameObject Prim(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material m, Transform p = null, bool isStatic=false)
    {
        var o = GameObject.CreatePrimitive(type); o.name=name;
        o.transform.position=pos; o.transform.localScale=scale; o.transform.SetParent(p ? p : parent, true);
        o.GetComponent<Renderer>().sharedMaterial=m;
        o.isStatic=isStatic;
        return o;
    }
    static Light AddLight(string name, LightType type, Vector3 pos, Color color, float intensity, float range, bool shadows=false, Transform p = null)
    {
        var go = new GameObject(name); go.transform.SetParent(p ? p : parent); go.transform.position=pos;
        var l = go.AddComponent<Light>(); l.type=type; l.color=color; l.intensity=intensity; l.range=range;
        l.shadows=shadows?LightShadows.Soft:LightShadows.None;
        if (type==LightType.Spot) { l.spotAngle=60; }
        return l;
    }
    static void Dust(string name, Vector3 pos, Vector3 boxSize, Transform p = null)
    {
        var go = new GameObject(name); go.transform.SetParent(p ? p : parent); go.transform.position = pos;
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main; main.startLifetime = 7f; main.startSpeed = 0.09f; main.startSize = 0.08f; main.maxParticles = 180; main.loop = true; main.simulationSpace = ParticleSystemSimulationSpace.World; main.startColor = new Color(0.84f,0.84f,0.84f,0.33f);
        var emission = ps.emission; emission.rateOverTime = 18f;
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = boxSize;
        var noise = ps.noise; noise.enabled = true; noise.strength = .15f; noise.frequency = .45f;
        var col = ps.colorOverLifetime; col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(0.9f,0.9f,0.9f),1)}, new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(0.35f,0.2f),new GradientAlphaKey(0.25f,0.8f),new GradientAlphaKey(0,1)});
        col.color = new ParticleSystem.MinMaxGradient(g);
        var rend = ps.GetComponent<ParticleSystemRenderer>(); rend.material = paper; rend.renderMode = ParticleSystemRenderMode.Billboard;
    }
    static void Caption(string text, Vector3 pos, Color c)
    {
        var go=new GameObject("Rotulo_"+text); go.transform.SetParent(parent); go.transform.position=pos;
        var mesh=go.AddComponent<TextMesh>(); mesh.text=text; mesh.fontSize=70; mesh.characterSize=.09f; mesh.anchor=TextAnchor.MiddleCenter; mesh.color=c;
        go.transform.rotation=Quaternion.Euler(0,180,0);
    }
    static void BloodStain(string name, Vector3 pos, Vector3 scale, float yrot=0)
    {
        var o=Prim(PrimitiveType.Cylinder,name,pos,scale,blood,null,true);
        o.transform.rotation = Quaternion.Euler(90, yrot, 0);
        Object.DestroyImmediate(o.GetComponent<Collider>());
    }
    static void Puddle(string name, Vector3 pos, Vector3 scale, float yrot=0)
    {
        var o=Prim(PrimitiveType.Cylinder,name,pos,scale,puddle,null,true);
        o.transform.rotation = Quaternion.Euler(90, yrot, 0);
        Object.DestroyImmediate(o.GetComponent<Collider>());
    }

static void FloorPanels(float centerZ, float length, float width, int rows, int cols)
{
    float startZ = centerZ - length * 0.5f;
    float startX = -width * 0.5f;
    float stepZ = length / rows;
    float stepX = width / cols;
    for (int r = 0; r < rows; r++)
    {
        for (int c = 0; c < cols; c++)
        {
            float px = startX + stepX * (c + 0.5f);
            float pz = startZ + stepZ * (r + 0.5f);
            var mat = ((r + c) % 2 == 0) ? dirtyFloor : floorAlt;
            Box("Suelo_panel_"+r+"_"+c, new Vector3(px, -0.005f, pz), new Vector3(stepX*0.97f, 0.012f, stepZ*0.97f), mat);
        }
    }
    for (int i = 0; i < rows-1; i++)
    {
        float z = startZ + stepZ * (i+1);
        Box("Linea_suelo_z_"+i, new Vector3(0, -0.001f, z), new Vector3(width*0.94f, 0.008f, 0.04f), floorEdge);
    }
    for (int i = 0; i < cols-1; i++)
    {
        float x = startX + stepX * (i+1);
        Box("Linea_suelo_x_"+i, new Vector3(x, -0.001f, centerZ), new Vector3(0.04f, 0.008f, length*0.94f), floorEdge);
    }
}
    static void Room(string name,float centerZ,float length,float width,Material ground,Material sideWalls, float wallHeight=4, float y=-.28f)
    {
        parent=new GameObject(name).transform;
        Box("Suelo",new Vector3(0,y,centerZ),new Vector3(width,.5f,length),ground, null, true);
        Box("Pared_izquierda",new Vector3(-width/2,wallHeight/2,centerZ),new Vector3(.55f,wallHeight,length),sideWalls);
        Box("Pared_derecha",new Vector3(width/2,wallHeight/2,centerZ),new Vector3(.55f,wallHeight,length),sideWalls);
    }
    static void WallEnd(float z,float width,float opening,Material m,float h=4)
    {
        float side=(width-opening)/2f;
        if (side > 0.05f) {
            Box("Muro_extremo_izq",new Vector3(-(opening+side)/2f,h/2,z),new Vector3(side,h,.45f),m);
            Box("Muro_extremo_der",new Vector3((opening+side)/2f,h/2,z),new Vector3(side,h,.45f),m);
        } else {
            Box("Muro_extremo",new Vector3(0,h/2,z),new Vector3(width,h,.45f),m);
        }
    }
    static void BrokenCrates(Vector3 start, int count, Transform p=null)
    {
        for(int i=0;i<count;i++) {
            float x = start.x + (i%3)*1.8f;
            float z = start.z + (i/3)*1.9f;
            RotBox("Caja_rota_"+i,new Vector3(x,0.42f,z),new Vector3(1.2f,0.85f,1.1f),new Vector3(0,Random.Range(-18,18),Random.Range(-2,2)),oldWood,p,true);
            RotBox("Tapa_rota_"+i,new Vector3(x+Random.Range(-0.3f,0.3f),0.95f,z+Random.Range(-0.25f,0.25f)),new Vector3(1.18f,0.08f,1.08f),new Vector3(Random.Range(-55,-5),Random.Range(0,360),Random.Range(-10,10)),oldWood,p,true);
        }
    }
    static void FilingCabinet(Vector3 pos)
    {
        var root = new GameObject("Archivador_oxidado"); root.transform.SetParent(parent); root.transform.position = pos;
        Box("Cuerpo", pos + new Vector3(0,1.1f,0), new Vector3(1.2f,2.2f,0.95f), rustMetal, root.transform);
        for(int i=0;i<3;i++) {
            Box("Gaveta_"+i, pos + new Vector3(0,0.45f+i*0.62f,0.48f), new Vector3(1.05f,0.46f,0.08f), wallDark, root.transform);
            Box("Manija_"+i, pos + new Vector3(0,0.45f+i*0.62f,0.56f), new Vector3(0.2f,0.06f,0.06f), trimMat, root.transform);
        }
    }
    static Material trimMat; // alias for rust metal details


static GameObject Human(Transform root, string name, bool detective, bool assassin=false)
{
    var rig = new GameObject(name); rig.transform.SetParent(root); rig.transform.localPosition=Vector3.zero;
    var rr = rig.transform;

    Transform MakePivot(string pivotName, Vector3 worldPos)
    {
        var pivot = new GameObject(pivotName).transform;
        pivot.SetParent(rr); pivot.position = worldPos;
        return pivot;
    }

    // Cuerpo mas detallado pero aun low-poly estilizado.
    var waist = Prim(PrimitiveType.Cube,"Waist", root.position + new Vector3(0,.73f,0), new Vector3(.52f,.24f,.26f), detective?shirt:blackCloth, rr);
    var chest = Prim(PrimitiveType.Cube,"Chest", root.position + new Vector3(0,1.18f,0), new Vector3(.68f,.70f,.30f), detective?coat:blackCloth, rr);
    var neck = Prim(PrimitiveType.Cylinder,"Neck", root.position + new Vector3(0,1.56f,0), new Vector3(.10f,.09f,.10f), skin, rr); neck.transform.rotation = Quaternion.Euler(90,0,0);
    var head = Prim(PrimitiveType.Sphere,"Head", root.position + new Vector3(0,1.83f,0), new Vector3(.34f,.40f,.33f), assassin?blackCloth:skin, rr);
    var hairCap = Prim(PrimitiveType.Cylinder,"Hair", root.position + new Vector3(0,1.99f,0), new Vector3(.24f,.08f,.24f), assassin?blackCloth:hair, rr);
    var eyeL = Prim(PrimitiveType.Sphere,"Eye_L", root.position + new Vector3(-.08f,1.84f,.14f), new Vector3(.03f,.03f,.03f), assassin?blood:blackCloth, rr);
    var eyeR = Prim(PrimitiveType.Sphere,"Eye_R", root.position + new Vector3(.08f,1.84f,.14f), new Vector3(.03f,.03f,.03f), assassin?blood:blackCloth, rr);
    var nose = Prim(PrimitiveType.Cube,"Nose", root.position + new Vector3(0,1.78f,.16f), new Vector3(.04f,.05f,.03f), assassin?blackCloth:skin, rr);
    var shoulderCapL = Prim(PrimitiveType.Sphere,"ShoulderCap_L", root.position + new Vector3(-.42f,1.41f,0), new Vector3(.16f,.16f,.16f), detective?coat:blackCloth, rr);
    var shoulderCapR = Prim(PrimitiveType.Sphere,"ShoulderCap_R", root.position + new Vector3(.42f,1.41f,0), new Vector3(.16f,.16f,.16f), detective?coat:blackCloth, rr);

    var leftShoulder = MakePivot("Shoulder_L", root.position + new Vector3(-.43f,1.42f,0));
    var rightShoulder = MakePivot("Shoulder_R", root.position + new Vector3(.43f,1.42f,0));
    var upperArmL = Prim(PrimitiveType.Cube,"UpperArm_L", leftShoulder.position + new Vector3(0,-.24f,0), new Vector3(.17f,.42f,.17f), detective?coat:blackCloth, leftShoulder);
    var foreArmL = Prim(PrimitiveType.Cube,"ForeArm_L", leftShoulder.position + new Vector3(0,-.60f,0), new Vector3(.16f,.34f,.16f), detective?coat:blackCloth, leftShoulder);
    var handL = Prim(PrimitiveType.Sphere,"Hand_L", leftShoulder.position + new Vector3(0,-.84f,.02f), new Vector3(.11f,.11f,.11f), assassin?blackCloth:skin, leftShoulder);
    var upperArmR = Prim(PrimitiveType.Cube,"UpperArm_R", rightShoulder.position + new Vector3(0,-.24f,0), new Vector3(.17f,.42f,.17f), detective?coat:blackCloth, rightShoulder);
    var foreArmR = Prim(PrimitiveType.Cube,"ForeArm_R", rightShoulder.position + new Vector3(0,-.60f,0), new Vector3(.16f,.34f,.16f), detective?coat:blackCloth, rightShoulder);
    var handR = Prim(PrimitiveType.Sphere,"Hand_R", rightShoulder.position + new Vector3(0,-.84f,.02f), new Vector3(.11f,.11f,.11f), assassin?blackCloth:skin, rightShoulder);

    var leftHip = MakePivot("Hip_L", root.position + new Vector3(-.16f,.58f,0));
    var rightHip = MakePivot("Hip_R", root.position + new Vector3(.16f,.58f,0));
    var thighL = Prim(PrimitiveType.Cube,"Thigh_L", leftHip.position + new Vector3(0,-.26f,0), new Vector3(.19f,.44f,.19f), detective?shirt:blackCloth, leftHip);
    var shinL = Prim(PrimitiveType.Cube,"Shin_L", leftHip.position + new Vector3(0,-.68f,0), new Vector3(.18f,.38f,.18f), detective?shirt:blackCloth, leftHip);
    var footL = Prim(PrimitiveType.Cube,"Foot_L", leftHip.position + new Vector3(0,-.92f,.07f), new Vector3(.22f,.10f,.35f), blackCloth, leftHip);
    var thighR = Prim(PrimitiveType.Cube,"Thigh_R", rightHip.position + new Vector3(0,-.26f,0), new Vector3(.19f,.44f,.19f), detective?shirt:blackCloth, rightHip);
    var shinR = Prim(PrimitiveType.Cube,"Shin_R", rightHip.position + new Vector3(0,-.68f,0), new Vector3(.18f,.38f,.18f), detective?shirt:blackCloth, rightHip);
    var footR = Prim(PrimitiveType.Cube,"Foot_R", rightHip.position + new Vector3(0,-.92f,.07f), new Vector3(.22f,.10f,.35f), blackCloth, rightHip);

    if (detective)
    {
        Prim(PrimitiveType.Cylinder,"Hat", root.position + new Vector3(0,2.02f,0), new Vector3(.28f,.09f,.28f), blackCloth, rr);
        Prim(PrimitiveType.Cube,"HatBrim", root.position + new Vector3(0,1.90f,0), new Vector3(.58f,.03f,.58f), blackCloth, rr);
        RotBox("Tie", root.position + new Vector3(0,.98f,.17f), new Vector3(.08f,.35f,.02f), Vector3.zero, blood, rr, false);
        RotBox("CoatTail_L", root.position + new Vector3(-.15f,.55f,-.13f), new Vector3(.12f,.42f,.05f), new Vector3(8f,0,5f), coat, rr, false);
        RotBox("CoatTail_R", root.position + new Vector3(.15f,.55f,-.13f), new Vector3(.12f,.42f,.05f), new Vector3(8f,0,-5f), coat, rr, false);
    }
    if (assassin)
    {
        Prim(PrimitiveType.Cube,"Apron", root.position + new Vector3(0,1.02f,.18f), new Vector3(.54f,.95f,.04f), blackCloth, rr);
        RotBox("Knife", root.position + new Vector3(.54f,.92f,.08f), new Vector3(.05f,.40f,.02f), new Vector3(25,0,-25), trimMat, rr, false);
        RotBox("Mask", root.position + new Vector3(0,1.82f,.15f), new Vector3(.19f,.22f,.03f), Vector3.zero, rustMetal, rr, false);
    }

    Transform torchRoot = null;
    if (detective)
    {
        torchRoot = new GameObject("Linterna_en_mano_derecha").transform;
        torchRoot.SetParent(rightShoulder);
        torchRoot.position = rightShoulder.position + new Vector3(0,-.82f,.17f);
        var torchBody = Prim(PrimitiveType.Cylinder,"Linterna_carcasa",torchRoot.position,new Vector3(.07f,.18f,.07f),blackCloth,torchRoot);
        torchBody.transform.rotation = Quaternion.Euler(90,0,0);
        var rim = Prim(PrimitiveType.Cylinder,"Linterna_frontal",torchRoot.position + new Vector3(0,0,.19f),new Vector3(.09f,.03f,.09f),rustMetal,torchRoot);
        rim.transform.rotation = Quaternion.Euler(90,0,0);
        var lens = Prim(PrimitiveType.Cylinder,"Cristal_luz",torchRoot.position + new Vector3(0,0,.22f),new Vector3(.07f,.012f,.07f),neon,torchRoot);
        lens.transform.rotation = Quaternion.Euler(90,0,0);
        var lightObj = new GameObject("Foco_linterna");
        lightObj.transform.SetParent(torchRoot); lightObj.transform.position = torchRoot.position + new Vector3(0,0,.23f);
        var torchLight = lightObj.AddComponent<Light>();
        torchLight.type=LightType.Spot; torchLight.range=22f; torchLight.spotAngle=49f;
        torchLight.intensity=8.5f; torchLight.color=new Color(.85f,.92f,1f);
        torchLight.shadows=LightShadows.None; torchLight.enabled=false;
        var anim = rig.AddComponent<AgentPProceduralAnimator>();
        anim.playerRoot=root; anim.leftArm=leftShoulder; anim.rightArm=rightShoulder;
        anim.leftLeg=leftHip; anim.rightLeg=rightHip; anim.flashlightModel=torchRoot.gameObject;
        anim.flashlightLight=torchLight; anim.torchRoot = torchRoot;
    }
    foreach (var c in rig.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
    return rig;
}

static AgentPInteractable CreateEvidence(string id, string name, Vector3 pos, string story)
    {
        var root = new GameObject(name); root.transform.SetParent(parent); root.transform.position = pos;
        Collider col = null;
        switch(id)
        {
            case "EXPEDIENTE":
                var exp=Box("Folder", pos+new Vector3(0,0.03f,0), new Vector3(.7f,.04f,.52f), paper, root.transform, false);
                RotBox("FolderTop", pos+new Vector3(.06f,0.08f,0), new Vector3(.52f,.02f,.44f), new Vector3(0,0,-6), blood, root.transform, false);
                col = exp.GetComponent<Collider>();
                break;
            case "FOTO":
                var foto=RotBox("Photo", pos+new Vector3(0,0.03f,0), new Vector3(.52f,.03f,.37f), new Vector3(0,13,0), paper, root.transform, false);
                RotBox("PhotoTear", pos+new Vector3(.06f,0.031f,.02f), new Vector3(.2f,.02f,.12f), new Vector3(0,-17,0), blood, root.transform, false);
                col=foto.GetComponent<Collider>();
                break;
            case "LLAVE":
                var ring=Prim(PrimitiveType.Cylinder,"Ring", pos+new Vector3(0,0.07f,0), new Vector3(.12f,.01f,.12f), gold, root.transform, false); ring.transform.rotation=Quaternion.Euler(90,0,0);
                var stem=RotBox("Stem", pos+new Vector3(.11f,0.03f,0), new Vector3(.28f,.03f,.05f), new Vector3(0,0,0), gold, root.transform, false);
                col=stem.GetComponent<Collider>();
                break;
            case "SIMBOLO":
                var badge=Prim(PrimitiveType.Cylinder,"Badge", pos+new Vector3(0,0.07f,0), new Vector3(.22f,.03f,.22f), rustMetal, root.transform, false); badge.transform.rotation=Quaternion.Euler(90,0,0);
                RotBox("Mark1", pos+new Vector3(0,0.08f,0), new Vector3(.22f,.01f,.04f), new Vector3(0,45,0), blood, root.transform, false);
                RotBox("Mark2", pos+new Vector3(0,0.081f,0), new Vector3(.22f,.01f,.04f), new Vector3(0,-45,0), blood, root.transform, false);
                col=badge.GetComponent<Collider>();
                break;
            case "GUANTE":
                var palm=RotBox("Palm", pos+new Vector3(0,0.05f,0), new Vector3(.24f,.06f,.18f), new Vector3(0,22,0), blackCloth, root.transform, false);
                for(int i=0;i<4;i++) RotBox("Finger"+i, pos+new Vector3(-.08f+i*0.05f,0.05f,.12f), new Vector3(.035f,.04f,.10f), new Vector3(0,22,18), blackCloth, root.transform, false);
                BloodStain("Blood_glove", pos+new Vector3(.06f,0.03f,-.02f), new Vector3(.18f,.18f,.025f), 15);
                col=palm.GetComponent<Collider>();
                break;
            case "HUELLA":
                var sole=Prim(PrimitiveType.Cylinder,"Sole", pos+new Vector3(0,0.02f,0), new Vector3(.18f,.01f,.26f), blood, root.transform, false); sole.transform.rotation=Quaternion.Euler(90,0,0);
                var toe=Prim(PrimitiveType.Cylinder,"Toe", pos+new Vector3(0,0.021f,.18f), new Vector3(.13f,.01f,.13f), blood, root.transform, false); toe.transform.rotation=Quaternion.Euler(90,0,0);
                col=sole.GetComponent<Collider>();
                break;
            case "TELEFONO":
                var phone=RotBox("Phone", pos+new Vector3(0,0.05f,0), new Vector3(.18f,.08f,.34f), new Vector3(0,16,0), blackCloth, root.transform, false);
                RotBox("Screen", pos+new Vector3(0,0.093f,0), new Vector3(.13f,.01f,.23f), new Vector3(0,16,0), glass, root.transform, false);
                col=phone.GetComponent<Collider>();
                break;
            case "REGISTRO":
                var book=RotBox("Book", pos+new Vector3(0,0.08f,0), new Vector3(.52f,.12f,.38f), new Vector3(0,-12,0), oldWood, root.transform, false);
                RotBox("Pages", pos+new Vector3(.02f,0.12f,0), new Vector3(.42f,.03f,.3f), new Vector3(0,-12,0), paper, root.transform, false);
                col=book.GetComponent<Collider>();
                break;
            case "IDENTIDAD":
                var idc=RotBox("ID", pos+new Vector3(0,0.03f,0), new Vector3(.35f,.03f,.24f), new Vector3(0,7,0), paper, root.transform, false);
                RotBox("FaceSlot", pos+new Vector3(-.07f,0.04f,.03f), new Vector3(.10f,.01f,.10f), new Vector3(0,7,0), skin, root.transform, false);
                RotBox("DataSlot", pos+new Vector3(.05f,0.04f,-.02f), new Vector3(.12f,.01f,.05f), new Vector3(0,7,0), wallDark, root.transform, false);
                col=idc.GetComponent<Collider>();
                break;
            case "CONFESION":
                var recorder=RotBox("Recorder", pos+new Vector3(0,0.05f,0), new Vector3(.28f,.08f,.16f), new Vector3(0,-24,0), rustMetal, root.transform, false);
                Prim(PrimitiveType.Cylinder,"Button", pos+new Vector3(.05f,0.1f,.02f), new Vector3(.025f,.01f,.025f), blood, root.transform, false).transform.rotation=Quaternion.Euler(90,0,0);
                col=recorder.GetComponent<Collider>();
                break;
            default:
                var def = Prim(PrimitiveType.Cylinder,"Evidence", pos+new Vector3(0,0.05f,0), new Vector3(.2f,.06f,.2f), paper, root.transform, false);
                col=def.GetComponent<Collider>();
                break;
        }
        if (col==null) col = root.AddComponent<BoxCollider>();
        var a = root.AddComponent<AgentPInteractable>(); a.kind=AgentPInteractable.Kind.Evidence;
        a.evidenceId=id; a.actionLabel="Registrar pista"; a.description=story;
        var glow=Prim(PrimitiveType.Sphere,name+"_indicador",pos+Vector3.up*.55f,Vector3.one*.17f,neon,root.transform,false);
        Object.DestroyImmediate(glow.GetComponent<Collider>());
        glow.AddComponent<AgentPPropPulse>().amplitude = .05f; a.glowObject=glow;
        var light=AddLight("Luz_pista_"+id,LightType.Point,pos+Vector3.up*.85f,new Color(.16f,.95f,1f),2.4f,4.6f,false, root.transform);
        return a;
    }

    static void Door(string name, float z, int required)
    {
        var o=Box(name,new Vector3(0,1.55f,z),new Vector3(3.2f,3.1f,.35f),rustMetal, null, false);
        o.isStatic=false;
        var a=o.AddComponent<AgentPInteractable>(); a.kind=AgentPInteractable.Kind.Door; a.actionLabel="Abrir puerta"; a.requiredClues=required;
        Box("Marco_izq",new Vector3(-1.8f,1.6f,z),new Vector3(.35f,3.2f,.55f),wallDark);
        Box("Marco_der",new Vector3(1.8f,1.6f,z),new Vector3(.35f,3.2f,.55f),wallDark);
        Box("Handle",new Vector3(.9f,1.45f,z+.22f),new Vector3(.18f,.07f,.07f),gold);
    }

    [MenuItem("Tools/Agente P/1 - Generar escenario jugable")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder(Root+"/Materials")) AssetDatabase.CreateFolder(Root,"Materials");
        if (!AssetDatabase.IsValidFolder(Root+"/Scenes")) AssetDatabase.CreateFolder(Root,"Scenes");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);

        dirtyFloor=Mat("Piso_humedo",new Color(.16f,.17f,.18f),0.1f,.58f);
        floorAlt=Mat("Piso_humedo_alt",new Color(.12f,.13f,.14f),0.12f,.72f);
        floorEdge=Mat("Piso_humedo_linea",new Color(.05f,.05f,.06f),0.0f,.18f);
        wall=Mat("Pared_sucia",new Color(.13f,.15f,.17f),0.0f,.2f);
        wallDark=Mat("Pared_oscura",new Color(.08f,.09f,.10f),0.0f,.18f);
        oldWood=Mat("Madera_vieja",new Color(.30f,.18f,.12f),0,.28f);
        rustMetal=Mat("Metal_oxidado",new Color(.24f,.24f,.25f),.6f,.45f);
        trimMat = rustMetal;
        paper=Mat("Papel_viejo",new Color(.78f,.73f,.58f),0,.15f);
        blood=Mat("Sangre",new Color(.33f,.03f,.04f),0,.2f,true,0.6f);
        tape=Mat("Cinta_policial",new Color(.86f,.73f,.08f),0,.18f);
        neon=Mat("Luz_pista",new Color(.14f,.82f,1f),0,.55f,true,2.8f);
        gold=Mat("Luz_final",new Color(1f,.66f,.18f),0,.35f,true,2.6f);
        puddle=Mat("Charco",new Color(.06f,.06f,.07f),.2f,.9f);
        skin=Mat("Piel",new Color(.83f,.69f,.58f),0,.35f);
        coat=Mat("Gabardina",new Color(.22f,.17f,.11f),0,.25f);
        shirt=Mat("Pantalon",new Color(.11f,.12f,.16f),0,.25f);
        blackCloth=Mat("Tela_negra",new Color(.05f,.05f,.06f),0,.22f);
        hair=Mat("Cabello",new Color(.14f,.10f,.07f),0,.25f);
        glass=Mat("Cristal",new Color(.12f,.25f,.32f),.1f,.88f);
        concrete=Mat("Concreto",new Color(.21f,.21f,.23f),0,.28f);

        // OFICINA
        Room("01_Oficina_del_agente",5,30,18,dirtyFloor,wall,4f);
        FloorPanels(5, 29.4f, 17.2f, 6, 4);
        WallEnd(-10,18,0,wall); WallEnd(20,18,4,wall);
        Caption("OFICINA DEL AGENTE P",new Vector3(0,3.35f,-9.5f),new Color(0.9f,0.9f,0.85f));
        Box("DeskTop",new Vector3(0,.86f,7.7f),new Vector3(4.2f,.18f,1.9f),oldWood);
        for(int i=0;i<4;i++) Box("DeskLeg_"+i,new Vector3(i<2?-1.75f:1.75f,.43f, i%2==0?6.95f:8.45f),new Vector3(.18f,.85f,.18f),rustMetal);
        Box("Monitor",new Vector3(0,1.28f,8.15f),new Vector3(1.2f,.72f,.08f),blackCloth);
        Box("Screen",new Vector3(0,1.28f,8.11f),new Vector3(1.05f,.58f,.02f),glass);
        Box("Keyboard",new Vector3(0,.98f,7.32f),new Vector3(.95f,.05f,.28f),blackCloth);
        RotBox("ChairSeat",new Vector3(-2.3f,.46f,7.7f),new Vector3(.65f,.12f,.65f),new Vector3(0,24,-16),blackCloth);
        RotBox("ChairBack",new Vector3(-2.52f,.98f,7.5f),new Vector3(.12f,.9f,.65f),new Vector3(0,24,-16),blackCloth);
        FilingCabinet(new Vector3(-6.7f,0,2.5f)); FilingCabinet(new Vector3(-5.3f,0,2.6f));
        Box("EvidenceBoard",new Vector3(0,2.3f,14.55f),new Vector3(6.6f,2.7f,.14f),oldWood);
        for(int i=0;i<7;i++) {
            float x = -2.4f + (i%4)*1.55f; float y = 1.55f + (i/4)*.95f;
            RotBox("Note_"+i,new Vector3(x,y,14.47f),new Vector3(.95f,.62f,.04f),new Vector3(0,0,Random.Range(-7,7)),paper);
        }
        RotBox("String1",new Vector3(-1.1f,2.15f,14.43f),new Vector3(2.2f,.03f,.02f),new Vector3(0,0,17),blood);
        RotBox("String2",new Vector3(1.2f,1.82f,14.43f),new Vector3(2.2f,.03f,.02f),new Vector3(0,0,-12),blood);
        BrokenCrates(new Vector3(4.7f,0,2.1f), 3);
        BloodStain("Sangre_silla",new Vector3(-1.6f,0.03f,10.2f),new Vector3(.45f,.45f,.02f),22);
        BloodStain("Sangre_pared",new Vector3(7.63f,1.4f,10.7f),new Vector3(.55f,.55f,.03f),0);
        Dust("DustOffice",new Vector3(0,1.5f,7f),new Vector3(12,3,16));
        var officeLight = AddLight("Luz_escritorio",LightType.Spot,new Vector3(0,3.3f,7.7f),new Color(1f,.76f,.54f),14f,9f,true); officeLight.transform.rotation=Quaternion.Euler(90,0,0);
        AddLight("Luz_relleno_oficina",LightType.Point,new Vector3(-5,2.4f,4),new Color(.4f,.55f,.75f),1.8f,7f);
        CreateEvidence("EXPEDIENTE","Expediente con sello Umbra",new Vector3(1.35f,.97f,7.75f),"Un expediente vincula tres escenas del crimen con la organizacion Umbra.");
        CreateEvidence("FOTO","Fotografia rasgada",new Vector3(-1.15f,.97f,7.26f),"La fotografia muestra a una victima junto a una camioneta negra.");
        CreateEvidence("LLAVE","Llave del deposito",new Vector3(5.2f,.42f,4.8f),"La llave abre el deposito 13 y lleva una marca del asesino.");
        Door("Puerta_oficina",20,3);

        // CALLEJON
        Room("02_Callejon_de_las_pistas",38.5f,37,15,concrete,wallDark,6f);
        FloorPanels(38.5f, 36.2f, 14.2f, 7, 3);
        WallEnd(57,15,4,wallDark,6f);
        for(int i=0;i<8;i++) {
            Box("Ladrillo_pared_izq_"+i,new Vector3(-7.25f,1.1f,25+i*3.7f),new Vector3(.14f,.55f,1.4f),concrete);
            Box("Ladrillo_pared_der_"+i,new Vector3(7.25f,1.8f,24+i*4.1f),new Vector3(.14f,.6f,1.2f),concrete);
        }
        for(int i=0;i<3;i++) {
            float z = 27 + i*10.5f;
            Prim(PrimitiveType.Cylinder,"Poste_"+i,new Vector3(5.4f,2.15f,z),new Vector3(.12f,2.2f,.12f),rustMetal, null, true);
            var l = AddLight("Farol_"+i,LightType.Point,new Vector3(5.4f,4.25f,z),new Color(.26f,.63f,1f),2.2f,8,false);
            if (i==1) l.gameObject.AddComponent<AgentPLightFlicker>();
        }
        BrokenCrates(new Vector3(-5.6f,0,26), 4);
        for(int i=0;i<5;i++) Prim(PrimitiveType.Sphere,"BolsaBasura_"+i,new Vector3(-4.6f+i*.7f,.25f,35+Random.Range(-.4f,.4f)),new Vector3(.6f,.45f,.6f),blackCloth,null,true);
        Box("Dumpster",new Vector3(-5.8f,.9f,47),new Vector3(1.8f,1.7f,3),rustMetal);
        RotBox("DumpsterLid",new Vector3(-5.8f,1.85f,47.6f),new Vector3(1.85f,.08f,2.8f),new Vector3(18,0,0),rustMetal);
        for(int i=0;i<4;i++) Puddle("Charco_"+i,new Vector3(Random.Range(-2.8f,2.8f),0.03f,27+i*6.5f),new Vector3(.32f+.1f*i,.32f,.52f+.06f*i),Random.Range(0,180));
        BloodStain("Sangre_callejon_0",new Vector3(1.3f,0.03f,31.4f),new Vector3(.5f,.5f,.03f),33);
        BloodStain("Sangre_callejon_1",new Vector3(-2.0f,0.03f,41.8f),new Vector3(.7f,.55f,.03f),4);
        BloodStain("Sangre_callejon_2",new Vector3(3.7f,0.03f,50.3f),new Vector3(.35f,.35f,.03f),81);
        RotBox("Cinta1",new Vector3(0.5f,1.15f,39.5f),new Vector3(6.5f,.06f,.03f),new Vector3(0,0,-8),tape);
        RotBox("Cinta2",new Vector3(.2f,1.55f,39.3f),new Vector3(6.9f,.06f,.03f),new Vector3(0,0,6),blackCloth);
        Caption("NO PASAR",new Vector3(0,2.4f,39.5f),new Color(1f,.8f,.2f));
        Caption("UMBRA",new Vector3(-5.8f,2.3f,43.5f),new Color(.6f,.07f,.07f));
        Dust("DustAlley",new Vector3(0,2.4f,40f),new Vector3(12,4,30));
        CreateEvidence("SIMBOLO","Simbolo del asesino",new Vector3(-1.4f,.07f,41.2f),"El simbolo Umbra aparece tallado sobre una insignia rota.");
        CreateEvidence("GUANTE","Guante ensangrentado",new Vector3(3.8f,.05f,31.5f),"Un guante con rastros de sangre coincide con la escena.");
        CreateEvidence("HUELLA","Huella de bota",new Vector3(-3.9f,.02f,49.3f),"La huella tiene el mismo dibujo que las botas del vigilante.");
        CreateEvidence("TELEFONO","Telefono roto",new Vector3(2.7f,.05f,52.1f),"El ultimo mensaje recibido dice: nos vemos en el almacen.");
        Door("Puerta_almacen",57,7);

        // ALMACEN
        Room("03_Almacen_Umbra",75,36,21,dirtyFloor,wallDark,7f);
        FloorPanels(75, 35.2f, 20.2f, 7, 4);
        WallEnd(93,21,0,wallDark,7f);
        for(int row=0; row<2; row++) {
            for(int col=0; col<2; col++) {
                float x = -6f + row*12f;
                float z = 67f + col*10f;
                Box("Estante_"+row+"_"+col, new Vector3(x,1.75f,z), new Vector3(0.25f,3.4f,5.2f), rustMetal);
                for(int s=0;s<3;s++) Box("Bandeja_"+row+"_"+col+"_"+s, new Vector3(x,0.55f+s*1.15f,z), new Vector3(2.6f,0.08f,5.2f), rustMetal);
                for(int c=0;c<4;c++) RotBox("Caja_stack_"+row+"_"+col+"_"+c,new Vector3(x + Random.Range(-.7f,.7f),0.3f+(c*.55f),z-1.8f+Random.Range(-1f,1f)),new Vector3(1.1f,.5f,1f),new Vector3(0,Random.Range(-12,12),0), oldWood);
            }
        }
        RotBox("Mesa_revelacion",new Vector3(0,.86f,86),new Vector3(4.6f,.18f,2.2f),new Vector3(0,0,0),oldWood);
        for(int i=0;i<4;i++) Box("PataMesa_"+i,new Vector3(i<2?-2f:2f,.4f,i%2==0?85.2f:86.8f),new Vector3(.18f,.8f,.18f),rustMetal);
        Box("Panel_archivos",new Vector3(0,3,91.4f),new Vector3(9.5f,4.2f,.28f),rustMetal);
        for(int i=0;i<6;i++) RotBox("Doc_final_"+i,new Vector3(-3.8f+(i%3)*3.4f,1.8f+(i/3)*1.4f,91.22f),new Vector3(1.5f,.95f,.05f),new Vector3(0,0,Random.Range(-8,8)),paper);
        BloodStain("Sangre_final_0",new Vector3(.9f,0.03f,84.5f),new Vector3(.7f,.7f,.03f),0);
        BloodStain("Sangre_final_1",new Vector3(-2.7f,0.03f,75.2f),new Vector3(.45f,.45f,.03f),18);
        RotBox("Cadena_0",new Vector3(7.5f,4.9f,71),new Vector3(.08f,3f,.08f),new Vector3(0,0,0),rustMetal);
        RotBox("Cadena_1",new Vector3(7.2f,4.5f,71.2f),new Vector3(.08f,2.2f,.08f),new Vector3(0,0,0),rustMetal);
        Dust("DustWarehouse",new Vector3(0,3,79f),new Vector3(18,5,28));
        var warehouseSpot = AddLight("Luz_almacen_central",LightType.Spot,new Vector3(0,5.8f,86),new Color(1f,.74f,.52f),12f,13,true); warehouseSpot.transform.rotation = Quaternion.Euler(90,0,0);
        var coldSpot = AddLight("Luz_almacen_fria",LightType.Point,new Vector3(-6.5f,4.2f,70),new Color(.18f,.45f,1f),1.9f,9,false); coldSpot.gameObject.AddComponent<AgentPLightFlicker>();
        var redSpot = AddLight("Luz_alerta",LightType.Point,new Vector3(6f,4f,82),new Color(.8f,.1f,.1f),1.2f,8,false); redSpot.gameObject.AddComponent<AgentPLightFlicker>();
        CreateEvidence("REGISTRO","Registro de movimientos",new Vector3(-1.8f,.95f,86.2f),"El registro conecta las tres escenas del caso Umbra.");
        CreateEvidence("IDENTIDAD","Documento de identidad",new Vector3(-3.6f,.05f,74.2f),"El documento identifica a Victor Salas como vigilante del almacen.");
        CreateEvidence("CONFESION","Grabadora con confesion",new Vector3(2.6f,.95f,85.8f),"La grabacion confirma la autoria del asesino y su patron de crimenes.");
        var finale=Prim(PrimitiveType.Cylinder,"Panel_final",new Vector3(1.65f,1.04f,86.2f),new Vector3(.65f,.08f,.65f),gold,null,false); finale.transform.rotation = Quaternion.Euler(90,0,0);
        var finalInteract=finale.AddComponent<AgentPInteractable>(); finalInteract.kind=AgentPInteractable.Kind.Finale; finalInteract.requiredClues=10; finalInteract.actionLabel="Arrestar al asesino";
        var lastGlow=AddLight("Revelacion_final",LightType.Point,new Vector3(0,4.35f,88),new Color(1,.68f,.25f),0f,14f,false); lastGlow.enabled=false;

        // PLAYER AND SYSTEMS
        parent=new GameObject("04_Jugador_y_sistemas").transform;
        var player=new GameObject("AgenteP_Player"); player.transform.SetParent(parent); player.transform.position=new Vector3(0,0.08f,-5);
        var cc=player.AddComponent<CharacterController>(); cc.height=1.85f; cc.radius=.37f; cc.center=new Vector3(0,.92f,0); cc.stepOffset=.28f;
        var playerScript=player.AddComponent<AgentPPlayer>();
        var detectiveModel = Human(player.transform, "Modelo_AgenteP", true, false);
        var anim = detectiveModel.GetComponent<AgentPProceduralAnimator>();
        playerScript.flashlight = anim.flashlightLight;

        var camgo=new GameObject("Main Camera"); camgo.tag="MainCamera"; camgo.transform.SetParent(parent);
        camgo.transform.position=new Vector3(0,3,-10);
        var camera=camgo.AddComponent<Camera>(); camera.fieldOfView=66f; camera.farClipPlane=160; camera.allowHDR=true;
        var cameraData=camgo.AddComponent<UniversalAdditionalCameraData>();cameraData.renderPostProcessing=true;
        camgo.AddComponent<AudioListener>();
        var camScript=camgo.AddComponent<AgentPCamera>(); camScript.target=player.transform; camScript.distance = 4.6f; camScript.sensitivity = 2.2f;
        playerScript.view=camgo.transform;
        if (anim != null) anim.playerView = camgo.transform;
        // El foco ahora pertenece a la linterna en la mano derecha del detective.
        var sys=new GameObject("GameManager");sys.transform.SetParent(parent);
        var game=sys.AddComponent<AgentPGame>();game.gameCamera=camScript;game.endingGlow=lastGlow; game.requiredClues=10; game.timeLimitSeconds=480f;
        var suspectRoot=new GameObject("Asesino_Victor_Salas"); suspectRoot.transform.position = new Vector3(0,0.08f,85); suspectRoot.transform.SetParent(parent);
        Human(suspectRoot.transform, "Modelo_Asesino", false, true); suspectRoot.SetActive(false); game.assassin=suspectRoot;

        // AMBIENT
        var sunGo=new GameObject("Luna_luz_principal"); sunGo.transform.SetParent(parent); sunGo.transform.rotation=Quaternion.Euler(53,-26,0);
        var sun=sunGo.AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(.33f,.47f,.75f); sun.intensity=.52f;sun.shadows=LightShadows.Soft;game.sun=sun;
        RenderSettings.ambientMode=AmbientMode.Flat; RenderSettings.ambientLight=new Color(.18f,.2f,.24f);
        RenderSettings.fog=true; RenderSettings.fogColor=new Color(.06f,.08f,.10f); RenderSettings.fogMode=FogMode.ExponentialSquared; RenderSettings.fogDensity=.02f;
        var volumeGO=new GameObject("Postprocesamiento_GlobalVolume");volumeGO.transform.SetParent(parent);
        var volume=volumeGO.AddComponent<Volume>();volume.isGlobal=true;volume.priority=1;
        var profile=ScriptableObject.CreateInstance<VolumeProfile>(); string profilePath=Root+"/Materials/AgenteP_PostProcessing.asset";
        var existing=AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        if (existing==null) {
            var bloom=profile.Add<Bloom>(); bloom.active=true; bloom.intensity.Override(.48f); bloom.threshold.Override(1.0f);
            var vignette=profile.Add<Vignette>(); vignette.active=true; vignette.intensity.Override(.33f); vignette.smoothness.Override(.5f);
            var colorAdjust=profile.Add<ColorAdjustments>(); colorAdjust.active=true; colorAdjust.contrast.Override(22f); colorAdjust.saturation.Override(-26f); colorAdjust.postExposure.Override(-.2f);
            var tonemap = profile.Add<Tonemapping>(); tonemap.active = true; tonemap.mode.Override(TonemappingMode.ACES);
            AssetDatabase.CreateAsset(profile,profilePath); existing=profile;
        } else Object.DestroyImmediate(profile);
        volume.sharedProfile=existing;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,Root+"/Scenes/AgenteP_Episodio1.unity");
        AssetDatabase.SaveAssets();
        var scenePath=Root+"/Scenes/AgenteP_Episodio1.unity"; EditorBuildSettings.scenes=new [] { new EditorBuildSettingsScene(scenePath,true) };
        Selection.activeGameObject=player;
        Debug.Log("AGENTE P VISUAL: escena terror creada. Input Handling = Both. Pulsa Play.");
        EditorUtility.DisplayDialog("Agente P - Version visual lista", "Se genero una escena mas oscura y presentable con personajes humanoides y props de crimen. Abre Assets/AgenteP/Scenes/AgenteP_Episodio1.unity y pulsa Play.", "OK");
    }
}

#endif
