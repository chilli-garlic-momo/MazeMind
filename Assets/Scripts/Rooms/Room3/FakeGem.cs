using UnityEngine;

public class FakeGem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("Fake gem collected!");

        GameManager.Instance.gems =
            Mathf.Max(0, GameManager.Instance.gems - 1);

        Destroy(gameObject);
    }
}