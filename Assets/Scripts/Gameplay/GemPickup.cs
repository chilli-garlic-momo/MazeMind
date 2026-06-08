using UnityEngine;

public class GemPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var gm = GameManager.EnsureExists();
        gm.AddGem();

        Debug.Log("Gems: " + gm.gems);
        Destroy(gameObject);
    }
}
