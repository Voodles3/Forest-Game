using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 1f;

    Rigidbody rb;
    Vector2 moveDirection;
    public InputActionReference moveAction;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        moveDirection = moveAction.action.ReadValue<Vector2>();
    }

    void FixedUpdate() 
    {
        float xMove = moveDirection.x * moveSpeed * Time.deltaTime * 100f;
        float zMove = moveDirection.y * moveSpeed * Time.deltaTime * 100f;
        rb.AddRelativeForce(xMove, 0, zMove);
    }
}
