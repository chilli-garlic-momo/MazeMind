using UnityEngine;
using MazeMind.Core;

public class PitDeathTrigger : MonoBehaviour
{
    public string sectionId = "1.2";

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var hp = other.GetComponent<PlayerHealth>();
        if (hp == null) return;

        // Reset any sliding platforms in the section
#pragma warning disable CS0618 // FindObjectsSortMode obsolete warning in Unity 6 — API still functional
        var sliders = FindObjectsByType<SlidingPlatform>(FindObjectsSortMode.None);
        foreach (var s in sliders)
        {
            if (!s.isForcedFall) s.ResetPlatform();
        }

        // Also reset jump detection zones
        var zones = FindObjectsByType<JumpDetectionZone>(FindObjectsSortMode.None);
        foreach (var z in zones) z.ResetTrigger();
#pragma warning restore CS0618

        hp.Kill(sectionId);
    }
}
