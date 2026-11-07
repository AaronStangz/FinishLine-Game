
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 50f;
    public float maxRange = 100f;
    public float damage = 25f;
    public float lifeTime = 5f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        float distance = speed * Time.deltaTime;

        if (Physics.Raycast(transform.position, transform.forward,
            out RaycastHit hit, distance))
        {
            // Apply damage if the object has a health component
            TargetHealth health = hit.collider.GetComponent<TargetHealth>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        transform.position += transform.forward * distance;

        // Destroy bullet when maximum range is reached
        if (Vector3.Distance(startPosition, transform.position) >= maxRange)
        {
            Destroy(gameObject);
        }
    }
}
