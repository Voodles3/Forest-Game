using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    float moveSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float stopForce;
    [SerializeField] float groundDrag;
    [SerializeField] float airDrag;
    [SerializeField] float dragTransitionTime;
    [SerializeField] float dynamicFriction;
    [SerializeField] float staticFriction;
    [SerializeField] MovementState currentMovementState;
    bool dragTransitioning;
    bool canAccelerate = true;

    [Header("Jumping")]
    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    [SerializeField] float airSpeedMultiplier;
    [SerializeField] float fallGravity;
    [SerializeField] float jumpPeakGravity;
    [SerializeField] float gravity;
    [SerializeField] bool grounded;
    bool readyToJump = true;

    [Header("References")]
    [SerializeField] Transform orientation;
    [SerializeField] Transform playerBody;
    [SerializeField] PhysicMaterial playerMaterial;
    [SerializeField] LayerMask groundMask;
    [SerializeField] TMP_Text velocityText;
    [SerializeField] TMP_Text maxVelocityText;
    float maxVel;

    PlayerActions inputActions;
    InputAction jumpAction;
    InputAction sprintAction;

    Rigidbody rb;
    Vector3 moveDirection;
    Vector2 inputs;

    enum MovementState
    {
        walking,
        sprinting,
        air, 
        crouching
    }

    void Awake()
    {
        inputActions = new PlayerActions();
        jumpAction = inputActions.Gameplay.Jump;
        sprintAction = inputActions.Gameplay.Sprint;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.drag = groundDrag;
    }

    void Update()
    {
        ReceiveInput();
        LimitSpeed();
        ApplyDrag();
        StateHandler();
        ApplyStopForce();
        UpdateHUDText();
    }

    void FixedUpdate() 
    {
        MovePlayer();
        ApplyGravity();
    }

    void UpdateHUDText()
    {
        Vector3 horizVel = new(rb.velocity.x, 0, rb.velocity.z);
        velocityText.text = $"Velocity: {horizVel.magnitude:F2}";
        if (horizVel.magnitude > maxVel)
        {
            maxVel = horizVel.magnitude;
            maxVelocityText.text = $"Max Velocity: {maxVel:F5}";
        }
    }

    void StateHandler()
    {
        if (grounded && sprintAction.ReadValue<float>() > 0f)
        {
            currentMovementState = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            currentMovementState = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            currentMovementState = MovementState.air;
        }
    }

    void ReceiveInput()
    {
        inputs = inputActions.Gameplay.Movement.ReadValue<Vector2>();
        
        if (jumpAction.ReadValue<float>() > 0)
        {
            OnJump();
        }
    }

    void MovePlayer()
    {
        Vector3 moveForce;
        moveDirection = orientation.forward * inputs.y + orientation.right * inputs.x;

        if (grounded)
        {
            moveForce = 450f * moveSpeed * Time.deltaTime * moveDirection.normalized;
            rb.useGravity = false;
        }
        else
        {
            moveForce = 450f * airSpeedMultiplier * moveSpeed * Time.deltaTime * moveDirection.normalized;
            rb.useGravity = true;
        }
        if (canAccelerate) { rb.AddForce(moveForce, ForceMode.Force); }
    }

    void OnJump()
    {
        if (readyToJump && grounded)
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
        if (grounded && Mathf.Abs(rb.velocity.y) < 0.1f)
        {
            if (rb.drag == airDrag)
            {
                StartCoroutine(SmoothDrag());
            }
        }
        else
        {
            rb.drag = airDrag;
            StopAllCoroutines();
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
