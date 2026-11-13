
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 50f;
    public float maxRange = 100f;
    public float damage = 25f;
    public float lifeTime = 5f;

    [Header("Headshot Settings")]
    public float headshotMultiplier = 3f;
    public string headTag = "Head";

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
                new Vector3(0.5f, 0.5f, 0f));

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
            float finalDamage = damage;

            // Check for headshot
            if (hit.collider.CompareTag(headTag))
            {
                finalDamage *= headshotMultiplier;
            }

            // SallyBot
            SallyBot sally = hit.collider.GetComponentInParent<SallyBot>();

            if (sally != null)
            {
                sally.TakeDamage(finalDamage);
            }
            else
            {
                // CarlBot
                CarlBot bot = hit.collider.GetComponentInParent<CarlBot>();

                if (bot != null)
                {
                    bot.HitBodyPart(hit.collider);

                    if (!hit.collider.CompareTag("InstantDeath"))
                    {
                        bot.TakeDamage(25f);
                    }
                }
            }

            Destroy(gameObject);
            return;
        }

        transform.position += direction * distance;

        if (Vector3.Distance(startPosition, transform.position) >= maxRange)
            Destroy(gameObject);
    }
}
