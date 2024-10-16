using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Forest.Movement
{
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] float sensitivity = 100f;
        //[SerializeField] float smoothTime = 0.1f;

        [SerializeField] Transform facingDirection;

        [SerializeField] float bobAmount;
        public float currentBobSpeed;
        float bobTimer;
        float defaultPosY;

        PlayerMovement playerMovement;
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
            playerMovement = FindObjectOfType<PlayerMovement>();
        }

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            defaultPosY = transform.localPosition.y;
        }

        void Update()
        {
            Look();
            ViewBobbing();
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
            facingDirection.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        void ViewBobbing()
        {
            if (playerMovement.grounded && playerMovement.isMoving)
            {
                bobTimer += Time.deltaTime * currentBobSpeed;
                transform.localPosition = new Vector3(transform.localPosition.x, defaultPosY + Mathf.Sin(bobTimer) * bobAmount, transform.localPosition.z);
            }
            else
            {
                bobTimer = 0;
                transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(transform.localPosition.y, defaultPosY, Time.deltaTime * currentBobSpeed * 2));
            }
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