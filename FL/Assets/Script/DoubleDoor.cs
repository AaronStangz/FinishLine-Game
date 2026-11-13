using UnityEngine;
using System.Collections;

public class DoubleDoor : MonoBehaviour
{
    [Header("Doors")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Open Rotations")]
    public Vector3 leftOpenRotation = new Vector3(0, -90, 0);
    public Vector3 rightOpenRotation = new Vector3(0, 90, 0);

    [Header("Settings")]
    public float openSpeed = 2f;
    public bool isOpen;

    private Quaternion leftClosedRotation;
    private Quaternion rightClosedRotation;

    void Start()
    {
        leftClosedRotation = leftDoor.localRotation;
        rightClosedRotation = rightDoor.localRotation;
    }

    public void OpenDoors()
    {
        if (isOpen) return;

        isOpen = true;
        StopAllCoroutines();
        StartCoroutine(OpenDoorAnimation());
    }

    IEnumerator OpenDoorAnimation()
    {
        Quaternion leftTarget = leftClosedRotation * Quaternion.Euler(leftOpenRotation);
        Quaternion rightTarget = rightClosedRotation * Quaternion.Euler(rightOpenRotation);

        while (Quaternion.Angle(leftDoor.localRotation, leftTarget) > 0.1f ||
               Quaternion.Angle(rightDoor.localRotation, rightTarget) > 0.1f)
        {
            leftDoor.localRotation = Quaternion.Slerp(
                leftDoor.localRotation, leftTarget, Time.deltaTime * openSpeed);

            rightDoor.localRotation = Quaternion.Slerp(
                rightDoor.localRotation, rightTarget, Time.deltaTime * openSpeed);

            yield return null;
        }

        leftDoor.localRotation = leftTarget;
        rightDoor.localRotation = rightTarget;
    }
}