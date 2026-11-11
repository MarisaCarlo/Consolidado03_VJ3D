using UnityEngine;

public class AgentPCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 5f;
    public float sensitivity = 2f;
    private float yaw;
    private float pitch = 18f;
    private bool revealing;
    private float revealEnd;
    private Vector3 revealPosition;
    private Vector3 revealLookAt;
    public void Reveal(Vector3 position, Vector3 lookAt, float seconds)
    { revealPosition = position; revealLookAt = lookAt; revealEnd = Time.time + seconds; revealing = true; }
    void LateUpdate()
    {
        if (!target) return;
        if (revealing && Time.time < revealEnd) {
            transform.position = Vector3.Lerp(transform.position, revealPosition, Time.deltaTime*1.9f);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(revealLookAt-transform.position), Time.deltaTime*1.9f);
            return;
        }
        revealing = false;
        if (Cursor.lockState == CursorLockMode.Locked) { yaw += Input.GetAxis("Mouse X")*sensitivity; pitch -= Input.GetAxis("Mouse Y")*sensitivity; }
        pitch = Mathf.Clamp(pitch, -12, 62);
        Quaternion rotation = Quaternion.Euler(pitch,yaw,0);
        var anchor = target.position + Vector3.up * 1.55f;
        Vector3 desired = anchor - rotation * Vector3.forward * distance;
        if (Physics.SphereCast(anchor, .18f, (desired-anchor).normalized, out var hit, distance, ~0, QueryTriggerInteraction.Ignore))
            if (hit.transform != target) desired = anchor + (desired-anchor).normalized*Mathf.Max(.6f,hit.distance-.25f);
        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime*12f);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime*12f);
    }
}
