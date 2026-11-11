using UnityEngine;

/// <summary>
/// Animacion procedural sencilla sin Mixamo ni clips externos.
/// Los brazos y piernas tienen pivotes creados por AgentPSceneBuilder.
/// </summary>
public class AgentPProceduralAnimator : MonoBehaviour
{
    public Transform playerRoot;
    public Transform leftArm, rightArm, leftLeg, rightLeg;
    public GameObject flashlightModel;
    public Light flashlightLight;
    public Transform torchRoot;
    public Transform playerView;

    [Header("Animacion")]
    public float walkFrequency = 7.2f;
    public float runFrequency = 12.0f;
    public float walkLegAngle = 26f;
    public float runLegAngle = 43f;
    public float smooth = 10f;

    CharacterController controller;
    Vector3 previousPosition;
    float cycle;
    float movement;
    Quaternion restLeftArm, restRightArm, restLeftLeg, restRightLeg;
    Vector3 initialLocalPosition;

    void Start()
    {
        if (!playerRoot) playerRoot=transform.parent;
        controller = playerRoot ? playerRoot.GetComponent<CharacterController>() : null;
        previousPosition=playerRoot ? playerRoot.position : transform.position;
        restLeftArm=leftArm ? leftArm.localRotation : Quaternion.identity;
        restRightArm=rightArm ? rightArm.localRotation : Quaternion.identity;
        restLeftLeg=leftLeg ? leftLeg.localRotation : Quaternion.identity;
        restRightLeg=rightLeg ? rightLeg.localRotation : Quaternion.identity;
        initialLocalPosition=transform.localPosition;
        if (flashlightModel) flashlightModel.SetActive(true);
    }
    void LateUpdate()
    {
        if (!playerRoot || Time.deltaTime <= 0) return;
        Vector3 delta = playerRoot.position-previousPosition;
        previousPosition=playerRoot.position;
        float horizontalSpeed = new Vector2(delta.x,delta.z).magnitude / Mathf.Max(Time.deltaTime,0.0001f);
        bool grounded = controller == null || controller.isGrounded;
        float targetMovement = grounded ? Mathf.Clamp01(horizontalSpeed / 4.8f) : 0f;
        movement = Mathf.MoveTowards(movement, targetMovement, Time.deltaTime * 5f);
        bool running = horizontalSpeed > 6f;
        float frequency=running ? runFrequency : walkFrequency;
        if (movement > 0.02f) cycle += frequency * Time.deltaTime;
        float swing = Mathf.Sin(cycle) * movement;
        float armAngle = running ? 26f : 16f;
        float legAngle = running ? runLegAngle : walkLegAngle;
        float leftLegAngle = grounded ? swing * legAngle : -24f;
        float rightLegAngle = grounded ? -swing * legAngle : 24f;
        float leftArmAngle = grounded ? -swing*armAngle : 16f;
        // El brazo derecho sostiene la linterna hacia delante, sin elevarse demasiado.
        float rightArmAngle = grounded ? -38f + swing*5f : -48f;
        Rotate(leftLeg,restLeftLeg, leftLegAngle);
        Rotate(rightLeg,restRightLeg, rightLegAngle);
        Rotate(leftArm,restLeftArm, leftArmAngle);
        Rotate(rightArm,restRightArm, rightArmAngle);
        float bob = grounded ? Mathf.Abs(Mathf.Sin(cycle))*movement*(running ? .07f : .035f) : -.06f;
        transform.localPosition = Vector3.Lerp(transform.localPosition, initialLocalPosition + Vector3.up*bob,Time.deltaTime*smooth);
        if (torchRoot)
        {
            Vector3 aimDir = playerView ? playerView.forward : playerRoot.forward;
            if (aimDir.sqrMagnitude < 0.001f) aimDir = playerRoot.forward;
            aimDir.y = Mathf.Clamp(aimDir.y, -0.08f, 0.08f);
            if (aimDir.sqrMagnitude < 0.001f) aimDir = playerRoot.forward;
            Quaternion desiredTorch = Quaternion.LookRotation(aimDir.normalized, Vector3.up) * Quaternion.Euler(6f, 0f, 0f);
            torchRoot.rotation = Quaternion.Slerp(torchRoot.rotation, desiredTorch, Time.deltaTime * (smooth + 2f));
        }
    }
    void Rotate(Transform limb, Quaternion rest, float angle)
    {
        if (!limb) return;
        limb.localRotation = Quaternion.Slerp(limb.localRotation, rest*Quaternion.Euler(angle,0,0),Time.deltaTime*smooth);
    }
}
