// File: GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int gems = 0;
    public bool hasKey = false;
    public int health = 5;
    public int currentRoom = 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void BootstrapInstance()
    {
        EnsureExists();
    }

    public static GameManager EnsureExists()
    {
        if (Instance != null) return Instance;

#pragma warning disable 0618
        var existing = FindObjectOfType<GameManager>();
#pragma warning restore 0618
        if (existing != null)
        {
            existing.BecomeInstance();
            return existing;
        }

        var go = new GameObject("GameManager");
        return go.AddComponent<GameManager>();
    }

    void Awake()
    {
        BecomeInstance();
    }

    void BecomeInstance()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void AddGem(int amount = 1)
    {
        gems = Mathf.Max(0, gems + amount);
    }

    public void CollectKey()
    {
        hasKey = true;
    }

    public bool SpendGems(int amount)
    {
        if (amount <= 0) return true;
        if (gems < amount) return false;
        gems -= amount;
        return true;
    }

    // Call between rooms to wipe pickup state (AI Director state persists separately).
    public void ResetForNextRoom()
    {
        gems = 0;
        hasKey = false;
        currentRoom++;
    }

    // Use this from WinScreen.PlayAgain so the menu starts clean.
    public void ResetGame()
    {
        gems = 0;
        hasKey = false;
        health = 5;
        currentRoom = 1;
    }
}
