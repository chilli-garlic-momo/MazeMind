// File: DacoitRoom2.cs
// Key-aware dacoit used by Room 1 Section 1.5 and Room 2.
using TMPro;
using UnityEngine;
using MazeMind.Core;

public class DacoitRoom2 : MonoBehaviour
{
    [Header("Runtime demand (set by Section15Director / Room2 manager)")]
    public int gemDemand;

    [Header("UI")]
    public TMP_Text demandLabel;

    [Header("No-key behaviour (Room 1 only)")]
    public Transform respawnIfNoKey;
    public string respawnSectionId = "1.1";
    public float respawnDelay = 1.5f;

    bool _paid;
    bool _respawningNoKey;

    void Awake()
    {
        GameManager.EnsureExists();
        AutoFindRespawnTargetIfNeeded();
    }

    void Start()
    {
        if (gemDemand <= 0) gemDemand = Room2DifficultyManager.GetDacoitDemand();
        RefreshLabel();
    }

    public void SetDemand(int d)
    {
        gemDemand = Mathf.Max(0, d);
        RefreshLabel();
    }

    void RefreshLabel()
    {
        if (gemDemand <= 0) SetDemandLabel("Show me the key.");
        else SetDemandLabel($"Bring me {gemDemand} gems.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (_paid || _respawningNoKey || !other.CompareTag("Player")) return;

        var gm = GameManager.EnsureExists();

        if (!gm.hasKey)
        {
            _respawningNoKey = true;
            SetDemandLabel("Go find the key first.");
            DecisionLogger.I?.Log("DacoitNoKey", "1.5", "DacoitBlocked",
                "Go find the key first.",
                "Player reached dacoit without the key. Respawning at Room 1 start.");
            StartCoroutine(RespawnToStart(other.gameObject));
            return;
        }

        int have = gm.gems;
        int shortBy = Mathf.Max(0, gemDemand - have);

        if (have >= gemDemand)
        {
            gm.SpendGems(gemDemand);
            _paid = true;
            SetDemandLabel("Accepted. Move along.");
            DecisionLogger.I?.Log("DacoitPaid", "1.5", "DacoitAccepted",
                "Smart. You listened.",
                $"Player paid {gemDemand} gems. Remaining: {gm.gems}");
            gameObject.SetActive(false);
        }
        else
        {
            SetDemandLabel($"Not enough.\nYou are {shortBy} short.");
            DecisionLogger.I?.Log("DacoitBlocked", "1.5", "DacoitDenied",
                $"You are {shortBy} short. The maze remembers.",
                $"Player has {have}, needs {gemDemand}. Blocked.");
        }
    }

    System.Collections.IEnumerator RespawnToStart(GameObject player)
    {
        yield return new WaitForSeconds(respawnDelay);

        AutoFindRespawnTargetIfNeeded(player);

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
            Debug.LogWarning("[DacoitRoom2] No respawn target found. Create Spawn_1_1 or assign respawnIfNoKey.");
        }

        _respawningNoKey = false;
        RefreshLabel();
    }

    void AutoFindRespawnTargetIfNeeded(GameObject player = null)
    {
        if (respawnIfNoKey != null) return;

        string[] names = { "Spawn_1_1", "Spawn_Run", "Room1Start", "StartSpawn" };
        foreach (var n in names)
        {
            var go = GameObject.Find(n);
            if (go != null)
            {
                respawnIfNoKey = go.transform;
                respawnSectionId = "1.1";
                return;
            }
        }

        if (player == null) player = GameObject.FindWithTag("Player");
        var hp = player != null ? player.GetComponent<PlayerHealth>() : null;
        if (hp != null && hp.fallbackSpawn != null)
        {
            respawnIfNoKey = hp.fallbackSpawn;
            respawnSectionId = hp.fallbackSectionId;
        }
    }

    void SetDemandLabel(string s)
    {
        if (demandLabel != null) demandLabel.text = s;
    }
}
