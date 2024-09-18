using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Forest.Movement
{
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] float sensitivity = 100f;
        //[SerializeField] float smoothTime = 0.1f;

        [SerializeField] Transform orientation;

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
            Look();
        }

        void Look()
        {
            Vector2 targetMouseDelta = sensitivity * Time.fixedUnscaledDeltaTime * lookAction.ReadValue<Vector2>();

            // Smooth the mouse delta
            /*currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, smoothTime);

            float mouseX = currentMouseDelta.x;
            float mouseY = currentMouseDelta.y;*/

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
}