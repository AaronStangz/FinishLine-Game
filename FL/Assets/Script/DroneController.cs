
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float verticalSpeed = 5f;

    [Header("Rotation")]
    public float stabilizationSpeed = 5f;

    [Header("Flight Boundary")]
    public BoxCollider flightZone;
    public float boundaryPadding = 0.5f;

    private Rigidbody rb;
    private Vector2 movementInput;
    private float verticalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.linearDamping = 2f;
        rb.angularDamping = 4f;
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        movementInput = Vector2.zero;
        verticalInput = 0f;

        if (Keyboard.current.wKey.isPressed) movementInput.y = 1;
        if (Keyboard.current.sKey.isPressed) movementInput.y = -1;
        if (Keyboard.current.aKey.isPressed) movementInput.x = -1;
        if (Keyboard.current.dKey.isPressed) movementInput.x = 1;

        if (Keyboard.current.spaceKey.isPressed) verticalInput = 1;
        if (Keyboard.current.leftCtrlKey.isPressed) verticalInput = -1;
    }

    void FixedUpdate()
    {
        if (playerCamera == null) return;

        // Camera-relative movement
        Vector3 forward = playerCamera.forward;
        Vector3 right = playerCamera.right;

        Vector3 moveDirection =
            forward * movementInput.y +
            right * movementInput.x;

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        Vector3 velocity =
            moveDirection * moveSpeed +
            Vector3.up * verticalInput * verticalSpeed;

        // Flight boundary
        if (flightZone != null)
        {
            Bounds bounds = flightZone.bounds;

            Vector3 min = bounds.min + Vector3.one * boundaryPadding;
            Vector3 max = bounds.max - Vector3.one * boundaryPadding;

            Vector3 nextPosition =
                rb.position + velocity * Time.fixedDeltaTime;

            Vector3 clampedPosition = new Vector3(
                Mathf.Clamp(nextPosition.x, min.x, max.x),
                Mathf.Clamp(nextPosition.y, min.y, max.y),
                Mathf.Clamp(nextPosition.z, min.z, max.z)
            );

            velocity = (clampedPosition - rb.position) / Time.fixedDeltaTime;
        }

        rb.linearVelocity = velocity;

        // Follow camera horizontal rotation without tilting
        Vector3 cameraDirection = playerCamera.forward;
        cameraDirection.y = 0f;

        if (cameraDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(cameraDirection);

            rb.MoveRotation(
                Quaternion.Slerp(
                    rb.rotation,
                    targetRotation,
                    stabilizationSpeed * Time.fixedDeltaTime
                )
            );
        }
    }
}
