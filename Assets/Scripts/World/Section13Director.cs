using UnityEngine; using MazeMind.Core;

/// <summary>
/// Section 1.3 — laser maze coordinator.
/// v11: ALSO acts as the section's re-entry trigger.
/// Attach this on the Section_1_3 root and assign:
///  - floorGenerator  : the CheckboxFloorGenerator
///  - entryTrigger    : a Box Collider (Is Trigger) covering the 1.3 doorway
///  - entrySpawnPoint : empty Transform at the doorway tile (player respawn here on trap death)
///
/// EVERY time the player walks into entryTrigger:
///   - registers a checkpoint at entrySpawnPoint (so trap deaths respawn here)
///   - regenerates the maze (new safe path, new key location)
/// Result: dying on a trap respawns at 1.3 start; being bounced back from
/// 1.5 ("go find the key") and re-entering 1.3 gives a fresh layout.
/// </summary>
public class Section13Director : MonoBehaviour {
    [Header("Re-entry wiring (v11)")]
    public CheckboxFloorGenerator floorGenerator;
    public Collider               entryTrigger;
    public Transform              entrySpawnPoint;

    [Header("Cooldown so we don't regen mid-step")]
    public float regenCooldownSec = 1.5f;
    float _lastRegen = -999f;

    [Header("Legacy hints")]
    public GameObject guideCat;
    public AudioSource voiceHint;

    void Awake() {
        if (entryTrigger != null) {
            entryTrigger.isTrigger = true;
            var relay = entryTrigger.gameObject.GetComponent<_S13EntryRelay>()
                       ?? entryTrigger.gameObject.AddComponent<_S13EntryRelay>();
            relay.director = this;
        }
    }

    public void OnPlayerEnteredSection(Collider playerCol) {
        if (Time.time - _lastRegen < regenCooldownSec) return;
        _lastRegen = Time.time;

        // 1. Register checkpoint at 1.3 entry so trap deaths respawn HERE, not 1.1.
        if (entrySpawnPoint != null) {
            var hp = playerCol.GetComponent<PlayerHealth>();
            if (hp != null) hp.RegisterCheckpoint(entrySpawnPoint.position, "1.3");
        }

        // 2. Notify metrics / AI we entered 1.3.
        PlayerMetrics.I?.OnEnterSection("1.3");
        AIDirector.I?.Fire(TriggerKind.OnSectionEnter, "1.3", 1);

        // 3. Re-shuffle the maze for a fresh layout.
        if (floorGenerator != null) {
            floorGenerator.Regenerate();
            DecisionLogger.I?.Log("Section13Regen", "1.3", "MazeShuffled",
                "The floor has changed.",
                "Player re-entered 1.3; maze regenerated with new safe path.");
        }
    }

    void Update() {
        if (AIDirector.I == null || AIDirector.I.state == null) return;
        var s = AIDirector.I.state;
        if (guideCat != null && s.spawn13GuideAnimal && !guideCat.activeSelf)
            guideCat.SetActive(true);
        if (voiceHint != null && s.give13VoiceHint && !voiceHint.isPlaying)
            voiceHint.Play();
    }
}

/// <summary>Tiny relay so the entry trigger forwards to Section13Director without subclassing.</summary>
public class _S13EntryRelay : MonoBehaviour {
    public Section13Director director;
    void OnTriggerEnter(Collider other) {
        if (director == null || !other.CompareTag("Player")) return;
        director.OnPlayerEnteredSection(other);
    }
}
