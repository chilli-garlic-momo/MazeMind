using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Touched by: "+other.name);
        
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.hasKey = true;

            Debug.Log("Key Collected");

            Destroy(gameObject);
        }
    }
}