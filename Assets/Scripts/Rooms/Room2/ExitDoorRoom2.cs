// File: ExitDoorRoom2.cs — v11
// Key change: the door has a SOLID wall (solidBlocker) that physically stops
// the player from walking through until the door actually opens. Without this,
// the player would walk into the trigger, fail the open conditions (no key /
// dacoit still blocking), and then *keep walking* through the door cube into
// the empty space behind it — falling and getting killBelowY'd. v10 had no
// physical barrier, only a trigger.
//
// Setup in Unity (Section_1_5 scene):
//   1. Make the ExitDoor a small GameObject with this script.
//   2. Add a Box Collider on it — Is Trigger = ON (proximity sensor).
//   3. Add a CHILD GameObject called "DoorBlocker" with a Box Collider
//      (Is Trigger = OFF) sized to fill the doorway opening. Drag that
//      child's Collider/GameObject into the solidBlocker slot.
//   4. (Optional) Add a child "SmallDoor" mesh and drag into smallDoor.
//   5. nextSceneName = "Room2".
//
// While locked: blocker is enabled — player physically can't pass.
// When TryOpen() succeeds: blocker disables, door swings, scene loads.

using System.Collections;
using UnityEngine;
using MazeMind.Core;

public class ExitDoorRoom2 : MonoBehaviour
{
    public DacoitRoom2 dacoit;
    public string nextSceneName = "Room3";

    [Header("Solid blocker (v11) — REQUIRED. Stops player walking through locked door.")]
    [Tooltip("A child GameObject with a non-trigger collider that fills the doorway.")]
    public GameObject solidBlocker;

    [Header("Door swing — assign the SmallDoor child Transform")]
    public Transform smallDoor;
    public float openAngle = 90f;
    public float openDuration = 0.5f;

    bool _opened;
    bool _hadDummyKey;
    public void NotifyDummyKeyCollected() => _hadDummyKey = true;

    void Awake() {
        // Always start locked: blocker on.
        if (solidBlocker != null) solidBlocker.SetActive(true);
        else Debug.LogWarning("[ExitDoorRoom2] solidBlocker not assigned — player will walk through the door if conditions fail.");
    }

    public void TryOpen()
    {
        if (_opened) return;

        bool hasKey = GameManager.Instance != null && GameManager.Instance.hasKey;
        bool dacoitGone = dacoit == null || !dacoit.gameObject.activeSelf;

        if (!hasKey)
        {
            string msg = _hadDummyKey
                ? "Wrong key. Return to the spawn room. The real key is waiting."
                : "Something is missing.";
            string dev = _hadDummyKey
                ? "Player has dummy key, tried exit — redirected to spawn room."
                : "Player has no key.";
            Log(msg, dev);
            return;
        }
        if (!dacoitGone)
        {
            int demand = dacoit.gemDemand;
            int have = GameManager.Instance != null ? GameManager.Instance.gems : 0;
            Log($"You are {Mathf.Max(0, demand - have)} short. The maze remembers.",
                "Player tried door — dacoit still blocking.");
            return;
        }

        // PASSED — open the door for real.
        _opened = true;
        if (solidBlocker != null) solidBlocker.SetActive(false);

        AIDirector.I?.Fire(TriggerKind.OnSectionExit, "1.5", 1);
        if (GameManager.Instance != null) {
            GameManager.Instance.hasKey = false;
            GameManager.Instance.ResetForNextRoom();
        }

        DecisionLogger.I?.Log("RoomComplete", "1.exit", "RoomEnd",
            "Profile updated. Adaptation complete. Preparing next room.",
            $"Room 1 exit confirmed. Loading {nextSceneName}.");

        if (smallDoor != null)
            StartCoroutine(SwingOpen());
        else
            LoadNext();
    }

    IEnumerator SwingOpen()
    {
        Quaternion from = smallDoor.localRotation;
        Quaternion to = Quaternion.Euler(0f, openAngle, 0f);
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

    void LoadNext() {
        // Freeze the player so they don't wander off the floor during the transition.
        var player = GameObject.FindWithTag("Player");
        if (player != null) {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            var pc = player.GetComponent("PlayerController") as MonoBehaviour;
            if (pc != null) pc.enabled = false;
        }

        if (BetweenRoomManager.I != null) BetweenRoomManager.I.ShowScreen(nextSceneName);
        else UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }

    void Log(string player, string dev) =>
        DecisionLogger.I?.Log("ExitAttempt", "1.exit", "DoorBlocked", player, dev);

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) TryOpen();
    }

    void OnTriggerStay(Collider other)
    {
        // Stay handles: player presses against locked door, then later collects
        // the key / pays dacoit — next physics tick re-evaluates and opens.
        if (!_opened && other.CompareTag("Player")) TryOpen();
    }
}
