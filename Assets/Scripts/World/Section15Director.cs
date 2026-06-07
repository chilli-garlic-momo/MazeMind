// Section15Director — Room 1 final chamber.
// No key here (key was collected in 1.3). Dacoit gates exit by gems.
// If player has no key, dacoit respawns them at Section 1.1 spawn.
// Exit door loads Room 2 (NOT MainMenu).
using UnityEngine;
using MazeMind.Core;

public class Section15Director : MonoBehaviour
{
    [Header("Refs")]
    public DacoitRoom2  dacoit;
    public ExitDoorRoom2 exitDoor;

    [Header("No-key respawn target (Section 1.1 spawn point)")]
    public Transform spawn_1_1;

    [Header("Scene to load when dacoit is paid")]
    public string nextSceneName = "Room2";

    void Start()
    {
        if (AIDirector.I != null && dacoit != null)
        {
            int demand = AIDirector.I.ComputeRoom1DacoitDemand();
            dacoit.SetDemand(demand);
            DecisionLogger.I?.Log("DacoitDemandSet", "1.5", "AIDirector",
                $"Dacoit demands {demand} gems.",
                $"AIDirector chose demand={demand} based on player performance.");
        }

        if (dacoit != null && spawn_1_1 != null)
        {
            dacoit.respawnIfNoKey = spawn_1_1;
            dacoit.respawnSectionId = "1.1";
        }

        if (exitDoor != null && !string.IsNullOrEmpty(nextSceneName))
            exitDoor.nextSceneName = nextSceneName;
    }
}
