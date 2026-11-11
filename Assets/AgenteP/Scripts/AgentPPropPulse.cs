using UnityEngine;

public class AgentPPropPulse : MonoBehaviour
{
    public float amplitude = .12f;
    private Vector3 initial;
    void Start() { initial = transform.position; }
    void Update() { transform.position = initial + Vector3.up * Mathf.Sin(Time.time * 2f)*amplitude; transform.Rotate(0, Time.deltaTime*28f, 0, Space.World); }
}
