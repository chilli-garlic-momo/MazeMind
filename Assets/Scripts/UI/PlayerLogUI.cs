using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MazeMind.Core;

public class PlayerLogUI : MonoBehaviour {
    public TMP_Text[] lines = new TMP_Text[5]; // assign in inspector, top→bottom
    readonly Queue<string> _q = new();

    void Start() {
        DecisionLogger.I.OnNew += e => StartCoroutine(Push(e.playerLine));
    }
    IEnumerator Push(string s) {
        _q.Enqueue(s);
        while (_q.Count > 5) _q.Dequeue();
        var arr = _q.ToArray();
        for (int i = 0; i < lines.Length; i++) {
            lines[i].text = i < arr.Length ? "" : "";
            lines[i].alpha = Mathf.Lerp(1f, 0.4f, (float)(lines.Length-1-i)/(lines.Length-1));
        }
        // typewriter on the newest (last) line
        var target = arr[arr.Length-1];
        var lineUI = lines[arr.Length-1];
        for (int i = 1; i <= target.Length; i++) {
            lineUI.text = target.Substring(0, i);
            yield return new WaitForSeconds(0.04f);
        }
    }
}