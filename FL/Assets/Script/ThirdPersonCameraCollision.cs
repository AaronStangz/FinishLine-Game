
using UnityEngine;

[DefaultExecutionOrder(100)]
public class PlayerCameraCollision : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;

    [Header("Collision Settings")]
    public float cameraRadius = 0.25f;
    public float wallPadding = 0.15f;
    public float minimumDistance = 0.3f;

    [Header("Smoothing")]
    public float moveInSpeed = 20f;
    public float moveOutSpeed = 8f;

    [Header("Collision Layers")]
    public LayerMask collisionLayers = ~0;

    private PlayerLook playerLook;
    private float currentDistance;

    void Start()
    {
        playerLook = GetComponent<PlayerLook>();

        if (playerBody == null && playerLook != null)
            playerBody = playerLook.playerBody;

        if (playerLook != null)
            currentDistance = playerLook.cameraDistance;
    }

    void LateUpdate()
    {
        if (playerBody == null || playerLook == null)
            return;

        // Get the camera's desired position from PlayerLook
        Vector3 desiredPosition = playerBody.position
            + playerBody.up * playerLook.cameraHeight
            + playerBody.right * (
                playerLook.shoulderDistance *
                (transform.localPosition.x >= 0 ? 1f : -1f)
            )
            - transform.forward * 0f;

        // Use the camera's actual rotation and shoulder position
        Vector3 cameraDirection = transform.rotation * Vector3.back;

        Vector3 shoulderPosition = playerBody.position
            + playerBody.up * playerLook.cameraHeight
            + playerBody.right * transform.localPosition.x;

        float desiredDistance = playerLook.cameraDistance;

        // Sphere cast to detect walls
        if (Physics.SphereCast(
            shoulderPosition,
            cameraRadius,
            cameraDirection,
            out RaycastHit hit,
            desiredDistance,
            collisionLayers,
            QueryTriggerInteraction.Ignore))
        {
            desiredDistance = Mathf.Max(
                minimumDistance,
                hit.distance - wallPadding
            );
        }

        // Smoothly move camera closer or farther away
        float speed = desiredDistance < currentDistance
            ? moveInSpeed
            : moveOutSpeed;

        currentDistance = Mathf.Lerp(
            currentDistance,
            desiredDistance,
            speed * Time.deltaTime
        );

        // Keep camera at the collision-safe position
        transform.position = shoulderPosition
            + cameraDirection * currentDistance;
    }
}
