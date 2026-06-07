// File: ExitDoorRoom2.cs
using UnityEngine; using MazeMind.Core;

public class ExitDoorRoom2 : MonoBehaviour {
    public DacoitRoom2 dacoit;
    public string nextSceneName = "MainMenu";

    bool _opened;

    public void TryOpen() {
        if (_opened) return;

        if (!GameManager.Instance.hasKey) {
            DecisionLogger.I?.Log("ExitAttempt", "2.exit", "NoKey",
                "Something is missing.", "Player tried door without key.");
            Debug.Log("Room 2 Exit: need key");
            return;
        }

        // Dacoit must have been paid (it deactivates itself when paid)
        if (dacoit != null && dacoit.gameObject.activeSelf) {
            Debug.Log("Room 2 Exit: dacoit still blocking");
            return;
        }

        _opened = true;
        AIDirector.I?.Fire(TriggerKind.OnSectionExit, "2.exit", 2);

        // Reset key for next room
        GameManager.Instance.hasKey = false;

        if (BetweenRoomManager.I != null)
            BetweenRoomManager.I.ShowScreen(nextSceneName);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }

    // Wire this to a trigger collider or an interact button
    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) TryOpen();
    }
}
