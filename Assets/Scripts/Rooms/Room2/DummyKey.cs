// File: DummyKey.cs
using UnityEngine; using MazeMind.Core;

public class DummyKey : MonoBehaviour {
    [Tooltip("Assign the RealKeyRelocator in the scene so it can push the real key away.")]
    public RealKeyRelocator relocator;

    void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;

        // Tell AI Director the player grabbed the dummy key
        AIDirector.I?.Fire(TriggerKind.OnSectionEnter, "dummy", 2);
        DecisionLogger.I?.Log("DummyKeyGrabbed", "2.x", "DummyKeyPickup",
            "You found something... but is it the right one?",
            "Player grabbed dummy key — real key relocated, Greedy+10 logged.");

        relocator?.Relocate();
        Destroy(gameObject);
    }
}