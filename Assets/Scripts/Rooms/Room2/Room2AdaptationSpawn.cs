// File: Room2AdaptationSpawner.cs
using UnityEngine;
using MazeMind.Core;

public class Room2AdaptationSpawner : MonoBehaviour
{
    [Header("Extra trap GameObjects (disabled by default, enabled if trapDensity >= threshold)")]
    public GameObject[] extraTraps;
    public float trapEnableThreshold = 1.1f;    // trapDensity above this → extra traps ON

    [Header("Moving hazards (already in scene — this sets their base speed)")]
    public MovingHazard[] movingHazards;

    [Header("Health pickup (disabled by default, enabled if needed)")]
    public GameObject healthPickupPrefab;        // leave null to skip spawning
    public Transform healthPickupSpawnPoint;
    public int highDamageThreshold = 40;  // total damage across Room 1

    [Header("Dacoit gem demand label")]
    public DacoitRoom2 dacoit;

    void Start()
    {
        if (AIDirector.I == null) return;
        var s = AIDirector.I.state;
        var m = PlayerMetrics.I;

        // --- Extra traps ---
        foreach (var t in extraTraps)
            if (t != null) t.SetActive(s.trapDensity >= trapEnableThreshold);

        // --- Moving hazard base speed is controlled inside MovingHazard.Update ---
        // Nothing extra needed; they already read hazardSpeedMultiplier live.

        // --- Health pickup ---
        if (healthPickupPrefab != null && healthPickupSpawnPoint != null)
        {
            bool needsHealth = m != null && m.damageTaken >= highDamageThreshold;
            if (needsHealth)
                Instantiate(healthPickupPrefab, healthPickupSpawnPoint.position,
                            healthPickupSpawnPoint.rotation);
        }

        // --- Dacoit demand ---
        if (dacoit != null)
            dacoit.SetDemand( Room2DifficultyManager.GetDacoitDemand());

        LogAdaptations(s, m);
    }
    void LogAdaptations(AdaptationState s, PlayerMetrics m)
    {
        if (DecisionLogger.I == null) return;

        if (s.trapDensity >= trapEnableThreshold)
            DecisionLogger.I.Log("Adaptation", "2.init", "TrapDensityUp",
                "Observed: High confidence. Adaptation: Hazard density increased.",
                $"trapDensity={s.trapDensity:F2} → extra traps enabled.");

        if (s.hazardSpeedMultiplier > 1.05f)
            DecisionLogger.I.Log("Adaptation", "2.init", "HazardSpeedUp",
                "Observed: High sprint usage. Adaptation: Hazard speed increased. Reason: Player rushes through rooms.",
                $"hazardSpeedMultiplier={s.hazardSpeedMultiplier:F2}");

        if (m != null && m.damageTaken >= highDamageThreshold)
            DecisionLogger.I.Log("Adaptation", "2.init", "HealthAdded",
                "Observed: High damage taken. Adaptation: Health pickup added.",
                $"damageTaken={m.damageTaken} → health pickup spawned.");
    }
}