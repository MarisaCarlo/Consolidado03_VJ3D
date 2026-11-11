using UnityEngine;

public class AgentPPerformance : MonoBehaviour
{
    public bool showFps;
    float smoothedDelta=0.016f;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3)) showFps = !showFps;
        smoothedDelta = Mathf.Lerp(smoothedDelta, Time.unscaledDeltaTime, 0.08f);
    }
    void OnGUI()
    {
        if (!showFps) return;
        GUI.Box(new Rect(Screen.width-170,12,158,48),"FPS " + Mathf.RoundToInt(1f/Mathf.Max(.0001f,smoothedDelta)) + "   (F3)");
    }
}
