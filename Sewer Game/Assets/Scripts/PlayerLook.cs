using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    public float sensitivity = 100f;
    public float smoothing = 0.1f;
    public Transform playerBody;

    private PlayerControls playerControls;
    private InputAction lookAction;

    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;

    private float xRotation = 0f;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        lookAction = playerControls.Camera.Look;
        lookAction.Enable();
    }

    private void OnDisable()
    {
        lookAction.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        Vector2 targetMouseDelta = lookAction.ReadValue<Vector2>() * sensitivity * Time.smoothDeltaTime;

        // Smooth the mouse delta using Lerp for simplicity
        currentMouseDelta = Vector2.Lerp(currentMouseDelta, targetMouseDelta, smoothing);

        float mouseX = currentMouseDelta.x;
        float mouseY = currentMouseDelta.y;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
