using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    public float sensitivity = 100f;
    public float smoothTime = 0.1f;

    public Transform orientation;

    PlayerActions inputActions;
    InputAction lookAction;

    Vector2 currentMouseDelta;
    Vector2 currentMouseDeltaVelocity;

    float pitch = 0f;
    float yaw = 0f;

    void Awake()
    {
        inputActions = new PlayerActions();
        lookAction = inputActions.Camera.Look;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Vector2 targetMouseDelta = lookAction.ReadValue<Vector2>() * sensitivity * Time.fixedUnscaledDeltaTime;

        // Smooth the mouse delta
        //currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, smoothTime);

        //float mouseX = currentMouseDelta.x;
        //float mouseY = currentMouseDelta.y;

        float mouseX = targetMouseDelta.x;
        float mouseY = targetMouseDelta.y;

        yaw += mouseX;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        orientation.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void OnEnable()
    {
        inputActions.Camera.Enable();
    }

    void OnDisable()
    {
        inputActions.Camera.Disable();
    }
}
