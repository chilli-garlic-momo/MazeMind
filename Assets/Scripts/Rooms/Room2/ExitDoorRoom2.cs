// File: ExitDoorRoom2.cs
using System.Collections;
using UnityEngine;
using MazeMind.Core;

public class ExitDoorRoom2 : MonoBehaviour
{
    public DacoitRoom2 dacoit;
    public string nextSceneName = "Room3";

    [Header("Door swing — assign the SmallDoor child Transform if you have one")]
    public Transform smallDoor;
    public float openAngle = 90f;
    public float openDuration = 0.5f;

    bool _opened;
    bool _hadDummyKey;

    void Awake()
    {
        GameManager.EnsureExists();
        if (dacoit == null)
        {
#pragma warning disable 0618
            dacoit = FindObjectOfType<DacoitRoom2>();
#pragma warning restore 0618
        }
    }

    public void NotifyDummyKeyCollected() => _hadDummyKey = true;

    public void TryOpen()
    {
        if (_opened) return;

        var gm = GameManager.EnsureExists();
        bool hasKey = gm.hasKey;
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
            Debug.Log("[ExitDoorRoom2] Blocked: no key.");
            return;
        }

        if (!dacoitGone)
        {
            int shortBy = dacoit != null ? Mathf.Max(0, dacoit.gemDemand - gm.gems) : 0;
            Log($"You are {shortBy} short. The maze remembers.",
                "Player tried door — dacoit still blocking.");
            Debug.Log("[ExitDoorRoom2] Blocked: dacoit has not accepted payment yet.");
            return;
        }

        _opened = true;
        AIDirector.I?.Fire(TriggerKind.OnSectionExit, "2.exit", 2);

        DecisionLogger.I?.Log("RoomComplete", "2.exit", "RoomEnd",
            "Profile updated. Adaptation complete. Preparing next room.",
            $"Exit confirmed. Loading {nextSceneName}.");

        gm.ResetForNextRoom();

        if (smallDoor != null) StartCoroutine(SwingOpen());
        else LoadNext();
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

    void LoadNext()
    {
        if (string.IsNullOrEmpty(nextSceneName)) return;

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
