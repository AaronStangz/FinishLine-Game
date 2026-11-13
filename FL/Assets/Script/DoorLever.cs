using UnityEngine;

public class DoorLever : MonoBehaviour
{
    [Header("Door")]
    public DoubleDoor doubleDoor;

    [Header("Lever")]
    public Transform lever;
    public Vector3 pulledRotation = new Vector3(-45, 0, 0);
    public float leverSpeed = 5f;

    [Header("Settings")]
    public KeyCode pullKey = KeyCode.E;
    public bool playerInRange;
    public bool hasBeenPulled;

    private Quaternion originalRotation;
    private Quaternion targetRotation;

    void Start()
    {
        originalRotation = lever.localRotation;
        targetRotation = originalRotation;
    }

    void Update()
    {
        lever.localRotation = Quaternion.Slerp(
            lever.localRotation,
            targetRotation,
            Time.deltaTime * leverSpeed
        );

        if (playerInRange && !hasBeenPulled && Input.GetKeyDown(pullKey))
        {
            PullLever();
        }
    }

    void PullLever()
    {
        hasBeenPulled = true;

        targetRotation = originalRotation * Quaternion.Euler(pulledRotation);

        if (doubleDoor != null)
        {
            doubleDoor.OpenDoors();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}