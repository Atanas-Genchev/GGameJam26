using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple first-person controller for walking simulator gameplay.
/// Handles WASD movement, mouse look, and jumping using the new Input System.
/// </summary>
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float gravity = -19.62f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1.5f;
    
    [Header("Mouse Look Settings")]
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private float maxLookAngle = 80f;
    
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    
    private CharacterController characterController;
    private Vector3 playerVelocity;
    private float xRotation = 0f;
    
    // Input values
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool cursorLocked = true;
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        // Lock and hide cursor for FPS controls
        LockCursor();
        
        // Auto-find camera if not assigned
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
            }
        }
    }
    
    private void Update()
    {
        ReadInput();
        HandleMouseLook();
        HandleMovement();
        HandleCursorToggle();
    }
    
    private void ReadInput()
    {
        // Read keyboard input for movement
        Vector2 wasd = Vector2.zero;
        Keyboard keyboard = Keyboard.current;
        
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) wasd.y += 1f;
            if (keyboard.sKey.isPressed) wasd.y -= 1f;
            if (keyboard.dKey.isPressed) wasd.x += 1f;
            if (keyboard.aKey.isPressed) wasd.x -= 1f;
        }
        moveInput = wasd.normalized;
        
        // Read mouse input for looking
        Mouse mouse = Mouse.current;
        if (mouse != null && cursorLocked)
        {
            lookInput = mouse.delta.ReadValue();
        }
        else
        {
            lookInput = Vector2.zero;
        }
    }
    
    private void HandleMouseLook()
    {
        if (!cursorLocked) return;
        
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;
        
        // Vertical rotation (pitch) - rotate the camera
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        
        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        
        // Horizontal rotation (yaw) - rotate the player body
        transform.Rotate(Vector3.up * mouseX);
    }
    
    private void HandleMovement()
    {
        bool isGrounded = characterController.isGrounded;
        
        // Reset vertical velocity when grounded
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        
        // Reset horizontal velocity every frame - movement is from input only
        playerVelocity.x = 0f;
        playerVelocity.z = 0f;
        
        // Calculate movement direction relative to player facing
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        
        // Handle jump
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame && isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        // Apply gravity
        playerVelocity.y += gravity * Time.deltaTime;
        
        // Combine horizontal movement and vertical velocity into single Move call
        Vector3 finalMove = (move * walkSpeed + playerVelocity) * Time.deltaTime;
        characterController.Move(finalMove);
    }
    
    private void HandleCursorToggle()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            if (cursorLocked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }
    }
    
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cursorLocked = true;
    }
    
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorLocked = false;
    }
}
