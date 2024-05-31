using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1f;

    public Transform orientation;
    public TMP_Text velocityText;
    public InputAction moveAction;

    Rigidbody rb;
    Vector3 moveDirection;
    Vector2 inputs;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        velocityText.text = $"Velocity: {rb.velocity.magnitude:F2}";
        ReceiveInput();
    }

    void FixedUpdate() 
    {
        MovePlayer();
    }

    void ReceiveInput()
    {
        inputs = moveAction.ReadValue<Vector2>();
    }

    void MovePlayer()
    {
        moveDirection = orientation.forward * inputs.y + orientation.right * inputs.x;

        rb.AddForce(moveDirection.normalized * moveSpeed * 100f * Time.deltaTime, ForceMode.Force);
    }

    void OnEnable()
    {
        moveAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
    }
}
