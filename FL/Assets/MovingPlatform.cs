
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement")]
    public Transform endPosition;
    public float moveSpeed = 2f;

    private Vector3 startPosition;
    private bool activated;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        Vector3 target = activated
            ? endPosition.position
            : startPosition;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );
    }

    public void ActivatePlatform()
    {
        activated = true;
    }

    public void DeactivatePlatform()
    {
        activated = false;
    }
}
