using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class FirstPersonController : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Look")]
    public Transform playerCamera;
    public float mouseSensitivity = 0.12f;
    public float controllerLookSensitivity = 120f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private float xRotation = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (playerCamera != null)
            {
                Camera cam = playerCamera.GetComponent<Camera>();
                if (cam != null) cam.enabled = false;

                AudioListener listener = playerCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false;
            }

            enabled = false;
            return;
        }

        if (playerCamera != null)
        {
            Camera cam = playerCamera.GetComponent<Camera>();
            if (cam != null) cam.enabled = true;

            AudioListener listener = playerCamera.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!IsOwner) return;

        LookAround();
        MovePlayer();
    }

    void MovePlayer()
    {
        Vector2 moveInput = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.zKey.isPressed)
                moveInput.y += 1;

            if (Keyboard.current.sKey.isPressed)
                moveInput.y -= 1;

            if (Keyboard.current.dKey.isPressed)
                moveInput.x += 1;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.qKey.isPressed)
                moveInput.x -= 1;
        }

        if (Gamepad.current != null)
        {
            moveInput += Gamepad.current.leftStick.ReadValue();
        }

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        if (animator != null)
            animator.SetFloat("Speed", moveInput.magnitude);

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * moveSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void LookAround()
    {
        Vector2 lookInput = Vector2.zero;

        if (Mouse.current != null)
            lookInput += Mouse.current.delta.ReadValue() * mouseSensitivity;

        if (Gamepad.current != null)
            lookInput += Gamepad.current.rightStick.ReadValue() * controllerLookSensitivity * Time.deltaTime;

        float mouseX = lookInput.x;
        float mouseY = lookInput.y;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}