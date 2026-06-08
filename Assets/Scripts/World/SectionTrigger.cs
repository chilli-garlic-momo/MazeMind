using UnityEngine; using MazeMind.Core;
// v11: added fireEverytime so the 1.3 trigger can re-fire on re-entry
// (important when the player is bounced back from 1.5 for missing the key).
public class SectionTrigger : MonoBehaviour {
    public string sectionId = "1.1";
    public int roomId = 1;
    [Tooltip("If true, fires on every entry (use for 1.3 so re-entry re-registers checkpoint). If false, fires once.")]
    public bool fireEverytime = false;
    public bool oneShot = true;

    bool fired;
    void Reset() {
        var c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }
    void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        if (!fireEverytime && fired) return;
        if (oneShot) fired = true;
        PlayerMetrics.I?.OnEnterSection(sectionId);
        AIDirector.I?.Fire(TriggerKind.OnSectionEnter, sectionId, roomId);
        var hp = other.GetComponent<PlayerHealth>();
        if (hp != null) hp.RegisterCheckpoint(transform.position, sectionId);
    }
}
