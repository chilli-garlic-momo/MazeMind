// File: HealthPickup.cs
using UnityEngine;

public class HealthPickup : MonoBehaviour {
    public int healAmount = 20;   // scaled to 100-HP system (≈2 old HP)

    void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        other.GetComponent<PlayerHealth>()?.Heal(healAmount);
        Destroy(gameObject);
    }
}