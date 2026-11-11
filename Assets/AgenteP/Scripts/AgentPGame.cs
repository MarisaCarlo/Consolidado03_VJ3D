using UnityEngine;
using System.Collections.Generic;

public class AgentPGame : MonoBehaviour
{
    public static AgentPGame Instance { get; private set; }
    public int clues;
    public int requiredClues = 10;
    public float timeLimitSeconds = 480f;
    public bool finished;
    public bool lost;
    public GameObject assassin;
    private float remaining;
    private bool initialized;
    private string[] caseNotes = new string[10];
    public AgentPCamera gameCamera;
    public Light sun;
    public Light endingGlow;
    public AgentPTimelineSequence cinematic;
    private readonly HashSet<string> collected = new HashSet<string>();
    private string notification = "";
    private float noticeUntil;
    private float finalAt;
    public string Objective => finished ? (lost ? "AGENTE CAIDO" : "ASESINO CAPTURADO") : clues >= requiredClues ? "Regresa al panel de arresto del almacen" : "Encuentra las " + requiredClues + " pruebas antes de que se agote el tiempo";
    void Awake() { Instance = this; Application.targetFrameRate = 60; remaining = timeLimitSeconds; initialized = true; }
    void Update()
    {
        if (!initialized || finished) return;
        remaining -= Time.deltaTime;
        if (remaining <= 0f) Lose();
    }
    public void Lose()
    {
        if (finished) return;
        finished = true; lost = true; finalAt = Time.time;
        Message("EL ASESINO TE ENCONTRO. No reuniste las pruebas a tiempo.", 20f);
        if (assassin) assassin.SetActive(true);
        if (cinematic) cinematic.PlayEnding(true);
        if (gameCamera) gameCamera.Reveal(new Vector3(0, 3.5f, 82), new Vector3(0, 1.5f, 85), 8f);
        if (sun) { sun.color = new Color(1f, .08f, .08f); sun.intensity = 1.2f; }
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
    }
    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
    public void Message(string text, float seconds = 4f) { notification = text; noticeUntil = Time.time + seconds; }
    public string Notice => Time.time < noticeUntil ? notification : "";
    public bool Collect(string id, string text)
    {
        if (!collected.Add(id)) { Message("Esta evidencia ya fue registrada."); return false; }
        clues++;
        Message("PISTA " + clues + "/" + requiredClues + ": " + text, 6f);
        return true;
    }
    public void Finish()
    {
        if (finished) return;
        if (clues < requiredClues) { Message("Necesitas " + requiredClues + " pistas para identificar al asesino."); return; }
        finished = true;
        Message("CASO RESUELTO: las " + requiredClues + " pistas identifican al asesino. Arresto confirmado.", 12f);
        if (gameCamera) gameCamera.Reveal(new Vector3(0, 6, 80), new Vector3(0, 2, 83), 8f);
        if (sun) { sun.color = new Color(1f, .76f, .45f); sun.intensity = 1.45f; }
        if (endingGlow) { endingGlow.enabled = true; endingGlow.intensity = 17f; }
        if (assassin) assassin.SetActive(true);
        if (cinematic) cinematic.PlayEnding(false);
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        finalAt = Time.time;
    }
    void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(16, Screen.height / 42), fontStyle = FontStyle.Bold, wordWrap = true };
        style.normal.textColor = Color.white;
        var small = new GUIStyle(style) { fontSize = Mathf.Max(13, Screen.height / 55), fontStyle = FontStyle.Normal };
        GUI.Box(new Rect(12, 12, Mathf.Min(540, Screen.width - 24), 115), "");
        GUI.Label(new Rect(25, 22, 500, 38), "AGENTE P  |  EXPEDIENTE UMBRA", style);
        GUI.Label(new Rect(25, 61, 500, 30), "Objetivo: " + Objective, small);
        GUI.Label(new Rect(25, 92, 500, 25), "Pistas: " + clues + "/" + requiredClues + "   |   Tiempo: " + Mathf.CeilToInt(Mathf.Max(0,remaining)) + "s", small);
        GUI.Label(new Rect(20, Screen.height - 40, Screen.width - 40, 34), "WASD mover  |  Mouse camara  |  E examinar  |  Shift correr  |  Espacio saltar  |  F linterna  |  Esc cursor", small);
        if (Notice.Length > 0) {
            float w = Mathf.Min(780f, Screen.width - 30);
            GUI.Box(new Rect((Screen.width-w)/2, Screen.height*.74f, w, 90), "");
            GUI.Label(new Rect((Screen.width-w)/2+12, Screen.height*.74f+12, w-24, 70), Notice, style);
        }
        if (finished && Time.time - finalAt > 2f) {
            GUI.Box(new Rect(Screen.width*.2f, Screen.height*.30f, Screen.width*.6f, 115), "");
            GUI.Label(new Rect(Screen.width*.22f, Screen.height*.33f, Screen.width*.56f, 85), (lost ? "HAS PERDIDO\nEl asesino alcanzo al Agente P." : "HAS GANADO\nEl Agente P capturo al asesino."), style);
            if (GUI.Button(new Rect(Screen.width*.4f, Screen.height*.48f, Screen.width*.2f, 45), "REINTENTAR")) Restart();
        }
    }
}

