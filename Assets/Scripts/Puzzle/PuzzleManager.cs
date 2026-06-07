// File: PuzzleManager.cs
using UnityEngine; using MazeMind.Core;

public class PuzzleManager : MonoBehaviour {
    public static PuzzleManager I { get; private set; }

    [Header("Switches that must all be ON to solve")]
    public SwitchInteractable[] switches;

    [Header("Gate that lowers / disables when puzzle is solved")]
    public GameObject gate;

    bool _solved;

    void Awake() {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
    }

    void Update() {
        if (_solved) return;
        if (AllOn()) Solve();
    }

    bool AllOn() {
        foreach (var sw in switches)
            if (sw == null || !sw.isOn) return false;
        return switches.Length > 0;
    }

    void Solve() {
        _solved = true;
        if (gate != null) gate.SetActive(false);
        DecisionLogger.I?.Log("PuzzleSolved", "2.puzzle", "SwitchPuzzle",
            "Hm. Faster than expected.",
            "All switches activated — gate lowered.");
        Debug.Log("PuzzleManager: puzzle solved, gate deactivated.");
    }

    public bool IsSolved => _solved;
}