using UnityEngine;
using MazeMind.Core;

public class PitDeathTrigger : MonoBehaviour
{
    public string sectionId = "1.2";

    void OnTriggerEnter(Collider other) => Trip(other);
    void OnTriggerStay(Collider other)  => Trip(other);

    void Trip(Collider other) {
        if (!other.CompareTag("Player")) return;
        var hp = other.GetComponent<PlayerHealth>();
        if (hp == null || hp.IsInvuln) return;

#pragma warning disable CS0618
        var sliders = FindObjectsByType<SlidingPlatform>(FindObjectsSortMode.None);
        foreach (var s in sliders)
        {
            if (!s.isForcedFall) s.ResetPlatform();
        }
        var zones = FindObjectsByType<JumpDetectionZone>(FindObjectsSortMode.None);
        foreach (var z in zones) z.ResetTrigger();
#pragma warning restore CS0618

        hp.Kill(sectionId);
    }
}
