
using UnityEngine;
using UnityEngine.InputSystem;

public class DroneCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public float mouseSensitivity = 150f;
    public float minLookAngle = -85f;
    public float maxLookAngle = 85f;

    [Header("References")]
    public Transform drone;

    private float xRotation;
    private float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (drone != null)
            yRotation = drone.eulerAngles.y;

        xRotation = transform.localEulerAngles.x;

        if (xRotation > 180f)
            xRotation -= 360f;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        float mouseX = Mouse.current.delta.x.ReadValue()
            * mouseSensitivity * Time.deltaTime;

        float mouseY = Mouse.current.delta.y.ReadValue()
            * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);

        // Rotate camera independently of drone
        transform.rotation = Quaternion.Euler(
            xRotation,
            yRotation,
            0f
        );
    }

}
