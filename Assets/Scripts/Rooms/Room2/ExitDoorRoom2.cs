// File: ExitDoorRoom2.cs — REPLACE ENTIRE FILE
using System.Collections;
using UnityEngine;
using MazeMind.Core;

public class ExitDoorRoom2 : MonoBehaviour
{
    public DacoitRoom2 dacoit;
    public string nextSceneName = "Room3";

    [Header("Door swing — assign the SmallDoor child Transform")]
    public Transform smallDoor;          // the door leaf that rotates
    public float openAngle = 90f;        // Y degrees to rotate open
    public float openDuration = 0.5f;

    bool _opened;
    // Add this field to ExitDoorRoom2
    bool _hadDummyKey;
    public void NotifyDummyKeyCollected() => _hadDummyKey = true;
    public void TryOpen()
    {
        if (_opened) return;

        bool hasKey = GameManager.Instance.hasKey;
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
            Log($"You are {dacoit.gemDemand - GameManager.Instance.gems} short. The maze remembers.",
                "Player tried door — dacoit still blocking.");
            return;
        }

        _opened = true;
        AIDirector.I?.Fire(TriggerKind.OnSectionExit, "2.exit", 2);
        GameManager.Instance.hasKey = false;

        DecisionLogger.I?.Log("RoomComplete", "2.exit", "RoomEnd",
            "Profile updated. Adaptation complete. Preparing next room.",
            "Room 2 exit confirmed.");

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
    if (BetweenRoomManager.I != null) BetweenRoomManager.I.ShowScreen(nextSceneName);
    else UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
}

    void Log(string player, string dev) =>
        DecisionLogger.I?.Log("ExitAttempt", "2.exit", "DoorBlocked", player, dev);

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) TryOpen();
    }
}
