using UnityEngine;
using UnityEngine.Playables;

/// <summary>Secuencia de revelacion del caso Umbra. Reproduce un Timeline de 5 segundos.</summary>
public class AgentPTimelineSequence : MonoBehaviour
{
    public PlayableDirector director;
    public Light revealLight;
    bool playing;
    public void PlayEnding(bool defeat)
    {
        if (playing) return;
        playing = true;
        if (revealLight) { revealLight.enabled = true; revealLight.color = defeat ? new Color(0.7f,0.08f,0.08f) : new Color(0.9f,0.65f,0.35f); }
        if (director != null && director.playableAsset != null) { director.time = 0; director.Play(); }
    }
}
