
using UnityEngine;

public class Knife : MonoBehaviour
{
    [Header("Knife Settings")]
    public float damage = 35f;
    public float attackRange = 2.5f;
    public float attackCooldown = 0.8f;

    [Header("Headshot Settings")]
    public float headshotMultiplier = 3f;
    public string headTag = "Head";

    [Header("Aim Settings")]
    public Camera playerCamera;
    public LayerMask hitLayers = ~0;

    [Header("Attack Settings")]
    public KeyCode attackKey = KeyCode.Mouse0;
    public float attackDelay = 0.1f;

    private float nextAttackTime;
    private bool isAttacking;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(attackKey))
        {
            Attack();
        }
    }

    void Attack()
    {
        if (Time.time < nextAttackTime || isAttacking)
            return;

        nextAttackTime = Time.time + attackCooldown;

        if (playerCamera == null)
            return;

        isAttacking = true;

        Invoke(nameof(PerformAttack), attackDelay);
        Invoke(nameof(ResetAttack), attackDelay + 0.15f);
    }

    void PerformAttack()
    {
        if (playerCamera == null)
            return;

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(ray, out RaycastHit hit,
            attackRange, hitLayers))
        {
            float finalDamage = damage;

            // Headshot damage
            if (hit.collider.CompareTag(headTag))
            {
                finalDamage *= headshotMultiplier;
            }

            // SallyBot
            SallyBot sally =
                hit.collider.GetComponentInParent<SallyBot>();

            if (sally != null)
            {
                sally.TakeDamage(finalDamage);
                return;
            }

            // CarlBot
            CarlBot carl =
                hit.collider.GetComponentInParent<CarlBot>();

            if (carl != null)
            {
                carl.TakeDamage(finalDamage);
                return;
            }

            // Other targets
            TargetHealth health =
                hit.collider.GetComponentInParent<TargetHealth>();

            if (health != null)
            {
                health.TakeDamage(finalDamage);
            }
        }
    }

    void ResetAttack()
    {
        isAttacking = false;
    }
}
