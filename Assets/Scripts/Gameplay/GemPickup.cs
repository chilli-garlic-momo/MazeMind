using UnityEngine;

public class GemPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.gems++;

            Debug.Log(
                "Gems: " +
                GameManager.Instance.gems);

            Destroy(gameObject);
        }
    }
}