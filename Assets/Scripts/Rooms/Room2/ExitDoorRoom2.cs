// File: ExitDoorRoom2.cs — v12 (hardened: proximity + heavy logging + scene fallback)
// Replace the entire file in Assets/Scripts/Rooms/Room2/ExitDoorRoom2.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MazeMind.Core;

public class ExitDoorRoom2 : MonoBehaviour
{
    [Header("Refs")]
    public DacoitRoom2 dacoit;
    public string nextSceneName = "Room2";

    [Header("Door swing — assign the SmallDoor child Transform (optional)")]
    public Transform smallDoor;
    public float openAngle = 90f;
    public float openDuration = 0.5f;

    [Header("Auto-open proximity (works even if no trigger collider is set up)")]
    public float autoOpenRadius = 2.5f;
    public LayerMask playerMask = ~0;   // default: everything; we still filter by tag

    [Header("Debug")]
    public bool verboseLogs = true;
    public KeyCode forceOpenKey = KeyCode.F2; // press to force-open while testing

    bool _opened;
    bool _hadDummyKey;
    float _nextProbeTime;

    void Awake()
    {
        AutoFindDacoitIfNeeded();
    }

    void Start()
    {
        AutoFindDacoitIfNeeded();
    }

    public void NotifyDummyKeyCollected() => _hadDummyKey = true;

    void Update()
    {
        if (_opened) return;

        // Debug override — useful while wiring the scene
        if (Input.GetKeyDown(forceOpenKey))
        {
            Debug.LogWarning("[ExitDoorRoom2] Force-open key pressed.");
            ForceOpen();
            return;
        }

        // Proximity probe (cheap, every 0.15s) — covers cases where the
        // BoxCollider isn't a trigger or the Player tag isn't perfect.
        if (Time.time < _nextProbeTime) return;
        _nextProbeTime = Time.time + 0.15f;

        Collider[] hits = Physics.OverlapSphere(transform.position, autoOpenRadius, playerMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].CompareTag("Player"))
            {
                if (verboseLogs) Debug.Log("[ExitDoorRoom2] Player in range — TryOpen()");
                TryOpen(hits[i].gameObject);
                return;
            }
        }
    }

    public void TryOpen(GameObject player = null)
    {
        if (_opened) return;

        GameManager.EnsureExists();
        bool hasKey      = GameManager.Instance != null && GameManager.Instance.hasKey;
        AutoFindDacoitIfNeeded();
        bool dacoitGone  = dacoit == null || dacoit.IsCleared;

        if (verboseLogs)
            Debug.Log($"[ExitDoorRoom2] TryOpen: hasKey={hasKey}, dacoitGone={dacoitGone}, " +
                      $"dacoit={(dacoit==null?"null":dacoit.name)}, nextScene='{nextSceneName}'");

        if (!dacoitGone && dacoit != null)
        {
            if (player == null)
            {
                var foundPlayer = GameObject.FindWithTag("Player");
                if (foundPlayer != null) player = foundPlayer;
            }

            dacoitGone = dacoit.TryResolveForPlayer(player, "ExitDoor");
            if (dacoitGone && verboseLogs)
                Debug.Log("[ExitDoorRoom2] Dacoit accepted payment during door check — opening exit.");
        }

        if (!hasKey)
        {
            string msg = _hadDummyKey
                ? "Wrong key. Return to the spawn room. The real key is waiting."
                : "Something is missing.";
            Log(msg, _hadDummyKey ? "Dummy key only." : "No key.");
            return;
        }
        if (!dacoitGone)
        {
            int demand = dacoit.gemDemand;
            int have   = GameManager.Instance != null ? GameManager.Instance.gems : 0;
            Log($"You are {Mathf.Max(0, demand - have)} short. The maze sends you back.",
                "Dacoit still blocking.");
            return;
        }

        OpenAndLoad();
    }

    public void ForceOpen()
    {
        if (_opened) return;
        OpenAndLoad();
    }

    void OpenAndLoad()
    {
        _opened = true;
        AIDirector.I?.Fire(TriggerKind.OnSectionExit, "1.exit", 1);
        if (GameManager.Instance != null) GameManager.Instance.hasKey = false;

        DecisionLogger.I?.Log("RoomComplete", "1.exit", "RoomEnd",
            "Profile updated. Adaptation complete. Preparing next room.",
            "Room 1 exit confirmed.");

        if (smallDoor != null) StartCoroutine(SwingOpen());
        else LoadNext();
    }

    IEnumerator SwingOpen()
    {
        Quaternion from = smallDoor.localRotation;
        Quaternion to   = from * Quaternion.Euler(0f, openAngle, 0f);
        float t = 0f;
        while (t < openDuration)
        {
            t += Time.deltaTime;
            smallDoor.localRotation = Quaternion.Slerp(from, to, t / openDuration);
            yield return null;
        }
        smallDoor.localRotation = to;
        yield return new WaitForSeconds(0.3f);
        LoadNext();
    }

    void LoadNext()
    {
        if (GameManager.Instance != null) GameManager.Instance.ResetForNextRoom();

        string scene = string.IsNullOrEmpty(nextSceneName) ? "Room2" : nextSceneName;

        // Verify scene is in Build Settings (common setup bug)
        if (!Application.CanStreamedLevelBeLoaded(scene))
        {
            Debug.LogError($"[ExitDoorRoom2] Scene '{scene}' is NOT in Build Settings. " +
                           "Open File > Build Settings and add Assets/Scenes/" + scene + ".unity.");
            // try common alternates
            string[] alts = { "Room2", "Room_2", "Room 2", "Scenes/Room2" };
            foreach (var a in alts)
            {
                if (Application.CanStreamedLevelBeLoaded(a))
                {
                    Debug.LogWarning($"[ExitDoorRoom2] Falling back to '{a}'.");
                    scene = a; break;
                }
            }
        }

        if (verboseLogs) Debug.Log($"[ExitDoorRoom2] Loading scene '{scene}'");

        if (BetweenRoomManager.I != null) BetweenRoomManager.I.ShowScreen(scene);
        else SceneManager.LoadScene(scene);
    }

    void Log(string player, string dev)
    {
        if (verboseLogs) Debug.Log($"[ExitDoorRoom2] Blocked: {dev} | msg=\"{player}\"");
        DecisionLogger.I?.Log("ExitAttempt", "1.exit", "DoorBlocked", player, dev);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (verboseLogs) Debug.Log("[ExitDoorRoom2] OnTriggerEnter by Player");
            TryOpen(other.gameObject);
        }
    }

    void AutoFindDacoitIfNeeded()
    {
        if (dacoit != null) return;
#pragma warning disable 0618
        dacoit = FindObjectOfType<DacoitRoom2>();
#pragma warning restore 0618
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, autoOpenRadius);
    }
}
