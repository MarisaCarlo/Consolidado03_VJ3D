using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AgentPPlayer : MonoBehaviour
{
    public Transform view;
    public Light flashlight;
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 1.1f;
    private CharacterController cc;
    private float verticalVelocity;
    private AgentPInteractable target;
    void Start() { cc = GetComponent<CharacterController>(); Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { bool locked = Cursor.lockState == CursorLockMode.Locked; Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked; Cursor.visible = locked; }
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        if (Input.GetKeyDown(KeyCode.F) && flashlight) flashlight.enabled = !flashlight.enabled;
        if (!cc || !cc.enabled || (AgentPGame.Instance && AgentPGame.Instance.finished)) return;
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input = Vector2.ClampMagnitude(input, 1f);
        var f = view ? view.forward : Vector3.forward;
        var r = view ? view.right : Vector3.right;
        f.y = 0; r.y = 0; f.Normalize(); r.Normalize();
        Vector3 direction = f * input.y + r * input.x;
        if (direction.sqrMagnitude > .01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime*12f);
        if (cc.isGrounded && verticalVelocity < 0) verticalVelocity = -2;
        if (cc.isGrounded && Input.GetButtonDown("Jump")) verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * -20f);
        verticalVelocity -= 20f * Time.deltaTime;
        cc.Move((direction * (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed) + Vector3.up * verticalVelocity)*Time.deltaTime);
        FindTarget();
        if (target && Input.GetKeyDown(KeyCode.E)) target.Interact();
    }
    void FindTarget()
    {
        target = null;
        Camera cam = Camera.main;
        if (!cam) return;
        var origin = transform.position + Vector3.up * 1.1f;
        var hits = Physics.OverlapSphere(origin, 2.6f);
        float nearest = 999;
        foreach (var h in hits) {
            var a = h.GetComponentInParent<AgentPInteractable>();
            if (!a || !a.gameObject.activeInHierarchy) continue;
            float d = Vector3.Distance(origin, a.transform.position);
            if (d < nearest) { nearest = d; target = a; }
        }
    }
    void OnGUI()
    {
        if (!target || Cursor.lockState != CursorLockMode.Locked) return;
        var s = new GUIStyle(GUI.skin.box) { fontSize = 19, fontStyle = FontStyle.Bold };
        GUI.Box(new Rect(Screen.width/2f-145, Screen.height*.83f, 290, 42), "[E] " + target.actionLabel, s);
    }
}
