// File: DynamicSign.cs  
using System.Collections;
using TMPro;
using UnityEngine;
using MazeMind.Core;

public class DynamicSign : MonoBehaviour {
    public TMP_Text signText;

    readonly string[] _lines = {
        "WELCOME",
        "WE ARE WATCHING",
        "INTERESTING"
    };

    void Start() {
        if (signText != null) signText.text = _lines[0];
        StartCoroutine(Cycle());
    }

    IEnumerator Cycle() {
        yield return new WaitForSeconds(8f);
        SetLine(1);
        yield return new WaitForSeconds(10f);
        SetLine(2);
        DecisionLogger.I?.Log("Sign", "2.spawn", "SignChanged",
            "We are watching.", "Dynamic sign advanced to INTERESTING.");
    }

    void SetLine(int i) {
        if (signText != null) signText.text = _lines[i];
    }
}