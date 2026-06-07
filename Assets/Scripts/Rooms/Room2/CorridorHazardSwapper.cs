using UnityEngine;

public class CorridorHazardSwapper : MonoBehaviour {
    public static CorridorHazardSwapper I { get; private set; }

    [Header("Active at room start — odd tiles")]
    public GameObject[] phaseOneTiles;   // Tile1,3,5,7,8,10

    [Header("Activated after dummy key — even tiles")]
    public GameObject[] phaseTwoTiles;   // Tile2,4,6,9,11,12

    void Awake() {
        I = this;
        // Start: phase one active, phase two inactive
        SetActive(phaseOneTiles, true);
        SetActive(phaseTwoTiles, false);
    }

    // Called by DummyKey when player picks it up
    public void SwapHazards() {
        SetActive(phaseOneTiles, false);
        SetActive(phaseTwoTiles, true);
        Debug.Log("[CorridorHazardSwapper] Hazards swapped to phase two.");
    }

    void SetActive(GameObject[] tiles, bool state) {
        foreach (var t in tiles)
            if (t != null) t.SetActive(state);
    }
}