using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MazeMind.Core;

public class PlayerLogUI : MonoBehaviour {
    public TMP_Text[] lines = new TMP_Text[5]; // assign in inspector, top→bottom
    readonly Queue<string> _q = new();

    void Start() {
        if (DecisionLogger.I == null) {
            Debug.LogWarning("PlayerLogUI: DecisionLogger.I is null — Boot scene not loaded?");
            return;
        }
        DecisionLogger.I.OnNew += e => StartCoroutine(Push(e.playerLine));
    }

    IEnumerator Push(string s) {
        if (string.IsNullOrEmpty(s)) yield break;
        _q.Enqueue(s);
        while (_q.Count > lines.Length) _q.Dequeue();
        var arr = _q.ToArray();

        // Fill all slots (oldest at top, newest at bottom), older = fainter
        for (int i = 0; i < lines.Length; i++) {
            if (lines[i] == null) continue;
            lines[i].text  = i < arr.Length ? arr[i] : "";
            lines[i].alpha = Mathf.Lerp(0.4f, 1f, arr.Length > 1 ? (float)i / (arr.Length - 1) : 1f);
        }

        // Typewriter on the newest line
        int newestIdx = arr.Length - 1;
        var lineUI = lines[newestIdx];
        var target = arr[newestIdx];
        if (lineUI == null) yield break;
        for (int i = 1; i <= target.Length; i++) {
            lineUI.text = target.Substring(0, i);
            yield return new WaitForSeconds(0.04f);
        }
    }
}
