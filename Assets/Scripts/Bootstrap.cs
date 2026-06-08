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
        // If AIDirector already exists, Boot already ran — skip straight to the room
        if (MazeMind.Core.AIDirector.I != null) {
            SceneManager.LoadScene(firstRoom);
            return;
        }
        SceneManager.LoadScene(bootScene);  // Boot scene's own Awake will load Room1

            }
}
