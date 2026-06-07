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
        var sliders = FindObjectsByType<SlidingPlatform>(FindObjectsSortMode.None);
        foreach (var s in sliders)
        {
            if (!s.isForcedFall) s.ResetPlatform();
        }

        // Also reset jump detection zones
        var zones = FindObjectsByType<JumpDetectionZone>(FindObjectsSortMode.None);
        foreach (var z in zones) z.ResetTrigger();

        hp.Kill(sectionId);
    }
}