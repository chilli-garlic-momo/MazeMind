using System.Collections; using UnityEngine; using MazeMind.Core;

public class ForcedFallSequence : MonoBehaviour {
    public Transform respawnAt_1_5;
    public AudioSource screamSfx;
    public CanvasGroup fadeToBlack;

    bool _firing;

    void OnTriggerEnter(Collider other) {
        if (_firing || !other.CompareTag("Player")) return;
        _firing = true;
        StartCoroutine(Run(other.transform));
    }

    IEnumerator Run(Transform player) {
        if (screamSfx != null) screamSfx.Play();

        float t = 0;
        while (t < 1.2f) {
            if (fadeToBlack != null) fadeToBlack.alpha = t / 1.2f;
            t += Time.deltaTime;
            yield return null;
        }

        if (respawnAt_1_5 != null) {
            player.position = respawnAt_1_5.position;
            player.rotation = respawnAt_1_5.rotation;
        }

        AIDirector.I?.Fire(TriggerKind.OnSectionExit,  "1.2", 1);
        AIDirector.I?.Fire(TriggerKind.OnSectionEnter, "1.5", 1);

        var hp = player.GetComponent<PlayerHealth>();
        if (hp != null && respawnAt_1_5 != null)
            hp.RegisterCheckpoint(respawnAt_1_5.position, "1.5");

        Section15Director.I?.StartGhostTimer(15f);

        t = 0;
        while (t < 1f) {
            if (fadeToBlack != null) fadeToBlack.alpha = 1f - t;
            t += Time.deltaTime;
            yield return null;
        }
        _firing = false;
    }
}
