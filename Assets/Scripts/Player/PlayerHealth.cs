using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using MazeMind.Core;

public class PlayerHealth : MonoBehaviour {
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    public float invulnSecondsAfterRespawn = 1.0f;

    [Header("Respawn fallback (used if no SectionTrigger entered yet)")]
    public Transform fallbackSpawn;
    public string fallbackSectionId = "1.1";

    [Header("Safety net")]
    [Tooltip("If the player ever falls below this Y, kill them. Catches missed PitDeathTrigger setups.")]
    public float killBelowY = -15f;

    [System.Serializable] public class HealthEvent : UnityEvent<int,int> {}
    public HealthEvent OnHealthChanged;

    Vector3 _checkpointPos;
    string  _currentSectionId = "1.1";
    bool    _invuln;
    bool    _respawning;
    CharacterController _cc;
    MonoBehaviour _controller;

    /// <summary>
    /// True if the player should not take damage right now (post-respawn window,
    /// currently dying, or already at 0 hp).
    /// </summary>
    public bool IsInvuln => _invuln || _respawning || currentHealth <= 0;

    void Awake() {
        currentHealth = maxHealth;
        _cc = GetComponent<CharacterController>();
        _controller = GetComponent("PlayerController") as MonoBehaviour;
        if (fallbackSpawn != null) _checkpointPos = fallbackSpawn.position;
        else _checkpointPos = transform.position;
    }

    void Update() {
        if (!_respawning && transform.position.y < killBelowY) {
            Kill(_currentSectionId);
        }
    }

    public void RegisterCheckpoint(Vector3 pos, string sectionId) {
        _checkpointPos = pos;
        _currentSectionId = sectionId;
    }

    public void Damage(int amount) {
        if (IsInvuln || amount <= 0) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        PlayerMetrics.I?.OnDamage(amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0) Die(_currentSectionId);
    }

    public void Heal(int amount) {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>Instant death from a trap. Deduped against active respawn / invuln.</summary>
    public void Kill(string sectionId) {
        if (IsInvuln) return;
        currentHealth = 0;
        OnHealthChanged?.Invoke(0, maxHealth);
        Die(sectionId);
    }

    void Die(string sectionId) {
        if (_respawning) return;
        _respawning = true;
        PlayerMetrics.I?.OnDeath(sectionId);
        AIDirector.I?.Fire(TriggerKind.OnSectionDeath, sectionId, 1);
        DecisionLogger.I?.Log("OnSectionDeath", sectionId, "PlayerDeath", "", $"Death at {sectionId}");
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine() {
        if (_controller != null) _controller.enabled = false;
        yield return new WaitForSeconds(0.4f);

        if (_cc != null) _cc.enabled = false;
        transform.position = _checkpointPos;
        if (_cc != null) _cc.enabled = true;

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Re-enable controller, then run an invuln window BEFORE clearing _respawning.
        _invuln = true;
        if (_controller != null) _controller.enabled = true;
        yield return new WaitForSeconds(invulnSecondsAfterRespawn);
        _invuln = false;
        _respawning = false;
    }
}
