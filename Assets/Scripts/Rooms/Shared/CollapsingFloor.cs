// File: CollapsingFloor.cs
using System.Collections; using UnityEngine;

public class CollapsingFloor : MonoBehaviour {
    public float delaySeconds = 0.8f;
    bool _triggered;

    void OnTriggerEnter(Collider other) {
        if (_triggered || !other.CompareTag("Player")) return;
        _triggered = true;
        StartCoroutine(Collapse());
    }

    IEnumerator Collapse() {
        // Optional: visual shake here
        yield return new WaitForSeconds(delaySeconds);
        gameObject.SetActive(false);
    }
}