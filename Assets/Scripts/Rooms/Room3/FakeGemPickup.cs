using UnityEngine;

public class FakeGemPickup : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private int gemPenalty = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Remove health
        PlayerHealth health = other.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.Damage(damage);
            Debug.Log("Lost " + damage + " HP");
        }

        // Remove gems
        GameManager.Instance.gems =
            Mathf.Max(0, GameManager.Instance.gems - gemPenalty);

        Debug.Log(
            "Lost " + gemPenalty +
            " gem(s). Gems left: " +
            GameManager.Instance.gems);

        Destroy(gameObject);
    }
}