// File: PlayerInteraction.cs
using UnityEngine;

public class PlayerInteraction : MonoBehaviour {
    public float interactDistance = 3f;

    void Update() {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance)) return;

        Debug.Log("Interact hit: " + hit.collider.name);

        // Generic Interactable (switches, levers, etc.)
        var interactable = hit.collider.GetComponent<Interactable>();
        if (interactable != null) { interactable.Interact(); return; }

        // Room 1 door
        var door1 = hit.collider.GetComponent<DoorInteractable>();
        if (door1 != null) { door1.TryOpenDoor(); return; }

        // Room 2 exit door
        var door2 = hit.collider.GetComponent<ExitDoorRoom2>();
        if (door2 != null) { door2.TryOpen(); return; }
    }
}