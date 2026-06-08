using UnityEngine; using MazeMind.Core;
// Drop on an empty in Room1 — configures PlayerMetrics for this room and binds the player.
public class Room1Init : MonoBehaviour {
    public Transform player;
    public int totalWalkableTiles = 220;
    public int gemsAvailable = 4;
    void Start() {
        if (PlayerMetrics.I == null) {
            Debug.LogError("Room1Init: PlayerMetrics.I is null. Did you load Boot scene first?");
            return;
        }
        PlayerMetrics.I.ConfigureRoom(totalWalkableTiles, gemsAvailable);
        if (player != null) PlayerMetrics.I.Bind(player);
    }
}