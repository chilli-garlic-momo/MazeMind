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

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = true;
        if (_playerHp == null) FindPlayer();

        // Register a LOCAL checkpoint at the section entry so the PlayerHealth
        // safety-net (kill-below-checkpoint-Y) measures from inside 1.5 rather
        // than from Spawn_1_1, which is far above and would insta-kill the
        // player on entry. The Dacoit handles the actual "no key -> bounce
        // back to Spawn_1_1" behaviour on contact; we should NOT continuously
        // re-bind the checkpoint to Spawn_1_1 here.
        if (_playerHp != null)
        {
            _playerHp.RegisterCheckpoint(other.transform.position, "1.5");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
    }

    void Update()
    {
        // Intentionally empty. Earlier versions re-bound the checkpoint to
        // Spawn_1_1 every frame while inside 1.5, which made the safety-net
        // kill fire instantly because 1.5 sits well below Spawn_1_1.Y.
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
