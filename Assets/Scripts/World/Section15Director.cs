using UnityEngine;
using MazeMind.Core;
public class Section15Director : MonoBehaviour {

    [Header("Refs")]
    public DacoitRoom2 dacoit;
    public GameObject  keyObject;         // KeyPickup
    public ExitDoorRoom2 exitDoor;        // door to next scene

    void Start() {
        if (AIDirector.I != null && dacoit != null) {
            int demand = AIDirector.I.ComputeRoom1DacoitDemand();
            dacoit.SetDemand(demand);
        }

        if (keyObject != null && !keyObject.activeSelf) keyObject.SetActive(true);
    }
}
