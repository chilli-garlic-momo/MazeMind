using UnityEngine;
using UnityEngine.SceneManagement;

public class BootInit : MonoBehaviour {
    public string firstRoom = "Room1";
    void Start() { SceneManager.LoadScene(firstRoom); }
}