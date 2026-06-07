// File: Bootstrap.cs
using UnityEngine; using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour {
    [Tooltip("Name of your persistent-managers scene (Boot).")]
    public string bootScene = "Boot";
    public string firstRoom = "Room1";

    void Start() {
        // If managers are already alive (scene reloaded), go straight to Room1
        if (MazeMind.Core.AIDirector.I != null) {
            SceneManager.LoadScene(firstRoom);
            return;
        }
        SceneManager.LoadScene(bootScene, LoadSceneMode.Additive);
        SceneManager.LoadScene(firstRoom);
    }
}