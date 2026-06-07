// File: DacoitRoom2.cs
using TMPro; using UnityEngine; using MazeMind.Core;

public class DacoitRoom2 : MonoBehaviour {
    [Header("Runtime demand (overridden by Room2AdaptationSpawner)")]
    public int gemDemand = 3;

    [Header("UI")]
    public TMP_Text demandLabel;   // world-space TMP floating above dacoit

    bool _paid;

    void Start() => RefreshLabel();

    public void SetDemand(int d) {
        gemDemand = d;
        RefreshLabel();
    }

    void RefreshLabel() {
        if (demandLabel != null)
            demandLabel.text = $"Pay {gemDemand} gems";
    }

    void OnTriggerEnter(Collider other) {
        if (_paid || !other.CompareTag("Player")) return;

        int have = GameManager.Instance.gems;

        if (have >= gemDemand) {
            GameManager.Instance.gems -= gemDemand;
            _paid = true;
            DecisionLogger.I?.Log("DacoitPaid", "2.exit", "DacoitAccepted",
                "Smart. You listened.",
                $"Player paid dacoit {gemDemand} gems. Remaining: {GameManager.Instance.gems}");
            RefreshLabel();
            // Optionally animate dacoit stepping aside here
            gameObject.SetActive(false);
        } else {
            int short_ = gemDemand - have;
            DecisionLogger.I?.Log("DacoitBlocked", "2.exit", "DacoitDenied",
                $"You are {short_} short. The maze remembers.",
                $"Player tried to pay dacoit. Has {have}, needs {gemDemand}. Blocked.");
        }
    }
}