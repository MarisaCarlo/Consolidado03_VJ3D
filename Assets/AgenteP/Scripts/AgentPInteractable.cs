using UnityEngine;

public class AgentPInteractable : MonoBehaviour
{
    public enum Kind { Evidence, Door, Finale }
    public Kind kind;
    public string evidenceId;
    [TextArea] public string description;
    public int requiredClues;
    public string actionLabel = "Examinar";
    public bool activated;
    public GameObject glowObject;
    public void Interact()
    {
        var game = AgentPGame.Instance;
        if (!game || game.finished) return;
        if (kind == Kind.Evidence) {
            if (!activated && game.Collect(evidenceId, description)) {
                activated = true;
                if (glowObject) glowObject.SetActive(false);
                var render = GetComponent<Renderer>(); if (render) render.material.color = new Color(.25f,.68f,.46f);
            } else if (activated) game.Message(description);
        } else if (kind == Kind.Door) {
            if (game.clues < requiredClues) { game.Message("Acceso bloqueado: busca mas pistas."); return; }
            game.Message("Acceso autorizado.");
            gameObject.SetActive(false);
        } else game.Finish();
    }
}
