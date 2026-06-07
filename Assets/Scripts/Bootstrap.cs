// File: Bootstrap.cs
using UnityEngine; using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour {
    [Tooltip("Name of your persistent-managers scene (Boot).")]
    public string bootScene = "Boot";
    public string firstRoom = "MainMenu";

    void Start() {
        // If managers are already alive (scene reloaded), go straight to first room
        if (MazeMind.Core.AIDirector.I != null) {
            SceneManager.LoadScene(firstRoom);
            return;
        }
        SceneManager.LoadScene(bootScene, LoadSceneMode.Additive);
        SceneManager.LoadScene(firstRoom);
    }
}
