// NOTE: Filename MUST equal class name in Unity. File = BulletTrap.cs, class = BulletTrap.
using UnityEngine; using MazeMind.Core;

public class BulletTrap : MonoBehaviour {
    [Tooltip("Section this trap belongs to. Used for death attribution in metrics.")]
    public string sectionId = "1.3";

    void OnTriggerEnter(Collider other) => TryKill(other);
    // OnTriggerStay: after a respawn the player may be teleported on top of a trap
    // tile, or the invuln window may end while standing on one. Without Stay the
    // trap appeared "disabled" because OnTriggerEnter only fires on a fresh entry.
    void OnTriggerStay(Collider other)  => TryKill(other);

    void TryKill(Collider other) {
        if (!other.CompareTag("Player")) return;
        var hp = other.GetComponent<PlayerHealth>();
        if (hp == null || hp.IsInvuln) return;
        if (Camera.main != null && Camera.main.transform.parent != null)
            Camera.main.transform.parent.SendMessage("OnHit", SendMessageOptions.DontRequireReceiver);
        hp.Kill(sectionId);
    }
}
