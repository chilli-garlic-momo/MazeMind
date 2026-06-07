// File: Room2AdaptationSpawner.cs
using UnityEngine; using MazeMind.Core;

public class Room2AdaptationSpawner : MonoBehaviour {
    [Header("Extra trap GameObjects (disabled by default, enabled if trapDensity >= threshold)")]
    public GameObject[] extraTraps;
    public float trapEnableThreshold = 1.1f;    // trapDensity above this → extra traps ON

    [Header("Moving hazards (already in scene — this sets their base speed)")]
    public MovingHazard[] movingHazards;

    [Header("Health pickup (disabled by default, enabled if needed)")]
    public GameObject healthPickupPrefab;        // leave null to skip spawning
    public Transform  healthPickupSpawnPoint;
    public int        highDamageThreshold = 40;  // total damage across Room 1

    [Header("Dacoit gem demand label")]
    public DacoitRoom2 dacoit;

    void Start() {
        if (AIDirector.I == null) return;
        var s = AIDirector.I.state;
        var m = PlayerMetrics.I;

        // --- Extra traps ---
        foreach (var t in extraTraps)
            if (t != null) t.SetActive(s.trapDensity >= trapEnableThreshold);

        // --- Moving hazard base speed is controlled inside MovingHazard.Update ---
        // Nothing extra needed; they already read hazardSpeedMultiplier live.

        // --- Health pickup ---
        if (healthPickupPrefab != null && healthPickupSpawnPoint != null) {
            bool needsHealth = m != null && m.damageTaken >= highDamageThreshold;
            if (needsHealth)
                Instantiate(healthPickupPrefab, healthPickupSpawnPoint.position,
                            healthPickupSpawnPoint.rotation);
        }

        // --- Dacoit demand ---
        if (dacoit != null)
            dacoit.SetDemand(s.dacoitGemDemand);
    }
}