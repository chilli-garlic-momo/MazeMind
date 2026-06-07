// File: SpikeTrap.cs
using UnityEngine;

public class SpikeTrap : MonoBehaviour {
    public int damageOnContact = 25;

    void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        other.GetComponent<PlayerHealth>()?.Damage(damageOnContact);
    }
}