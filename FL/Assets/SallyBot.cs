using UnityEngine.UI;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SallyBot : MonoBehaviour
{
    public enum AIState
    {
        Wander,
        Chase,
        Attack,
        RunAway
    }

    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;

    [Header("Detection")]
    public float detectionRange = 20f;
    public float fieldOfView = 120f;
    public float losePlayerRange = 35f;

    [Header("Wandering")]
    public float wanderRadius = 15f;
    public float wanderWaitTime = 2f;

    [Header("Attack")]
    public float attackRange = 2f;
    public float attackDamage = 15f;
    public float attackCooldown = 1.5f;
    public float attackDuration = 0.5f;

    [Header("Run Away")]
    public float runAwayDistance = 12f;
    public float runAwayDuration = 3f;
    public float runAwaySpeed = 7f;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public bool isDead = false;

    [Header("Health Bar")]
    public Slider healthSlider;
    public GameObject healthBarObject;
    public bool hideHealthBarWhenFull = true;

    [Header("Movement")]
    public float wanderSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Current State")]
    public AIState currentState = AIState.Wander;

    private float wanderTimer;
    private float attackTimer;
    private float runTimer;
    private bool attacking;
    private bool hasSeenPlayer;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        agent.speed = wanderSpeed;
        PickWanderPosition();

        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthBarObject != null && hideHealthBarWhenFull)
        {
            healthBarObject.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null || agent == null)
            return;

        float distance = Vector3.Distance(
            transform.position, player.position);

        switch (currentState)
        {
            case AIState.Wander:
                Wander();

                if (CanSeePlayer())
                {
                    hasSeenPlayer = true;
                    currentState = AIState.Chase;
                }
                break;

            case AIState.Chase:
                ChasePlayer(distance);
                break;

            case AIState.Attack:
                AttackPlayer(distance);
                break;

            case AIState.RunAway:
                RunAway();
                break;
        }
    }

    bool CanSeePlayer()
    {
        float distance = Vector3.Distance(
            transform.position, player.position);

        if (distance > detectionRange)
            return false;

        Vector3 direction =
            (player.position - transform.position).normalized;

        float angle = Vector3.Angle(transform.forward, direction);

        if (angle > fieldOfView * 0.5f)
            return false;

        if (Physics.Raycast(
            transform.position + Vector3.up,
            direction, out RaycastHit hit, detectionRange))
        {
            return hit.transform == player ||
                   hit.transform.IsChildOf(player);
        }

        return false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Show health bar when damaged.
        if (healthBarObject != null)
            healthBarObject.SetActive(true);

        // Update slider.
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        agent.isStopped = true;
        agent.ResetPath();

        StopAllCoroutines();

        if (healthBarObject != null)
            healthBarObject.SetActive(false);

        Destroy(gameObject, 2f);
    }

    void Wander()
    {
        agent.speed = wanderSpeed;
        wanderTimer += Time.deltaTime;

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            if (wanderTimer >= wanderWaitTime)
            {
                PickWanderPosition();
                wanderTimer = 0f;
            }
        }
    }

    void PickWanderPosition()
    {
        Vector3 randomDirection =
            Random.insideUnitSphere * wanderRadius;

        randomDirection += transform.position;

        if (NavMesh.SamplePosition(
            randomDirection, out NavMeshHit hit,
            wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void ChasePlayer(float distance)
    {
        agent.speed = chaseSpeed;
        agent.isStopped = false;

        if (distance > losePlayerRange && !CanSeePlayer())
        {
            currentState = AIState.Wander;
            PickWanderPosition();
            return;
        }

        if (distance <= attackRange)
        {
            currentState = AIState.Attack;
            attackTimer = 0f;
            return;
        }

        agent.SetDestination(player.position);
    }

    void AttackPlayer(float distance)
    {
        // Keep moving toward the player if not close enough.
        agent.isStopped = false;
        agent.speed = chaseSpeed;

        if (distance > attackRange + 0.5f)
        {
            agent.SetDestination(player.position);
            return;
        }

        agent.ResetPath();

        // Face the player.
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion rotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotation,
                10f * Time.deltaTime);
        }

        attackTimer += Time.deltaTime;

        if (!attacking && attackTimer >= attackCooldown)
        {
            StartCoroutine(PerformAttack());
        }
    }

    IEnumerator PerformAttack()
    {
        attacking = true;

        // Attack animation / hit delay.
        yield return new WaitForSeconds(attackDuration);

        float distance = Vector3.Distance(
            transform.position, player.position);

        // Only deal damage if still close enough.
        if (distance <= attackRange + 0.5f)
        {
            PlayerHealthHunger health =
                player.GetComponent<PlayerHealthHunger>();

            if (health == null)
                health = player.GetComponentInParent<PlayerHealthHunger>();

            if (health != null)
            {
                health.TakeDamage(attackDamage);
            }
        }

        attacking = false;
        attackTimer = 0f;

        // Run away after attacking.
        currentState = AIState.RunAway;
        runTimer = 0f;
    }

    void RunAway()
    {
        agent.isStopped = false;
        agent.speed = runAwaySpeed;

        runTimer += Time.deltaTime;

        Vector3 awayDirection =
            (transform.position - player.position).normalized;

        Vector3 runPosition =
            transform.position + awayDirection * runAwayDistance;

        if (NavMesh.SamplePosition(
            runPosition, out NavMeshHit hit,
            runAwayDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        if (runTimer >= runAwayDuration)
        {
            currentState = AIState.Chase;
        }
    }
}
