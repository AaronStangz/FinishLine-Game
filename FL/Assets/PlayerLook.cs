
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 150f;
    public float minLookAngle = -80f;
    public float maxLookAngle = 80f;

    [Header("Shoulder Switching")]
    public float shoulderDistance = 0.6f;
    public float switchSpeed = 5f;
    public float switchThreshold = 0.1f;

    [Header("Camera Distance")]
    public float cameraDistance = 3f;
    public float cameraHeight = 1.6f;

    [Header("Camera Sway")]
    public float swayAmount = 5f;
    public float swaySpeed = 5f;

    [Header("Crosshair Settings")]
    public RectTransform crosshair;
    public float crosshairOffset = 40f;
    public float crosshairMoveSpeed = 5f;

    private Vector2 crosshairStartPosition;

    public Transform playerBody;

    private float xRotation;
    private float currentSide = 1f;
    private float targetSide = 1f;
    private float currentSway;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshair != null)
        {
            crosshairStartPosition = crosshair.anchoredPosition;
        }
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Mouse look
        xRotation -= mouseY * mouseSensitivity * Time.deltaTime;

        xRotation = Mathf.Clamp(
            xRotation,
            minLookAngle,
            maxLookAngle
        );

        // Rotate player
        playerBody.Rotate(
            Vector3.up * mouseX * mouseSensitivity * Time.deltaTime
        );

        // Switch shoulders
        if (mouseX > switchThreshold)
            targetSide = 1f;
        else if (mouseX < -switchThreshold)
            targetSide = -1f;

        currentSide = Mathf.Lerp(
            currentSide,
            targetSide,
            Time.deltaTime * switchSpeed
        );

        // Camera sway
        float targetSway = -mouseX * swayAmount;

        currentSway = Mathf.Lerp(
            currentSway,
            targetSway,
            Time.deltaTime * swaySpeed
        );

        // Camera position
        Vector3 targetPosition = new Vector3(
            currentSide * shoulderDistance,
            cameraHeight,
            -cameraDistance
        );

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * switchSpeed
        );

        // Camera rotation
        transform.localRotation = Quaternion.Euler(
            xRotation,
            0f,
            currentSway
        );

        // Move crosshair with shoulder switching
        if (crosshair != null)
        {
            Vector2 targetCrosshairPosition = crosshairStartPosition;

            targetCrosshairPosition.x += -currentSide * crosshairOffset;

            crosshair.anchoredPosition = Vector2.Lerp(
                crosshair.anchoredPosition,
                targetCrosshairPosition,
                Time.deltaTime * crosshairMoveSpeed
            );
        }
    }
}
