using System.Collections;
using UnityEngine;
using MazeMind.Core;

public class CorridorEvent : MonoBehaviour {
    [Header("Trigger once when player enters this collider")]
    public Light[] flickerLights;
    public float flickerDuration = 2.5f;

    bool _fired;

    void OnTriggerEnter(Collider other) {
        if (_fired || !other.CompareTag("Player")) return;
        _fired = true;
        StartCoroutine(Flicker());
        DecisionLogger.I?.Log("CorridorEvent", "2.corridor", "LightFlicker",
            "Did you see that?",
            "Corridor mid-point light flicker triggered.");
    }

    IEnumerator Flicker() {
        float t = 0f;
        while (t < flickerDuration) {
            foreach (var l in flickerLights)
                if (l != null) l.enabled = !l.enabled;
            float wait = Random.Range(0.05f, 0.2f);
            yield return new WaitForSeconds(wait);
            t += wait;
        }
        foreach (var l in flickerLights)
            if (l != null) l.enabled = true;
    }
}