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
    [Tooltip("If the player ever falls more than this far BELOW the current checkpoint Y, kill them. " +
             "Was an absolute world Y in earlier versions — now relative to the active checkpoint so " +
             "sections built far below the maze (e.g. 1.5) don't false-trigger the kill.")]
    public float killBelowOffsetFromCheckpoint = 20f;

    [System.Serializable] public class HealthEvent : UnityEvent<int,int> {}
    public HealthEvent OnHealthChanged;

    Vector3 _checkpointPos;
    string  _currentSectionId = "1.1";
    bool    _invuln;
    bool    _respawning;
    CharacterController _cc;
    MonoBehaviour _controller;

    public bool IsInvuln => _invuln || _respawning || currentHealth <= 0;

    /// <summary>
    /// Public toggle so cinematic sequences (e.g. ForcedFallSequence) can suppress
    /// the safety-net kill while the player is mid-fall / mid-teleport.
    /// </summary>
    public void SetInvulnerable(bool on) { _invuln = on; }

    void Awake() {
        currentHealth = maxHealth;
        _cc = GetComponent<CharacterController>();
        _controller = GetComponent("PlayerController") as MonoBehaviour;
        if (fallbackSpawn != null) _checkpointPos = fallbackSpawn.position;
        else _checkpointPos = transform.position;
    }

    void Update() {
        // Skip safety-net kill if we're invuln, respawning, or already dead.
        if (IsInvuln) return;
        // Relative kill threshold — only fires if the player is way below their last checkpoint.
        if (transform.position.y < _checkpointPos.y - killBelowOffsetFromCheckpoint) {
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

        _invuln = true;
        if (_controller != null) _controller.enabled = true;
        yield return new WaitForSeconds(invulnSecondsAfterRespawn);
        _invuln = false;
        _respawning = false;
    }
}
