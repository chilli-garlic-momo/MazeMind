using System.Collections;
using UnityEngine;
using MazeMind.Core;

/// <summary>
/// Section 1.2 — Valley jump with three gaps.
///
///  Gap 1 (A -> B):  honest, fixed width. PitDeathTrigger underneath kills on miss.
///  Gap 2 (B -> C):  WIDENS visibly mid-jump. Single-jump fails; double-jump clears.
///                   PitDeathTrigger underneath kills on miss.
///  Gap 3 (C -> ?):  no opposite platform. Trigger volume in mid-air forces the
///                   ForcedFallSequence (fade-to-black + scream + respawn at 1.5).
///                   Do NOT put a PitDeathTrigger under Gap 3 — it would steal
///                   the kill from the forced-fall sequence.
/// </summary>
public class Section12Director : MonoBehaviour {

    [Header("Platforms")]
    public Transform platformC;
    [Tooltip("How far PlatformC slides on Z when Gap 2 widens. Pumped up so it's VISIBLY moving — was too subtle.")]
    public Vector3   platformCWidenOffset = new Vector3(0, 0, 3.2f);
    public float     widenDuration = 0.45f;

    [Header("Gap 2 detection")]
    public Collider gap2DetectionZone;

    [Header("Gap 3 forced fall")]
    public Collider gap3FallTrigger;
    public ForcedFallSequence forcedFallSequence;

    private Vector3 _platformCStart;
    private bool _widened;
    private bool _fallTriggered;

    void Start() {
        if (platformC != null) _platformCStart = platformC.localPosition;
        if (AIDirector.I != null) {
            float w = AIDirector.I.ComputeRoom12Gap2Widen();
            // Clamp so the widen is always at least visible.
            w = Mathf.Max(w, 0.75f);
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

/// <summary>Helper relay so Section12Director doesn't need to subclass each trigger.</summary>
public class _TriggerRelay : MonoBehaviour {
    public System.Action onPlayerEnter;
    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) onPlayerEnter?.Invoke();
    }
}
