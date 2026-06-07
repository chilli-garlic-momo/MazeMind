using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Touched by: " + other.name);

        if (!other.CompareTag("Player")) return;

        GameManager.EnsureExists().CollectKey();
        Debug.Log("Key Collected");
        Destroy(gameObject);
    }
}
