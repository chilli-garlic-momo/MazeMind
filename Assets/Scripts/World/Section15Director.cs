// Section15Director — Room 1 final chamber.
// Only overrides the player's respawn checkpoint to Spawn_1_1 WHILE the player
// is physically inside Section 1.5 without the key. This prevents the no-key
// death loop in 1.5 without breaking other sections (1.3 etc.).
using UnityEngine;
using MazeMind.Core;

public class Section15Director : MonoBehaviour
{
    [Header("Refs (optional — auto-found if empty)")]
    public DacoitRoom2 dacoit;
    public ExitDoorRoom2 exitDoor;

    [Header("No-key respawn target (Room 1 start)")]
    public Transform spawn_1_1;

    [Header("Scene to load when dacoit is paid")]
    public string nextSceneName = "Room2";

    [Header("Bounds of Section 1.5 (auto-built from this object's BoxCollider if left null)")]
    public Collider sectionBounds;

    [Tooltip("If true, will create a large trigger BoxCollider on this GameObject when none is found.")]
    public bool autoCreateBounds = true;

    PlayerHealth _playerHp;
    Transform _playerT;
    bool _playerInside;

    void Awake() { AutoWireReferences(); EnsureBounds(); }

    void Start()
    {
        AutoWireReferences();
        FindPlayer();

        var gm = GameManager.EnsureExists();

        if (dacoit != null)
        {
            int demand = dacoit.gemDemand;
            if (AIDirector.I != null) demand = AIDirector.I.ComputeRoom1DacoitDemand();
            demand = Mathf.Clamp(demand, 0, Mathf.Max(0, gm.gems));
            dacoit.SetDemand(demand);

            DecisionLogger.I?.Log("DacoitDemandSet", "1.5", "AIDirector",
                demand <= 0 ? "Show me the key." : $"Dacoit demands {demand} gems.",
                $"Room 1 dacoit demand set to {demand}; player has {gm.gems} gems.");

            if (spawn_1_1 != null)
            {
                dacoit.respawnIfNoKey = spawn_1_1;
                dacoit.respawnSectionId = "1.1";
            }
        }

        if (exitDoor != null)
        {
            exitDoor.dacoit = dacoit;
            if (!string.IsNullOrEmpty(nextSceneName)) exitDoor.nextSceneName = nextSceneName;
        }
    }

    Vector3 _entryPos;
    bool _entryPosValid;
    bool _bouncing;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = true;
        if (_playerHp == null) FindPlayer();

        // Register a LOCAL checkpoint at the section entry so the PlayerHealth
        // safety-net measures from inside 1.5 (not from Spawn_1_1 above).
        _entryPos = other.transform.position;
        _entryPosValid = true;
        if (_playerHp != null)
        {
            _playerHp.RegisterCheckpoint(_entryPos, "1.5");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
    }

    void Update()
    {
        // If player is inside 1.5 without the full dacoit criteria (real key
        // AND enough gems) and starts falling off an edge, bounce them back to
        // Spawn_1_1 immediately. If criteria are met, never bounce/respawn.
        if (!_playerInside || _bouncing || _playerT == null || spawn_1_1 == null) return;
        if (!_entryPosValid) return;

        var gm = GameManager.Instance;
        int demand = dacoit != null ? dacoit.gemDemand : 0;
        bool dacoitCleared = dacoit == null || !dacoit.gameObject.activeSelf;
        if (dacoitCleared) return; // already paid/cleared → never bounce
        if (gm != null && gm.hasKey && gm.gems >= demand) return; // criteria met → normal play

        if (_playerT.position.y < _entryPos.y - 5f)
        {
            StartCoroutine(BounceToStart());
        }
    }

    System.Collections.IEnumerator BounceToStart()
    {
        _bouncing = true;
        var player = _playerT.gameObject;
        var hp = _playerHp;
        var cc = player.GetComponent<CharacterController>();
        var controller = player.GetComponent("PlayerController") as MonoBehaviour;

        if (hp != null) hp.SetInvulnerable(true);
        if (controller != null) controller.enabled = false;
        if (cc != null) cc.enabled = false;

        player.transform.position = spawn_1_1.position;
        player.transform.rotation = spawn_1_1.rotation;
        if (hp != null) hp.RegisterCheckpoint(spawn_1_1.position, "1.1");

        DecisionLogger.I?.Log("Section15FallBounce", "1.5", "NoKeyFall",
            "The maze sends you back. Bring the key and enough gems.",
            "Player fell in Section 1.5 without full dacoit criteria; bounced to Spawn_1_1.");

        yield return new WaitForSeconds(0.4f);

        if (cc != null) cc.enabled = true;
        if (controller != null) controller.enabled = true;
        if (hp != null) hp.SetInvulnerable(false);

        _bouncing = false;
    }



    void FindPlayer()
    {
        var go = GameObject.FindWithTag("Player");
        if (go == null) return;
        _playerT = go.transform;
        _playerHp = go.GetComponent<PlayerHealth>();
    }

    void EnsureBounds()
    {
        if (sectionBounds == null) sectionBounds = GetComponent<Collider>();
        if (sectionBounds == null && autoCreateBounds)
        {
            var box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(40, 20, 40); // generous — adjust in Inspector if 1.5 is bigger
            sectionBounds = box;
        }
        if (sectionBounds != null) sectionBounds.isTrigger = true;
    }

    void AutoWireReferences()
    {
#pragma warning disable 0618
        if (dacoit == null) dacoit = FindObjectOfType<DacoitRoom2>();
        if (exitDoor == null) exitDoor = FindObjectOfType<ExitDoorRoom2>();
#pragma warning restore 0618

        if (spawn_1_1 == null)
        {
            string[] names = { "Spawn_1_1", "Spawn_Run", "Room1Start", "StartSpawn" };
            foreach (var n in names)
            {
                var go = GameObject.Find(n);
                if (go != null) { spawn_1_1 = go.transform; break; }
            }
        }

        if (spawn_1_1 == null)
        {
            var player = GameObject.FindWithTag("Player");
            var hp = player != null ? player.GetComponent<PlayerHealth>() : null;
            if (hp != null && hp.fallbackSpawn != null) spawn_1_1 = hp.fallbackSpawn;
        }
    }
}
