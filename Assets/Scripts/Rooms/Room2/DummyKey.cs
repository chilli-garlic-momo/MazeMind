// File: DummyKey.cs
using UnityEngine;
using MazeMind.Core;

public class DummyKey : MonoBehaviour {
    public RealKeyRelocator relocator;
    public ExitDoorRoom2   exitDoor;   // assign in Inspector — no FindObjectOfType needed

    void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;

        DecisionLogger.I?.Log(
            "DummyKeyGrabbed", "2.puzzle", "DummyKeyPickup",
            "Wrong key. The correct key has moved. Check the spawn room.",
            "Dummy key collected — real key relocated, hazards swapped.");

        relocator?.Relocate();
        CorridorHazardSwapper.I?.SwapHazards();
        exitDoor?.NotifyDummyKeyCollected();
        Destroy(gameObject);
    }
}