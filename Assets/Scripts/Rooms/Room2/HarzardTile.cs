// File: HazardTile.cs

using UnityEngine;
using MazeMind.Core;

public class HazardTile : MonoBehaviour
{
    [Header("Damage")]
    public int damageOnContact = 10;

    [Header("Cooldown")]
    public float damageCooldown = 1f;

    private float _lastDamageTime;

    void Start()
    {
        Debug.Log("Hazard active on " + gameObject.name);
    }
    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Triggering with: " + other.name);

        if (!other.CompareTag("Player"))
            return;

        if (Time.time < _lastDamageTime + damageCooldown)
            return;

        var hp = other.GetComponent<PlayerHealth>();
        hp?.Damage(damageOnContact);

        _lastDamageTime = Time.time;

        DecisionLogger.I?.Log(
            "Hazard",
            gameObject.name,
            "Damage",
            $"Player took {damageOnContact} damage.",
            "Stepped on hazard tile."
        );
    }
}