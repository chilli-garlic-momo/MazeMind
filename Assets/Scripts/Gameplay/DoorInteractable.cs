using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteractable : MonoBehaviour
{
    [Tooltip("Scene to load when door opens (Room 1 -> Room2).")]
    public string nextSceneName = "Room2";

    [Tooltip("If true, show the BetweenRoomManager stats screen before loading.")]
    public bool useBetweenRoomScreen = true;

    private bool opened = false;

    public void TryOpenDoor()
    {
        if (opened) return;

        if (!GameManager.Instance.hasKey)
        {
            Debug.Log("Need a Key");
            return;
        }

        transform.Rotate(0f, 90f, 0f);
        opened = true;
        Debug.Log("Door Opened");

        // Consume key + advance room state
        GameManager.Instance.hasKey = false;
        GameManager.Instance.ResetForNextRoom();

        if (string.IsNullOrEmpty(nextSceneName)) return;

        if (useBetweenRoomScreen && BetweenRoomManager.I != null)
            BetweenRoomManager.I.ShowScreen(nextSceneName);
        else
            SceneManager.LoadScene(nextSceneName);
    }
}
