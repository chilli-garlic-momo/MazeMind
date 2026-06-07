using UnityEngine;
using MazeMind.Core;

public class Room2Init : MonoBehaviour
{
    public static Room2Init I;

    public Transform player;
    public int totalWalkableTiles = 240;
    public int gemsAvailable = 5;

    [Header("Room References")]
    public GameObject wallBackHide;

    private void Awake()
    {
        I = this;
    }

    void Start()
    {
        if (PlayerMetrics.I == null)
        {
            Debug.LogError("Room2Init: PlayerMetrics.I is null.");
            return;
        }

        PlayerMetrics.I.ConfigureRoom(
            totalWalkableTiles,
            gemsAvailable);

        if (player != null)
            PlayerMetrics.I.Bind(player);

        AIDirector.I?.Fire(
            TriggerKind.OnSectionEnter,
            "2.1",
            2);

        var hp =
            player?.GetComponent<PlayerHealth>();

        if (hp != null)
            hp.RegisterCheckpoint(
                player.position,
                "2.1");
    }

    public void OnRealKeyCollected()
    {
        if (wallBackHide != null)
        {
            wallBackHide.SetActive(false);
        }
    }
}