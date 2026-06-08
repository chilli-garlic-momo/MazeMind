using UnityEngine;
public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    float xRotation = 0f;
    bool _warned = false;

    void Start()
    {
        // Auto-bind playerBody to the root (Player) if it wasn't set in the inspector.
        if (playerBody == null)
        {
            var root = transform;
            while (root.parent != null) root = root.parent;
            playerBody = root;
            Debug.LogWarning(
                $"[MouseLook] playerBody was unassigned on '{name}'. Auto-bound to '{root.name}'. " +
                "If PlayerController is also on the Player, you can remove this MouseLook component to silence duplicate look input.");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (playerBody == null)
        {
            if (!_warned) { Debug.LogWarning("[MouseLook] playerBody still null, skipping look."); _warned = true; }
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
