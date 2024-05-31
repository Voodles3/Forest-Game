using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float groundDrag;
    [SerializeField] float airDrag;
    [SerializeField] float dragTransitionTime;
    bool dragTransitioning;
    bool canAccelerate = true;

    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    [SerializeField] float airSpeedMultiplier;
    [SerializeField] float jumpGravityMultiplier;
    bool readyToJump = true;

    [Header("Ground Check")]
    public LayerMask groundMask;
    [SerializeField] float groundDistance;
    [SerializeField] [ReadOnly(true)] bool grounded;

    public Transform orientation;
    public Transform playerBody;
    public TMP_Text velocityText;

    PlayerActions inputActions;
    InputAction jumpAction;

    Rigidbody rb;
    Vector3 moveDirection;
    Vector2 inputs;

    void Awake()
    {
        inputActions = new PlayerActions();
        jumpAction = inputActions.Gameplay.Jump;
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
        GroundCheck();
        LimitSpeed();
        ApplyDrag();

        Vector3 horizVel = new(rb.velocity.x, 0, rb.velocity.z);
        velocityText.text = $"Velocity: {horizVel.magnitude:F2}";
    }

    void FixedUpdate() 
    {
        MovePlayer();
        ApplyGravity();
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
            moveForce = 350f * moveSpeed * Time.deltaTime * moveDirection.normalized;
        }
        else
        {
            moveForce = 350f * airSpeedMultiplier * moveSpeed * Time.deltaTime * moveDirection.normalized;
        }
        if (canAccelerate) { rb.AddForce(moveForce, ForceMode.Force); }
    }

    void OnJump()
    {
        if (readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    void Jump()
    {
        rb.velocity = new(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ApplyGravity()
    {
        // Jumping gravity
        if (!grounded)
        {
            rb.AddForce(Physics.gravity * jumpGravityMultiplier, ForceMode.Force);
        }
    }

    void GroundCheck()
    {
        if (Physics.Raycast(playerBody.transform.position, Vector3.down, groundDistance, groundMask))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }
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
