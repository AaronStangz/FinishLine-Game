
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float crouchSpeed = 2.5f;
    public float jumpHeight = 2f;
    public float gravity = -20f;

    [Header("Crouching")]
    public float standingHeight = 2f;
    public float crouchingHeight = 1f;

    [Header("Step Up")]
    public float stepHeight = 0.4f;
    public float stepSmooth = 0.1f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isCrouching;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    private bool isGrounded;


    void Start()
    {
        controller = GetComponent<CharacterController>();

        controller.height = standingHeight;
        controller.center = new Vector3(0, standingHeight / 2f, 0);
    }

    void Update()
    {
        CheckGround();
        Move();
        Jump();
        Crouch();
        ApplyGravity();
        StepClimb();
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x +
                       transform.forward * z;

        float speed = walkSpeed;

        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching)
        {
            speed = runSpeed;
        }

        if (isCrouching)
        {
            speed = crouchSpeed;
        }

        controller.Move(move.normalized * speed * Time.deltaTime);
    }
    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }

    void Jump()
    {
        if (isGrounded && Input.GetButtonDown("Jump")
            && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isGrounded = false;
        }
    }

    void Crouch()
    {
        isCrouching = Input.GetKey(KeyCode.LeftControl);

        float targetHeight = isCrouching
            ? crouchingHeight
            : standingHeight;

        controller.height = Mathf.Lerp(
            controller.height,
            targetHeight,
            Time.deltaTime * 10f
        );

        controller.center = new Vector3(
            0,
            controller.height / 2f,
            0
        );
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void StepClimb()
    {
        if (!controller.isGrounded)
            return;

        Vector3 direction = new Vector3(
            Input.GetAxis("Horizontal"),
            0,
            Input.GetAxis("Vertical")
        );

        if (direction.magnitude < 0.1f)
            return;

        direction = transform.TransformDirection(direction.normalized);

        Vector3 lowerRay = transform.position +
                           Vector3.up * 0.1f;

        Vector3 upperRay = transform.position +
                           Vector3.up * stepHeight;

        if (!Physics.Raycast(lowerRay, direction, 0.5f))
        {
            if (Physics.Raycast(upperRay, direction, 0.5f))
                return;

            if (Physics.Raycast(
                upperRay + direction * 0.5f,
                Vector3.down,
                out RaycastHit hit,
                stepHeight
            ))
            {
                float difference = hit.point.y - transform.position.y;

                if (difference > 0 && difference <= stepHeight)
                {
                    controller.Move(
                        Vector3.up * stepSmooth
                    );
                }
            }
        }
    }
}
