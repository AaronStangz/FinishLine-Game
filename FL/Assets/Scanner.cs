
using UnityEngine;
using UnityEngine.InputSystem;

public class Scanner : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 15f;
    public float scanRange = 40f;

    [Header("Tags")]
    public string enemyTag = "Enemy";
    public string lootTag = "Loot";

    [Header("Scanning")]
    public Key scanKey = Key.V;
    public float scanCooldown = 5f;

    private float nextScanTime;

    void Update()
    {
        // Automatic detection
        Collider[] nearby = Physics.OverlapSphere(
            transform.position,
            detectionRange
        );

        foreach (Collider target in nearby)
        {
            FindAndActivate(target);
        }

        // Manual scan
        if (Keyboard.current != null &&
            Keyboard.current[scanKey].wasPressedThisFrame &&
            Time.time >= nextScanTime)
        {
            ScanArea();
        }
    }

    void ScanArea()
    {
        nextScanTime = Time.time + scanCooldown;

        Collider[] targets = Physics.OverlapSphere(
            transform.position,
            scanRange
        );

        foreach (Collider target in targets)
        {
            FindAndActivate(target);
        }
    }

    void FindAndActivate(Collider target)
    {
        if (!target.CompareTag(enemyTag) &&
            !target.CompareTag(lootTag))
            return;

        ObjectHighlight highlight =
            target.GetComponentInParent<ObjectHighlight>();

        if (highlight == null)
            highlight = target.GetComponent<ObjectHighlight>();

        if (highlight != null)
        {
            highlight.ActivateHighlight();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, scanRange);
    }
}
