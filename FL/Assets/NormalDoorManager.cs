using UnityEngine;
using UnityEngine.InputSystem;

public class NormalDoorManager : MonoBehaviour
{
    [Header("Door Panels")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 5f;

    [Header("Interaction")]
    public float interactDistance = 3f;

    private Quaternion leftClosedRotation;
    private Quaternion rightClosedRotation;

    private Quaternion leftTargetRotation;
    private Quaternion rightTargetRotation;

    private bool isOpen = false;

    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;

        leftClosedRotation = leftDoor.localRotation;
        rightClosedRotation = rightDoor.localRotation;

        leftTargetRotation = leftClosedRotation;
        rightTargetRotation = rightClosedRotation;
    }

    void Update()
    {
        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.transform == leftDoor || hit.transform == rightDoor)
            {
                if (Keyboard.current != null &&
                    Keyboard.current.fKey.wasPressedThisFrame)
                {
                    ToggleDoor();
                }
            }
        }

        // Smooth opening/closing
        leftDoor.localRotation = Quaternion.Slerp(
            leftDoor.localRotation,
            leftTargetRotation,
            openSpeed * Time.deltaTime
        );

        rightDoor.localRotation = Quaternion.Slerp(
            rightDoor.localRotation,
            rightTargetRotation,
            openSpeed * Time.deltaTime
        );
    }

    void ToggleDoor()
    {
        isOpen = !isOpen;

        if (!isOpen)
        {
            leftTargetRotation = leftClosedRotation;
            rightTargetRotation = rightClosedRotation;
            return;
        }

        // Get player position relative to the door
        Vector3 playerPosition = playerCamera.transform.position;
        Vector3 doorPosition = transform.position;

        Vector3 directionToPlayer = playerPosition - doorPosition;
        directionToPlayer.y = 0f;

        // Determine which side of the door the player is on
        float side = Vector3.Dot(
            transform.forward,
            directionToPlayer.normalized
        );

        // IMPORTANT:
        // Change these two numbers if your door's hinges are oriented differently.
        if (side > 0)
        {
            // Player is in front
            leftTargetRotation =
                leftClosedRotation *
                Quaternion.Euler(0f, -openAngle, 0f);

            rightTargetRotation =
                rightClosedRotation *
                Quaternion.Euler(0f, openAngle, 0f);
        }
        else
        {
            // Player is behind
            leftTargetRotation =
                leftClosedRotation *
                Quaternion.Euler(0f, openAngle, 0f);

            rightTargetRotation =
                rightClosedRotation *
                Quaternion.Euler(0f, -openAngle, 0f);
        }
    }
}