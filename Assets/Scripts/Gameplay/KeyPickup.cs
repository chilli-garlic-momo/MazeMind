using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [Tooltip("If true, destroys the closest pickup object, but never the scene/section root.")]
    public bool destroyRoot = true;

    [Tooltip("Optional explicit object to destroy. Generators set this so spawned keys only delete themselves.")]
    public Transform destroyTargetOverride;

    [Tooltip("Safety limit for old prefabs where the pickup script is on a child of the visible key.")]
    public int maxDestroyParentHops = 2;

    [Tooltip("Adds a simple visible marker at runtime if the prefab only has a trigger collider.")]
    public bool autoCreateVisibleMarker = true;

    bool _collected;
    bool _invalidContainerPickup;

    void Awake()
    {
        // Safety guard for scene mistakes: if this script is placed on a room / section
        // container (for example Section_1_5), collecting it would destroy that whole
        // hierarchy and make the player fall through the missing floor. Real keys are
        // small pickup objects, not protected scene containers.
        if (destroyTargetOverride == null && IsProtectedAncestor(transform))
        {
            _invalidContainerPickup = true;
            Debug.LogWarning($"[KeyPickup] Disabled unsafe pickup on protected container '{name}'. Move KeyPickup to the actual key object instead.");
            return;
        }

        EnsureTriggerCollider();
        EnsureVisibleMarker();
    }

    void Reset()
    {
        EnsureTriggerCollider();
    }

    void EnsureTriggerCollider()
    {
        var c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void EnsureVisibleMarker()
    {
        if (!autoCreateVisibleMarker) return;
        if (GetComponentInChildren<Renderer>(true) != null) return;

        var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = "Runtime_Key_Visual";
        marker.transform.SetParent(transform, false);
        marker.transform.localPosition = Vector3.zero;
        marker.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        marker.transform.localScale = new Vector3(0.55f, 0.08f, 0.55f);

        var markerCollider = marker.GetComponent<Collider>();
        if (markerCollider != null)
        {
            markerCollider.enabled = false;
            Destroy(markerCollider);
        }

        var renderer = marker.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1f, 0.82f, 0.16f, 1f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (_invalidContainerPickup) return;
        if (_collected) return;
        if (!other.CompareTag("Player")) return;

        _collected = true;

        GameManager.EnsureExists().CollectKey();
        Debug.Log("[KeyPickup] Key collected. hasKey=" + GameManager.Instance.hasKey);

        GameObject target = ResolveDestroyTarget();

        // Disable every collider on the target tree first so there's no 1-frame
        // window where a non-trigger collider blocks the player before Destroy runs.
        foreach (var col in target.GetComponentsInChildren<Collider>(true))
            col.enabled = false;

        // Hide visuals immediately for feedback.
        foreach (var r in target.GetComponentsInChildren<Renderer>(true))
            r.enabled = false;

        Destroy(target);
    }

    GameObject ResolveDestroyTarget()
    {
        if (destroyTargetOverride != null) return destroyTargetOverride.gameObject;
        if (!destroyRoot) return gameObject;
        if (IsProtectedAncestor(transform)) return gameObject;

        // IMPORTANT: transform.root is unsafe for spawned keys parented under a room/section.
        // It can destroy the whole Section_1_3 hierarchy, making the floor vanish and the
        // player fall immediately after collecting the key. Only climb to a nearby pickup
        // container, and stop before section/room/world parents.
        Transform target = transform;
        if (HasPickupShape(target)) return target.gameObject;

        int hops = Mathf.Max(0, maxDestroyParentHops);
        while (hops-- > 0 && target.parent != null && !IsProtectedAncestor(target.parent))
        {
            target = target.parent;
            if (HasPickupShape(target)) return target.gameObject;
        }

        return gameObject;
    }

    static bool HasPickupShape(Transform t)
    {
        return t.GetComponent<Collider>() != null || t.GetComponent<Renderer>() != null;
    }

    static bool IsProtectedAncestor(Transform t)
    {
        string n = t.name;
        return t.parent == null
            || n.StartsWith("Section_")
            || n.Contains("Room")
            || n.Contains("World")
            || t.GetComponent<CheckboxFloorGenerator>() != null;
    }
}
