
using UnityEngine;

public class SlidingDoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform leftDoor;
    public Transform rightDoor;

    public float openDistance = 2f;
    public float openSpeed = 2f;

    [Header("Button Settings")]
    public bool button1Pressed;
    public bool button2Pressed;

    [Header("Positions")]
    private Vector3 leftClosedPosition;
    private Vector3 rightClosedPosition;

    private Vector3 leftOpenPosition;
    private Vector3 rightOpenPosition;

    void Start()
    {
        leftClosedPosition = leftDoor.localPosition;
        rightClosedPosition = rightDoor.localPosition;

        leftOpenPosition = leftClosedPosition + Vector3.left * openDistance;
        rightOpenPosition = rightClosedPosition + Vector3.right * openDistance;
    }

    void Update()
    {
        bool shouldOpen = button1Pressed && button2Pressed;

        Vector3 targetLeft = shouldOpen ? leftOpenPosition : leftClosedPosition;
        Vector3 targetRight = shouldOpen ? rightOpenPosition : rightClosedPosition;

        leftDoor.localPosition = Vector3.MoveTowards(
            leftDoor.localPosition,
            targetLeft,
            openSpeed * Time.deltaTime
        );

        rightDoor.localPosition = Vector3.MoveTowards(
            rightDoor.localPosition,
            targetRight,
            openSpeed * Time.deltaTime
        );
    }

    public void PressButton1()
    {
        button1Pressed = true;
    }

    public void ReleaseButton1()
    {
        button1Pressed = false;
    }

    public void PressButton2()
    {
        button2Pressed = true;
    }

    public void ReleaseButton2()
    {
        button2Pressed = false;
    }
}
