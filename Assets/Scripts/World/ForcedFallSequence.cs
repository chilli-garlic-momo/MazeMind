using System.Collections;
using UnityEngine;
using MazeMind.Core;

/// <summary>
/// "Forced fall" cinematic. Can be invoked two ways:
///   1. As a trigger volume (legacy): player walks in -> Play() runs.
///   2. Programmatically: Section12Director calls Play() when the
///      player crosses the gap-3 fall trigger.
/// On completion: fade-to-black, teleport player to respawnAt_1_5,
/// fire AIDirector enter/exit events.
/// </summary>
public class ForcedFallSequence : MonoBehaviour {
    public Transform respawnAt_1_5;
    public AudioSource screamSfx;
    public CanvasGroup fadeToBlack;
    public float fadeOutSeconds = 1.2f;
    public float fadeInSeconds  = 1.0f;

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
        if (screamSfx != null) screamSfx.Play();

        float t = 0;
        while (t < fadeOutSeconds) {
            if (fadeToBlack != null) fadeToBlack.alpha = t / fadeOutSeconds;
            t += Time.deltaTime;
            yield return null;
        }
        if (fadeToBlack != null) fadeToBlack.alpha = 1f;

        if (respawnAt_1_5 != null) {
            // CharacterController fights direct .position writes — disable for the teleport.
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.position = respawnAt_1_5.position;
            player.rotation = respawnAt_1_5.rotation;
            if (cc != null) cc.enabled = true;
        }

        AIDirector.I?.Fire(TriggerKind.OnSectionExit,  "1.2", 1);
        AIDirector.I?.Fire(TriggerKind.OnSectionEnter, "1.5", 1);

        var hp = player.GetComponent<PlayerHealth>();
        if (hp != null && respawnAt_1_5 != null)
            hp.RegisterCheckpoint(respawnAt_1_5.position, "1.5");

        t = 0;
        while (t < fadeInSeconds) {
            if (fadeToBlack != null) fadeToBlack.alpha = 1f - (t / fadeInSeconds);
            t += Time.deltaTime;
            yield return null;
        }
        if (fadeToBlack != null) fadeToBlack.alpha = 0f;
        _firing = false;
    }
}
