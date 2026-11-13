using UnityEngine;

public class MassBasedPickupSway : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRange = 3f;
    public Transform holdPoint;
    public LayerMask pickupLayer;

    [Header("Movement")]
    public float baseMoveSpeed = 10f;
    public float forceMultiplier = 50f;

    [Header("Held Rotation")]
    public float rotateSpeed = 120f;

    [Header("Sway")]
    public float swayAmount = 0.05f;
    public float swaySmooth = 6f;

    [Header("Throw Charge")]
    public float maxThrowForce = 25f;
    public float chargeSpeed = 15f;

    private float throwCharge;

    [Header("Zoom")]
    public Camera playerCamera;
    public float normalFOV = 60f;
    public float zoomFOV = 40f;
    public float zoomSpeed = 8f;

    private Rigidbody heldRb;
    private Vector3 swayOffset;
    private Vector3 swayVelocity;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldRb == null)
            {
                TryPickup();
            }
            else
            {
                Drop();
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (heldRb != null)
            {
                throwCharge += Time.deltaTime * chargeSpeed;
                throwCharge = Mathf.Clamp(throwCharge, 0f, 1f);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (heldRb != null)
                ThrowObject();
        }

        // zoom (if you already added it)
        if (Input.GetMouseButton(1))
        {
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                zoomFOV,
                Time.deltaTime * zoomSpeed
            );
        }
        else
        {
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                normalFOV,
                Time.deltaTime * zoomSpeed
            );
        }
    }

    void FixedUpdate()
    {
        if (heldRb != null)
        {
            MoveHeldObject();
        }

        if (heldRb != null)
        {
            // R = Y axis (left/right)
            if (Input.GetKey(KeyCode.R))
            {
                heldRb.MoveRotation(
                    heldRb.rotation * Quaternion.Euler(0f, rotateSpeed * Time.deltaTime, 0f)
                );
            }

            // Q = X axis (up/down)
            if (Input.GetKey(KeyCode.Q))
            {
                heldRb.MoveRotation(
                    heldRb.rotation * Quaternion.Euler(-rotateSpeed * Time.deltaTime, 0f, 0f)
                );
            }
        }
    }

    void TryPickup()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupLayer))
        {
            Rigidbody rb = hit.rigidbody;
            if (rb != null)
            {
                heldRb = rb;
                heldRb.useGravity = false;
                heldRb.linearDamping = 10f;
            }
        }
    }

    void Drop()
    {
        if (heldRb == null) return;

        heldRb.useGravity = true;
        heldRb.linearDamping = 0f;
        heldRb = null;
    }

    void ThrowObject()
    {
        if (heldRb == null) return;

        heldRb.useGravity = true;
        heldRb.linearDamping = 0f;

        float mass = Mathf.Max(heldRb.mass, 0.1f);

        // charge determines force
        float force = Mathf.Lerp(5f, maxThrowForce, throwCharge);

        // heavier = shorter throw
        force /= (1f + mass * 0.5f);

        Vector3 throwDir = transform.forward;

        heldRb.linearVelocity = Vector3.zero;
        heldRb.AddForce(throwDir * force, ForceMode.Impulse);

        heldRb = null;
        throwCharge = 0f;
    }

    void MoveHeldObject()
    {
        float mass = Mathf.Max(heldRb.mass, 0.1f);

        // Heavier = slower response
        float speed = baseMoveSpeed / mass;

        Vector3 targetPos = holdPoint.position;

        // --- CENTERING (strong pull to hold point) ---
        Vector3 toTarget = targetPos - heldRb.position;

        // stronger "snap to center"
        Vector3 springForce = toTarget * (speed * 10f);

        // damping to stop overshooting / wobble
        Vector3 damping = -heldRb.linearVelocity * (6f * mass);

        // --- SWAY (reduced when heavy) ---
        Vector3 inputSway = new Vector3(
            Mathf.Sin(Time.time * 2f),
            Mathf.Cos(Time.time * 2f),
            0f
        ) * (swayAmount / mass);

        Vector3 finalForce = springForce + damping + inputSway;

        heldRb.AddForce(finalForce, ForceMode.Force);
    }
}