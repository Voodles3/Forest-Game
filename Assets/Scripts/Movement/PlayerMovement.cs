using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Forest.UI;
using UnityEditor.Rendering.LookDev;

namespace Forest.Movement
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] float walkSpeed;
        [SerializeField] float crouchSpeed;
        [SerializeField] float sprintSpeed;
        [SerializeField] float stopForce;
        [SerializeField] float groundDrag;
        [SerializeField] float airDrag;
        [SerializeField] float staticDrag;
        [SerializeField] float dragTransitionTime;
        [SerializeField] float dynamicFriction;
        [SerializeField] float staticFriction;
        [SerializeField] MovementState currentMovementState;
        float moveSpeed;
        bool dragTransitioning;
        bool canAccelerate = true;
        public bool isMoving;

        [Header("Jumping")]
        [SerializeField] float jumpForce;
        [SerializeField] float jumpCooldown;
        [SerializeField] float airSpeedMultiplier;
        [SerializeField] float fallGravity;
        [SerializeField] float jumpPeakGravity;
        [SerializeField] float gravity;
        public bool grounded;
        bool readyToJump = true;

        [Header("View Bobbing")]
        [SerializeField] float crouchBobSpeed;
        [SerializeField] float walkBobSpeed;
        [SerializeField] float sprintBobSpeed;

        [Header("FOV")]
        [SerializeField] float walkFOV;
        [SerializeField] float sprintFOV;
        [SerializeField] float FOVTransitionTime;
        float targetFOV;

        [Header("Stamina")]
        [SerializeField] float sprintCost;
        [SerializeField] float jumpCost;

        [Header("Visiblity")]
        [SerializeField] float crouchVisibility;
        [SerializeField] float walkVisibility;
        [SerializeField] float sprintVisibility;
        float currentVisiblity;

        [Header("Noise")]
        [SerializeField] float crouchNoise;
        [SerializeField] float walkNoise;
        [SerializeField] float sprintNoise;
        float currentNoiseRadius;

        [Header("References")]
        [SerializeField] Transform orientation;
        [SerializeField] Transform playerBody;
        [SerializeField] PhysicMaterial playerMaterial;
        [SerializeField] LayerMask groundMask;
        PlayerLook look;
        Animator animator;
        StaminaBar staminaBar;

        PlayerActions inputActions;
        InputAction jumpAction;
        InputAction sprintAction;
        InputAction crouchAction;

        Rigidbody rb;
        Vector3 moveDirection;
        Vector2 inputs;

        public enum MovementState
        {
            walking,
            sprinting,
            air, 
            crouching
        }

        public MovementState CurrentMovementState
        {
            get { return currentMovementState; }
        }
        public float CurrentNoiseRadius
        {
            get { return currentNoiseRadius; }
        }
        public float CurrentVisibility
        {
            get { return currentVisiblity; }
        }


        void Awake()
        {
            inputActions = new PlayerActions();
            jumpAction = inputActions.Gameplay.Jump;
            sprintAction = inputActions.Gameplay.Sprint;
            crouchAction = inputActions.Gameplay.Crouch;
            
            animator = GetComponent<Animator>();
            staminaBar = FindObjectOfType<StaminaBar>();
            look = FindObjectOfType<PlayerLook>();
        }

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            rb.drag = groundDrag;
        }

        void Update()
        {
            IsMoving();
            ChangeFOV();
            ReceiveInput();
            SetState();
            LimitSpeed();
            ApplyDrag();
            ApplyStopForce();
            DrainStamina();
        }

        void FixedUpdate() 
        {
            MovePlayer();
            ApplyGravity();
        }

        void IsMoving()
        {
            if (inputs.magnitude > 0f && rb.velocity.magnitude > 0.5f)
            {
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }
        }

        void ChangeFOV()
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, Time.deltaTime / FOVTransitionTime);
        }

        void SetState()
        {
            if (crouchAction.ReadValue<float>() > 0f)
            {
                currentMovementState = MovementState.crouching;

                moveSpeed = crouchSpeed;
                look.currentBobSpeed = crouchBobSpeed;
                currentVisiblity = crouchVisibility;
                currentNoiseRadius = crouchNoise;
            }
            else if (staminaBar.currentStamina > 0f && grounded && sprintAction.ReadValue<float>() > 0f && isMoving)
            {
                currentMovementState = MovementState.sprinting;

                moveSpeed = sprintSpeed;
                look.currentBobSpeed = sprintBobSpeed;
                targetFOV = sprintFOV;
                currentVisiblity = sprintVisibility;
                currentNoiseRadius = sprintNoise;
            }
            else if (grounded)
            {
                currentMovementState = MovementState.walking;

                moveSpeed = walkSpeed;
                look.currentBobSpeed = walkBobSpeed;
                targetFOV = walkFOV;
                currentVisiblity = walkVisibility;
                currentNoiseRadius = walkNoise;
            }
            else
            {
                currentMovementState = MovementState.air;
            }
        }

        void ReceiveInput()
        {
            inputs = inputActions.Gameplay.Movement.ReadValue<Vector2>(); // Move
            if (jumpAction.ReadValue<float>() > 0f) { OnJump(); } // Jump
            Crouch(crouchAction.ReadValue<float>() > 0f); // Crouch
        }

        void MovePlayer()
        {
            Vector3 moveForce;
            moveDirection = orientation.forward * inputs.y + orientation.right * inputs.x;

            if (grounded)
            {
                moveForce = 450f * moveSpeed * Time.deltaTime * moveDirection.normalized;
                //rb.useGravity = false;
            }
            else
            {
                moveForce = 450f * airSpeedMultiplier * moveSpeed * Time.deltaTime * moveDirection.normalized;
                //rb.useGravity = true;
            }
            if (canAccelerate) { rb.AddForce(moveForce, ForceMode.Force); }
        }

        void Crouch(bool crouched)
        {
            animator.SetBool("crouched", crouched);
        }

        void OnJump()
        {
            if (readyToJump && grounded && staminaBar.currentStamina >= jumpCost)
            {
                readyToJump = false;
                playerMaterial.dynamicFriction = 0f;
                playerMaterial.staticFriction = 0f;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
                Invoke(nameof(ResetFriction), 0.2f);
            }
        }

        void Jump()
        {
            rb.velocity = new(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            staminaBar.RemoveStamina(jumpCost);
        }

        void ApplyGravity()
        {
            if (!grounded && rb.velocity.y < 0.5f)
            {
                rb.AddForce(100f * fallGravity * Time.deltaTime * Physics.gravity, ForceMode.Force);
            }
            else if (!grounded && rb.velocity.y < 0f)
            {
                rb.AddForce(100f * jumpPeakGravity * Time.deltaTime * Physics.gravity, ForceMode.Force);
            }
            else if (!grounded)
            {
                rb.AddForce(100f * gravity * Time.deltaTime * Physics.gravity, ForceMode.Force);
            }
        }

        public void GroundCheck(bool onGround) // Called from GroundCheck script
        {
            grounded = onGround;
        }

        void ApplyDrag()
        {
            if (readyToJump && grounded && Mathf.Abs(rb.velocity.magnitude) < 0.2f && inputs.magnitude == 0f)
            {
                rb.drag = staticDrag;
            }
            else if (readyToJump && grounded)
            {
                StartCoroutine(SmoothDrag());
            }
            else
            {
                StopAllCoroutines();
                rb.drag = airDrag;
            }
        }

        IEnumerator SmoothDrag()
        {
            float timeElapsed = 0f;
            dragTransitioning = true;
            while (timeElapsed <= dragTransitionTime)
            {
                rb.drag = Mathf.Lerp(airDrag, groundDrag, timeElapsed / dragTransitionTime);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            rb.drag = groundDrag;
            dragTransitioning = false;
        }

        void ResetFriction()
        {
            playerMaterial.dynamicFriction = dynamicFriction;
            playerMaterial.staticFriction = staticFriction;
        }

        void LimitSpeed()
        {
            Vector3 horizVel = new(rb.velocity.x, 0f, rb.velocity.z);

            if (horizVel.magnitude > moveSpeed && grounded)
            {
                canAccelerate = false;
                //Vector3 correctedVel = horizVel.normalized * moveSpeed;
                //rb.velocity = new(correctedVel.x, rb.velocity.y, correctedVel.z);
            }
            else
            {
                canAccelerate = true;
            }
        }

        void ApplyStopForce()
        {
            if (inputs.magnitude == 0f && grounded)
            {
                Vector3 horizVel = new(rb.velocity.x, 0, rb.velocity.z);

                if (Mathf.Abs(horizVel.magnitude) > 0.5f)
                {
                    rb.AddForce(100f * stopForce * Time.deltaTime * -horizVel.normalized, ForceMode.Force);
                }
                else
                {
                    rb.velocity = new(0f, rb.velocity.y, 0f);
                }
            }
        }

        void DrainStamina()
        {
            if (currentMovementState == MovementState.sprinting)
            {
                staminaBar.StartDrainingStamina(sprintCost);
            }
            else
            {
                staminaBar.StopDrainingStamina();
            }
        }

        void ResetJump()
        {
            readyToJump = true;
        }

        void OnEnable()
        {
            inputActions.Gameplay.Enable();
        }

        void OnDisable()
        {
            inputActions.Gameplay.Disable();
        }
    }
}

