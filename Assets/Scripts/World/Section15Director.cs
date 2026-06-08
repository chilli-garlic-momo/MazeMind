// Section15Director — Room 1 final chamber.
// No key here (key was collected in 1.3). Dacoit gates exit by gems.
// If player has no key, dacoit respawns them at the Room 1 start spawn.
// Exit door loads Room2.
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

    void Awake()
    {
        AutoWireReferences();
    }

    void Start()
    {
        AutoWireReferences();

        var gm = GameManager.EnsureExists();

        if (dacoit != null)
        {
            int demand = dacoit.gemDemand;
            if (AIDirector.I != null) demand = AIDirector.I.ComputeRoom1DacoitDemand();

            // Important for the prototype: do not make Room 1 impossible after the
            // player reaches 1.5. Demand can be 0 if they collected no gems.
            demand = Mathf.Clamp(demand, 0, Mathf.Max(0, gm.gems));
            dacoit.SetDemand(demand);

            DecisionLogger.I?.Log("DacoitDemandSet", "1.5", "AIDirector",
                demand <= 0 ? "Show me the key." : $"Dacoit demands {demand} gems.",
                $"Room 1 dacoit demand set to {demand}; player currently has {gm.gems} gems.");

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
                if (go != null)
                {
                    spawn_1_1 = go.transform;
                    break;
                }
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
