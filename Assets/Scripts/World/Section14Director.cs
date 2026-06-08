using System.Collections.Generic;
using UnityEngine;
using MazeMind.Core;

/// <summary>
/// Section 1.4 — color-floor corridor. Director picks a variant from
/// AIDirector.state.trapDensity and applies it on Start.
///
/// Variants:
///  - Honest        : nothing happens. Walk straight through.
///  - SwitchMidway  : at the midpoint of the corridor, the safe colour flips;
///                    tiles re-tag and re-colour live.
///  - SpikeRhythm   : every Nth tile gets a SpikeTrap that pulses on/off,
///                    and a MovingHazard slides across the corridor.
/// </summary>
public class Section14Director : MonoBehaviour {

    [Header("Corridor tiles (red = trap, black = safe by default)")]
    public List<GameObject> redTiles  = new();
    public List<GameObject> blackTiles = new();

    [Header("Prefabs")]
    public GameObject spikeTrapPrefab;     // SpikeTrap.cs
    public GameObject movingHazardPrefab;  // MovingHazard.cs

    [Header("Spike rhythm")]
    public int   spikeEveryNth = 3;
    public float spikePulseRateHz = 1.2f;

    [Header("Moving hazard")]
    public Transform movingHazardSpawn;    // optional anchor
    public float movingHazardRange = 4f;
    public float movingHazardBaseSpeed = 2.5f;

    private readonly List<SpikeTrap> _spawnedSpikes = new();
    private MovingHazard _hazardInstance;
    private Section14Variant _variant;
    private bool _midwayApplied;

    void Start() {
        _variant = (AIDirector.I != null)
            ? AIDirector.I.ComputeRoom14Variant()
            : Section14Variant.Honest;

        switch (_variant) {
            case Section14Variant.Honest:        ApplyHonest();       break;
            case Section14Variant.SwitchMidway:  ApplyHonest();       break; // flip later in Update
            case Section14Variant.SpikeRhythm:   ApplySpikeRhythm();  break;
        }

        SpawnMovingHazard();
    }

    void Update() {
        if (_variant == Section14Variant.SwitchMidway && !_midwayApplied) {
            // Trigger flip when the player has crossed the midpoint of the corridor.
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;
            float mid = AverageZ();
            if (player.transform.position.z >= mid) {
                FlipSafeColour();
                _midwayApplied = true;
            }
        }

        // Pulse the spike colliders for SpikeRhythm.
        if (_variant == Section14Variant.SpikeRhythm) {
            bool armed = Mathf.PingPong(Time.time * spikePulseRateHz, 1f) > 0.5f;
            foreach (var s in _spawnedSpikes) {
                if (s == null) continue;
                var col = s.GetComponent<Collider>();
                if (col != null) col.enabled = armed;
                s.gameObject.GetComponentInChildren<MeshRenderer>(true)?.gameObject.SetActive(armed);
            }
        }
    }

    void ApplyHonest() {
        TagSafeAndTrap(blackTiles, redTiles);
    }

    void ApplySpikeRhythm() {
        TagSafeAndTrap(blackTiles, redTiles);
        if (spikeTrapPrefab == null) return;

        // Order red tiles along Z so "every Nth" is meaningful.
        var ordered = new List<GameObject>(redTiles);
        ordered.Sort((a, b) => a.transform.position.z.CompareTo(b.transform.position.z));

        for (int i = 0; i < ordered.Count; i++) {
            if (i % Mathf.Max(1, spikeEveryNth) != 0) continue;
            var t = ordered[i];
            var spike = Instantiate(spikeTrapPrefab,
                t.transform.position + Vector3.up * 0.3f,
                Quaternion.identity, t.transform);
            var st = spike.GetComponent<SpikeTrap>();
            if (st != null) _spawnedSpikes.Add(st);
        }
    }

    void FlipSafeColour() {
        TagSafeAndTrap(redTiles, blackTiles);
        Debug.Log("[Section14Director] Safe colour FLIPPED at midpoint.");
    }

    void TagSafeAndTrap(List<GameObject> safe, List<GameObject> trap) {
        foreach (var t in safe) {
            if (t == null) continue;
            t.tag = "SafeTile";
            var col = t.GetComponent<Collider>();
            if (col != null) col.isTrigger = false;
            var bt = t.GetComponent<BulletTrap>();
            if (bt != null) Destroy(bt);
        }
        foreach (var t in trap) {
            if (t == null) continue;
            t.tag = "TrapTile";
            var col = t.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            if (t.GetComponent<BulletTrap>() == null)
                t.AddComponent<BulletTrap>();
        }
    }

    void SpawnMovingHazard() {
        if (movingHazardPrefab == null) return;
        Vector3 pos = movingHazardSpawn != null ? movingHazardSpawn.position : transform.position;
        var go = Instantiate(movingHazardPrefab, pos, Quaternion.identity, transform);
        _hazardInstance = go.GetComponent<MovingHazard>();
        if (_hazardInstance != null) {
            _hazardInstance.axis = MovingHazard.Axis.X;
            _hazardInstance.range = movingHazardRange;
            _hazardInstance.baseSpeed = movingHazardBaseSpeed;
        }
    }

    float AverageZ() {
        float sum = 0; int n = 0;
        foreach (var t in redTiles)   { if (t != null) { sum += t.transform.position.z; n++; } }
        foreach (var t in blackTiles) { if (t != null) { sum += t.transform.position.z; n++; } }
        return n > 0 ? sum / n : transform.position.z;
    }
}
