using UnityEngine;

public class AgentPLightFlicker : MonoBehaviour
{
    public float minIntensity = 0.8f;
    public float maxIntensity = 2.4f;
    public float speed = 16f;
    public bool randomizeRange = true;
    private Light l;
    private float baseRange;
    void Awake() { l = GetComponent<Light>(); if (l) baseRange = l.range; }
    void Update()
    {
        if (!l) return;
        float n = Mathf.PerlinNoise(Time.time * 0.7f, speed * 0.1f);
        l.intensity = Mathf.Lerp(minIntensity, maxIntensity, n);
        if (randomizeRange) l.range = baseRange * Mathf.Lerp(0.92f, 1.08f, n);
    }
}
