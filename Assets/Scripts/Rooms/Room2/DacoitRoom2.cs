// File: DacoitRoom2.cs
using TMPro;
using UnityEngine;
using MazeMind.Core;

public class DacoitRoom2 : MonoBehaviour
{
    [Header("Runtime demand (overridden by Room2AdaptationSpawner)")]
    public int gemDemand;

    [Header("UI")]
    public TMP_Text demandLabel;   // world-space TMP floating above dacoit

    bool _paid;

    void Start()
    {
        gemDemand = Room2DifficultyManager.GetDacoitDemand();
        RefreshLabel();
    }

    public void SetDemand(int d)
    {
        gemDemand = d;
        RefreshLabel();
    }

    void RefreshLabel() => SetDemandLabel($"Bring me {gemDemand} gems.");



    void OnTriggerEnter(Collider other)
    {
        if (_paid || !other.CompareTag("Player")) return;

        int have = GameManager.Instance.gems;
        int short_ = gemDemand - have;

        if (have >= gemDemand)
        {
            GameManager.Instance.gems -= gemDemand;
            _paid = true;
            SetDemandLabel("Accepted. Move along.");
            DecisionLogger.I?.Log("DacoitPaid", "2.exit", "DacoitAccepted",
                "Smart. You listened.",
                $"Player paid {gemDemand} gems. Remaining: {GameManager.Instance.gems}");
            gameObject.SetActive(false);
        }
        else
        {
            SetDemandLabel($"Not enough.\nCome back richer.");
            DecisionLogger.I?.Log("DacoitBlocked", "2.exit", "DacoitDenied",
                $"You are {short_} short. The maze remembers.",
                $"Player has {have}, needs {gemDemand}. Blocked.");
        }
    }

    void SetDemandLabel(string s)
    {
        if (demandLabel != null) demandLabel.text = s;
    }
}