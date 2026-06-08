using UnityEngine;

public class RealKeyPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        GameManager.Instance.hasKey = true;

        Debug.Log("REAL KEY COLLECTED");

        if (Room2Init.I != null)
        {
            Room2Init.I.OnRealKeyCollected();
        }

        Destroy(gameObject);
    }
}