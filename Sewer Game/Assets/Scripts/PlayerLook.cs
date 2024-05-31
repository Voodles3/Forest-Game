using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    public Transform orientation;
    public InputAction lookAction;

    public float sensitivity = 100f;
    public float smoothTime = 0.1f;

    Vector2 currentMouseDelta;
    Vector2 currentMouseDeltaVelocity;

    float pitch = 0f;
    float yaw = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Vector2 targetMouseDelta = lookAction.ReadValue<Vector2>() * sensitivity * Time.fixedUnscaledDeltaTime;

        // Smooth the mouse delta
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, smoothTime);

        //Debug.Log($"Current mouse delta: {currentMouseDelta}");

        //float mouseX = currentMouseDelta.x;
        //float mouseY = currentMouseDelta.y;

        float mouseX = targetMouseDelta.x;
        float mouseY = targetMouseDelta.y;

        //float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivity;
        //float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivity;

        yaw += mouseX;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        orientation.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    private void OnEnable()
    {
        lookAction.Enable();
    }

    private void OnDisable()
    {
        lookAction.Disable();
    }
}
