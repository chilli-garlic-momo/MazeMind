// File: DacoitRoom2.cs
// Now key-aware: if player arrives without the key, dacoit speaks "go find the key"
// and the player is respawned to the Section 1.1 spawn point (assigned via Section15Director).
using TMPro;
using UnityEngine;
using MazeMind.Core;

public class DacoitRoom2 : MonoBehaviour
{
    [Header("Runtime demand (set by Section15Director from AIDirector)")]
    public int gemDemand;

    [Header("UI")]
    public TMP_Text demandLabel;   // world-space TMP floating above dacoit

    [Header("No-key behaviour (set by Section15Director)")]
    public Transform respawnIfNoKey;       // Spawn_1_1 transform
    public string    respawnSectionId = "1.1";
    public float     respawnDelay = 1.5f;

    bool _paid;
    bool _respawningNoKey;

    void Start()
    {
        // If anyone else set demand already, keep it. Otherwise fall back.
        if (gemDemand <= 0) gemDemand = Room2DifficultyManager.GetDacoitDemand();
        RefreshLabel();
    }

    public void SetDemand(int d) { gemDemand = d; RefreshLabel(); }

    void RefreshLabel() => SetDemandLabel($"Bring me {gemDemand} gems.");

    void OnTriggerEnter(Collider other)
    {
        if (_paid || _respawningNoKey || !other.CompareTag("Player")) return;

        // 1. No key? Send them back to 1.1.
        if (GameManager.Instance != null && !GameManager.Instance.hasKey)
        {
            _respawningNoKey = true;
            SetDemandLabel("Go find the key first.");
            DecisionLogger.I?.Log("DacoitNoKey", "1.5", "DacoitBlocked",
                "Go find the key first.",
                "Player reached dacoit without the key. Respawning at 1.1.");
            StartCoroutine(RespawnToStart(other.gameObject));
            return;
        }

        // 2. Has key — check gems.
        int have = GameManager.Instance != null ? GameManager.Instance.gems : 0;
        int short_ = gemDemand - have;

        if (have >= gemDemand)
        {
            GameManager.Instance.gems -= gemDemand;
            _paid = true;
            SetDemandLabel("Accepted. Move along.");
            DecisionLogger.I?.Log("DacoitPaid", "1.5", "DacoitAccepted",
                "Smart. You listened.",
                $"Player paid {gemDemand} gems. Remaining: {GameManager.Instance.gems}");
            gameObject.SetActive(false);
        }
        else
        {
            SetDemandLabel($"Not enough.\nYou are {short_} short.");
            DecisionLogger.I?.Log("DacoitBlocked", "1.5", "DacoitDenied",
                $"You are {short_} short. The maze remembers.",
                $"Player has {have}, needs {gemDemand}. Blocked.");
        }
    }

    System.Collections.IEnumerator RespawnToStart(GameObject player)
    {
        yield return new WaitForSeconds(respawnDelay);

        var hp = player.GetComponent<PlayerHealth>();
        var cc = player.GetComponent<CharacterController>();

        if (respawnIfNoKey != null)
        {
            if (hp != null) hp.RegisterCheckpoint(respawnIfNoKey.position, respawnSectionId);
            if (cc != null) cc.enabled = false;
            player.transform.position = respawnIfNoKey.position;
            player.transform.rotation = respawnIfNoKey.rotation;
            if (cc != null) cc.enabled = true;
        }
        else
        {
            Debug.LogWarning("[DacoitRoom2] respawnIfNoKey not assigned — cannot send player to 1.1.");
        }

        _respawningNoKey = false;
        RefreshLabel();
    }

    void SetDemandLabel(string s)
    {
        if (demandLabel != null) demandLabel.text = s;
    }
}
