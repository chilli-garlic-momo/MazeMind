using UnityEngine;
using UnityEngine.InputSystem;
using MazeMind.Core;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.2f;
    public float doubleJumpHeight = 1.0f;
    public float gravity = -20f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.2f;
    public Transform cameraRig;

    [Header("References (optional)")]
    public CharacterController controller;  // kept for compat — auto-assigned if null

    private Vector3 _velocity;
    private float _pitch = 0f;
    private bool _hasMoved = false;

    private bool _canDoubleJump = false;
    private bool _usedDoubleJump = false;

    void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        if (Mouse.current == null) return;
        var delta = Mouse.current.delta.ReadValue();
        float yaw = delta.x * mouseSensitivity;
        float pitch = delta.y * mouseSensitivity;

        transform.Rotate(0, yaw, 0);

        _pitch -= pitch;
        _pitch = Mathf.Clamp(_pitch, -80f, 80f);
        if (cameraRig != null)
            cameraRig.localRotation = Quaternion.Euler(_pitch, 0, 0);
    }

    void HandleMovement()
    {
        if (Keyboard.current == null) return;

        float h = 0f, v = 0f;
        if (Keyboard.current.aKey.isPressed) h = -1f;
        if (Keyboard.current.dKey.isPressed) h = 1f;
        if (Keyboard.current.wKey.isPressed) v = 1f;
        if (Keyboard.current.sKey.isPressed) v = -1f;

        Vector3 move = transform.right * h + transform.forward * v;
        move = Vector3.ClampMagnitude(move, 1f);

        if (move.magnitude > 0.1f && !_hasMoved)
        {
            _hasMoved = true;
            if (PlayerMetrics.I != null)
                PlayerMetrics.I.Bind(transform);
        }

        // Sprint with Shift
        float currentSpeed = walkSpeed;
        if (Keyboard.current.leftShiftKey.isPressed)
            currentSpeed = sprintSpeed;

        if (controller.isGrounded)
        {
            if (_velocity.y < 0) _velocity.y = -2f;
            _canDoubleJump = false;
            _usedDoubleJump = false;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (controller.isGrounded)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _canDoubleJump = true;
            }
            else if (_canDoubleJump && !_usedDoubleJump)
            {
                _velocity.y = Mathf.Sqrt(doubleJumpHeight * -2f * gravity);
                _usedDoubleJump = true;
            }
        }

        _velocity.y += gravity * Time.deltaTime;
        controller.Move((move * currentSpeed + _velocity) * Time.deltaTime);
    }

    public bool IsInAir => !controller.isGrounded;
    public Vector3 Velocity => _velocity;
}