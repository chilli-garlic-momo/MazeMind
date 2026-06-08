using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [Tooltip("If true, destroys the root GameObject (handles cases where the visible mesh/collider is on a parent).")]
    public bool destroyRoot = true;

    bool _collected;

    void Reset()
    {
        var c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_collected) return;
        if (!other.CompareTag("Player")) return;

        _collected = true;

        GameManager.EnsureExists().CollectKey();
        Debug.Log("[KeyPickup] Key collected. hasKey=" + GameManager.Instance.hasKey);

        GameObject target = destroyRoot ? transform.root.gameObject : gameObject;

        // Disable every collider on the target tree first so there's no 1-frame
        // window where a non-trigger collider blocks the player before Destroy runs.
        foreach (var col in target.GetComponentsInChildren<Collider>(true))
            col.enabled = false;

        // Hide visuals immediately for feedback.
        foreach (var r in target.GetComponentsInChildren<Renderer>(true))
            r.enabled = false;

        Destroy(target);
    }
}
