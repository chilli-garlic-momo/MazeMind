using System.Collections;
using UnityEngine;
using MazeMind.Core;

/// <summary>
/// "Forced fall" cinematic for Section 1.2 -> 1.5.
/// v13: now guards the player with PlayerHealth.SetInvulnerable() for the
/// entire fall + a grace window after the teleport, so the safety-net
/// killBelowY check in PlayerHealth can't kill the player mid-fall and
/// race-respawn them at the old (1.1) checkpoint.
/// </summary>
public class ForcedFallSequence : MonoBehaviour {
    public Transform respawnAt_1_5;
    public AudioSource screamSfx;
    public CanvasGroup fadeToBlack;
    public float fadeOutSeconds = 1.2f;
    public float fadeInSeconds  = 1.0f;
    [Tooltip("Extra invulnerability after teleport so PlayerHealth doesn't kill the player on landing in 1.5.")]
    public float postTeleportInvulnSeconds = 1.5f;

    bool _firing;

    public void Play() {
        if (_firing) return;
        var player = GameObject.FindWithTag("Player");
        if (player == null) {
            Debug.LogWarning("[ForcedFallSequence] No Player found.");
            return;
        }
        _firing = true;
        StartCoroutine(Run(player.transform));
    }

    void OnTriggerEnter(Collider other) {
        if (_firing || !other.CompareTag("Player")) return;
        _firing = true;
        StartCoroutine(Run(other.transform));
    }

    IEnumerator Run(Transform player) {
        // if (screamSfx == null || fadeToBlack == null) {
        //     Debug.LogError("[ForcedFallSequence] Missing refs");
        //     yield break;
        // }
        var hp = player.GetComponent<PlayerHealth>();

        // Suppress safety-net killBelowY while the player is mid-cinematic.
        if (hp != null) hp.SetInvulnerable(true);

        if (screamSfx != null) screamSfx.Play();

        float t = 0;
        while (t < fadeOutSeconds) {
            if (fadeToBlack != null) fadeToBlack.alpha = t / fadeOutSeconds;
            t += Time.deltaTime;
            yield return null;
        }
        if (fadeToBlack != null) fadeToBlack.alpha = 1f;

        if (respawnAt_1_5 != null) {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.position = respawnAt_1_5.position;
            player.rotation = respawnAt_1_5.rotation;
            if (cc != null) cc.enabled = true;
        }

        AIDirector.I?.Fire(TriggerKind.OnSectionExit,  "1.2", 1);
        AIDirector.I?.Fire(TriggerKind.OnSectionEnter, "1.5", 1);

        // Register 1.5 checkpoint BEFORE clearing invuln so the relative
        // killBelowY check (now in PlayerHealth) uses the new low Y as its baseline.
        if (hp != null && respawnAt_1_5 != null)
            hp.RegisterCheckpoint(respawnAt_1_5.position, "1.5");

        t = 0;
        while (t < fadeInSeconds) {
            if (fadeToBlack != null) fadeToBlack.alpha = 1f - (t / fadeInSeconds);
            t += Time.deltaTime;
            yield return null;
        }
        if (fadeToBlack != null) fadeToBlack.alpha = 0f;

        // Grace period before re-enabling the safety net.
        if (postTeleportInvulnSeconds > 0f)
            yield return new WaitForSeconds(postTeleportInvulnSeconds);
        if (hp != null) hp.SetInvulnerable(false);

        _firing = false;
    }
}
