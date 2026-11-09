
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 50f;
    public float maxRange = 100f;
    public float damage = 25f;
    public float lifeTime = 5f;

    [Header("Aim Settings")]
    public Camera playerCamera;
    public float aimDistance = 200f;

    private Vector3 startPosition;
    private Vector3 direction;

    void Start()
    {
        startPosition = transform.position;

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera != null)
        {
            Ray ray = playerCamera.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f)
            );

            Vector3 targetPoint;

            if (Physics.Raycast(ray, out RaycastHit cameraHit, aimDistance))
                targetPoint = cameraHit.point;
            else
                targetPoint = ray.GetPoint(aimDistance);

            direction = (targetPoint - transform.position).normalized;
        }
        else
        {
            direction = transform.forward;
        }

        transform.forward = direction;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        float distance = speed * Time.deltaTime;

        if (Physics.Raycast(transform.position, direction,
            out RaycastHit hit, distance))
        {
            CarlBot bot = hit.collider.GetComponentInParent<CarlBot>();

            if (bot != null)
            {
                bot.TakeDamage(damage);
            }
            else
            {
                TargetHealth health =
                    hit.collider.GetComponentInParent<TargetHealth>();

                if (health != null)
                    health.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        transform.position += direction * distance;

        if (Vector3.Distance(startPosition, transform.position) >= maxRange)
        {
            Destroy(gameObject);
        }
    }
}
