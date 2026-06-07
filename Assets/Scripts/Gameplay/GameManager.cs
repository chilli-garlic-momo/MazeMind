// File: GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    public int  gems       = 0;
    public bool hasKey     = false;
    public int  health     = 5;
    public int  currentRoom = 1;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    // Call between rooms to wipe pickup state (AI Director state persists separately)
    public void ResetForNextRoom() {
        gems   = 0;
        hasKey = false;
        currentRoom++;
    }
}