#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayModeBootLoader
{
    static PlayModeBootLoader()
    {
        var bootScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Boot.unity");
        if (bootScene != null)
            EditorSceneManager.playModeStartScene = bootScene;
    }
}
#endif
