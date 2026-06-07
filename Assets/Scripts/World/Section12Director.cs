using System.Collections;
using UnityEngine;
using MazeMind.Core;

/// <summary>
/// Section 1.2 — Valley jump with three gaps.
///
///  Gap 1 (A -> B):  honest, fixed width.
///  Gap 2 (B -> C):  WIDENS mid-jump. When the player crosses the
///                   gap2DetectionZone trigger, PlatformC slides further away.
///                   Single-jump fails; double-jump still clears.
///  Gap 3 (C -> ?):  no opposite platform. Player must fall. The fall is
///                   framed as death (fade-to-black + scream) but actually
///                   teleports them to Spawn_1_5 (handed off to Section 1.5).
/// </summary>
public class Section12Director : MonoBehaviour {

    [Header("Platforms")]
    public Transform platformC;            // the platform that widens away during gap-2
    public Vector3   platformCWidenOffset = new Vector3(0, 0, 2.0f); // base widen distance
    public float     widenDuration = 0.35f;

    [Header("Gap 2 detection")]
    public Collider gap2DetectionZone;     // trigger above gap 2

    [Header("Gap 3 forced fall")]
    public Collider gap3FallTrigger;       // trigger below gap 3 / floor of pit
    public ForcedFallSequence forcedFallSequence; // assign — uses Spawn_1_5 as respawn

    private Vector3 _platformCStart;
    private bool _widened;
    private bool _fallTriggered;

    void Start() {
        if (platformC != null) _platformCStart = platformC.localPosition;

        // Widen amount scaled by AI director
        if (AIDirector.I != null) {
            float w = AIDirector.I.ComputeRoom12Gap2Widen();
            platformCWidenOffset *= w;
        }
    }

    void OnEnable() {
        if (gap2DetectionZone != null) {
            var relay = gap2DetectionZone.gameObject.GetComponent<_TriggerRelay>()
                       ?? gap2DetectionZone.gameObject.AddComponent<_TriggerRelay>();
            relay.onPlayerEnter = OnPlayerEnteredGap2;
        }
        if (gap3FallTrigger != null) {
            var relay = gap3FallTrigger.gameObject.GetComponent<_TriggerRelay>()
                       ?? gap3FallTrigger.gameObject.AddComponent<_TriggerRelay>();
            relay.onPlayerEnter = OnPlayerHitGap3;
        }
    }

    void OnPlayerEnteredGap2() {
        if (_widened || platformC == null) return;
        _widened = true;
        StartCoroutine(WidenPlatform());
        DecisionLogger.I?.Log("Room1Gap2", "1.2", "WidenGap",
            "The floor moved.",
            $"Gap 2 widened by {platformCWidenOffset.z:F2}m mid-jump.");
    }

    IEnumerator WidenPlatform() {
        Vector3 from = platformC.localPosition;
        Vector3 to   = _platformCStart + platformCWidenOffset;
        float t = 0f;
        while (t < widenDuration) {
            t += Time.deltaTime;
            platformC.localPosition = Vector3.Lerp(from, to, t / widenDuration);
            yield return null;
        }
        platformC.localPosition = to;
    }

    void OnPlayerHitGap3() {
        if (_fallTriggered) return;
        _fallTriggered = true;
        DecisionLogger.I?.Log("Room1Gap3", "1.2", "ForcedFall",
            "...",
            "Player fell into gap 3 (forced). Respawning into Section 1.5.");
        if (forcedFallSequence != null) {
            forcedFallSequence.Play();
        } else {
            Debug.LogWarning("[Section12Director] ForcedFallSequence not assigned.");
        }
    }
}

/// <summary>
/// Tiny helper so Section12Director doesn't have to subclass each trigger.
/// </summary>
public class _TriggerRelay : MonoBehaviour {
    public System.Action onPlayerEnter;
    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) onPlayerEnter?.Invoke();
    }
}
