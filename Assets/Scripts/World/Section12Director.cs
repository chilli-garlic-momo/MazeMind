using System.Collections;
using UnityEngine;
using MazeMind.Core;

/// <summary>
/// Section 1.2 — Valley jump.
/// v11 rework: the THIRD platform (PlatformC) is intentionally UNREACHABLE.
/// When the player jumps from PlatformB toward PlatformC, the third platform
/// slides away forever (SlideForever) so the player falls into the pit between
/// B and C. The pit's fall-trigger fires the ForcedFallSequence which fades
/// to black and respawns the player at Spawn_1_5.
///
/// Inspector wiring:
///   platformC              -> Transform of the THIRD platform
///   gap2DetectionZone      -> Box Collider (Is Trigger) above the gap BEFORE C
///                             (the trigger the player crosses when jumping at C)
///   gap3FallTrigger        -> Box Collider (Is Trigger) in the pit below that gap
///   forcedFallSequence     -> ForcedFallSequence component with respawnAt_1_5 set
/// </summary>
public class Section12Director : MonoBehaviour {

    [Header("Platforms")]
    public Transform platformC;
    [Tooltip("How fast PlatformC retreats once player commits the jump. Bigger = obviously unreachable.")]
    public float platformCRetreatSpeed = 12f;

    [Header("Gap-before-C detection (player jumps in)")]
    public Collider gap2DetectionZone;

    [Header("Pit fall trigger (under that gap) + sequence")]
    public Collider gap3FallTrigger;
    public ForcedFallSequence forcedFallSequence;

    private bool _retreating;
    private bool _fallTriggered;

    void OnEnable() {
        if (gap2DetectionZone != null) {
            gap2DetectionZone.isTrigger = true;
            var relay = gap2DetectionZone.gameObject.GetComponent<_TriggerRelay>()
                       ?? gap2DetectionZone.gameObject.AddComponent<_TriggerRelay>();
            relay.onPlayerEnter = OnPlayerCommittedJumpAtC;
        }
        if (gap3FallTrigger != null) {
            gap3FallTrigger.isTrigger = true;
            var relay = gap3FallTrigger.gameObject.GetComponent<_TriggerRelay>()
                       ?? gap3FallTrigger.gameObject.AddComponent<_TriggerRelay>();
            relay.onPlayerEnter = OnPlayerHitPit;
        }
    }

    void OnPlayerCommittedJumpAtC() {
        if (_retreating || platformC == null) return;
        _retreating = true;
        StartCoroutine(RetreatPlatformC());
        DecisionLogger.I?.Log("Room1Gap3", "1.2", "PlatformRetreat",
            "The far edge slipped away.",
            "PlatformC sliding away forever; player will fall into 1.5.");
    }

    IEnumerator RetreatPlatformC() {
        // Slide on local +Z (away from PlatformB). Continues until scene unloads
        // or player respawns elsewhere via ForcedFallSequence.
        while (platformC != null) {
            platformC.position += new Vector3(0, 0, platformCRetreatSpeed * Time.deltaTime);
            yield return null;
        }
    }

    void OnPlayerHitPit() {
        if (_fallTriggered) return;
        _fallTriggered = true;
        DecisionLogger.I?.Log("Room1Gap3", "1.2", "ForcedFall",
            "...",
            "Player fell into the pit (forced). Respawning into Section 1.5.");
        if (forcedFallSequence != null) {
            forcedFallSequence.Play();
        } else {
            Debug.LogWarning("[Section12Director] ForcedFallSequence not assigned.");
        }
    }
}

/// <summary>Helper relay so Section12Director doesn't need to subclass each trigger.</summary>
public class _TriggerRelay : MonoBehaviour {
    public System.Action onPlayerEnter;
    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) onPlayerEnter?.Invoke();
    }
}
