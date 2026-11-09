
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class CarlBot : MonoBehaviour
{
    public enum AIState
    {
        Wander,
        Chase,
        Search,
        Stunned,
        Dead
    }

    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthSlider;
    public GameObject healthBarObject;

    [Header("Stun")]
    public float stunDuration = 3f;
    public GameObject StunObj;
    public Slider stunSlider;
    public float startingFlashSpeed = 0.25f;
    public float fastestFlashSpeed = 0.04f;

    private bool hasBeenShot;
    private bool isStunned;
    private bool isDead;

    [Header("Player Attacking")]
    public float attackRange = 2.2f;
    public float attackDamage = 10f;
    public float attackInterval = 1.5f;
    public float firstAttackDelay = 1.5f;

    private float attackTimer;
    private bool hasAttacked;
    private PlayerHealthHunger playerHealth;

    [Header("Death Physics")]
    public Rigidbody bodyRigidbody;
    public float deathForce = 3f;
    public bool destroyOnDeath = true;
    public float destroyDelay = 10f;

    [Header("Detection")]
    public float detectionRange = 25f;
    public float losePlayerRange = 40f;
    public float fieldOfView = 120f;
    public LayerMask obstacleMask;
    public float eyeHeight = 1.5f;

    [Header("Movement")]
    public float wanderSpeed = 2f;
    public float chaseSpeed = 6f;
    public float searchSpeed = 3f;
    public float wanderRadius = 15f;

    [Header("Search")]
    public float searchRadius = 10f;
    public float searchDuration = 15f;
    public float searchWaitTime = 2f;

    private AIState currentState = AIState.Wander;
    private Vector3 lastKnownPosition;
    private float searchTimer;
    private float waitTimer;

    void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (bodyRigidbody == null)
            bodyRigidbody = GetComponent<Rigidbody>();

        if (bodyRigidbody != null)
        {
            bodyRigidbody.isKinematic = true;
            bodyRigidbody.useGravity = false;
        }

        if (StunObj != null)
            StunObj.SetActive(false);

        if (stunSlider != null)
            stunSlider.gameObject.SetActive(false);
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        StartWandering();

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealthHunger>();

            if (playerHealth == null)
                playerHealth = player.GetComponentInParent<PlayerHealthHunger>();
        }
    }

    void Update()
    {
        if (isDead || isStunned || player == null || agent == null)
            return;

        switch (currentState)
        {
            case AIState.Wander:
                LookForPlayer();
                Wander();
                break;

            case AIState.Chase:
                LookForPlayer();
                ChasePlayer();
                break;

            case AIState.Search:
                LookForPlayer();
                SearchArea();
                break;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isStunned)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();

        if (!hasBeenShot)
        {
            hasBeenShot = true;

            if (player != null)
                lastKnownPosition = player.position;

            StartCoroutine(FirstShotStun());
        }

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator FirstShotStun()
    {
        isStunned = true;
        currentState = AIState.Stunned;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (StunObj != null)
            StunObj.SetActive(true);

        if (stunSlider != null)
            stunSlider.gameObject.SetActive(true);

        float timer = 0f;
        float flashTimer = 0f;
        bool visible = true;

        if (stunSlider != null)
        {
            stunSlider.maxValue = stunDuration;
            stunSlider.value = stunDuration;
        }

        while (timer < stunDuration)
        {
            if (isDead)
                yield break;

            timer += Time.deltaTime;

            float remaining = Mathf.Clamp(
                stunDuration - timer, 0f, stunDuration);

            if (stunSlider != null)
                stunSlider.value = remaining;

            // Flash faster as the stun gets closer to ending
            float progress = timer / stunDuration;

            float flashSpeed = Mathf.Lerp(
                startingFlashSpeed,
                fastestFlashSpeed,
                progress
            );

            flashTimer += Time.deltaTime;

            if (flashTimer >= flashSpeed)
            {
                flashTimer = 0f;
                visible = !visible;

                if (StunObj != null)
                    StunObj.SetActive(visible);
            }

            yield return null;
        }

        if (StunObj != null)
            StunObj.SetActive(false);

        isStunned = false;

        if (stunSlider != null)
            stunSlider.gameObject.SetActive(false);

        if (player != null)
            lastKnownPosition = player.position;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;

            if (player != null)
            {
                currentState = AIState.Chase;
                agent.speed = chaseSpeed;
                agent.SetDestination(player.position);
            }
            else
            {
                currentState = AIState.Search;
                agent.speed = searchSpeed;
                agent.SetDestination(lastKnownPosition);
            }
        }
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthBarObject != null)
            healthBarObject.SetActive(currentHealth > 0);
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isStunned = false;
        currentState = AIState.Dead;

        StopAllCoroutines();

        if (StunObj != null)
            StunObj.SetActive(false);

        if (agent != null)
            agent.enabled = false;

        if (animator != null)
            animator.enabled = false;

        if (bodyRigidbody != null)
        {
            bodyRigidbody.isKinematic = false;
            bodyRigidbody.useGravity = true;

            Vector3 forceDirection =
                (transform.forward * -1f + Vector3.up * 0.8f).normalized;

            bodyRigidbody.AddForce(
                forceDirection * deathForce,
                ForceMode.Impulse
            );

            bodyRigidbody.AddTorque(
                Random.insideUnitSphere * deathForce,
                ForceMode.Impulse
            );
        }

        if (destroyOnDeath)
            Destroy(gameObject, destroyDelay);

        if (HoloObj != null)
            Destroy(HoloObj);
    }

    public GameObject HoloObj;

    bool CanSeePlayer()
    {
        float distance = Vector3.Distance(
            transform.position, player.position);

        if (distance > detectionRange)
            return false;

        Vector3 direction =
            (player.position - transform.position).normalized;

        float angle = Vector3.Angle(
            transform.forward, direction);

        if (angle > fieldOfView / 2f)
            return false;

        Vector3 eyePosition =
            transform.position + Vector3.up * eyeHeight;

        Vector3 targetPosition =
            player.position + Vector3.up;

        if (Physics.Linecast(
            eyePosition, targetPosition, obstacleMask))
            return false;

        return true;
    }

    void LookForPlayer()
    {
        if (CanSeePlayer())
        {
            lastKnownPosition = player.position;

            if (currentState != AIState.Chase)
            {
                currentState = AIState.Chase;
                agent.speed = chaseSpeed;
            }
        }
    }

    void Wander()
    {
        agent.speed = wanderSpeed;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= 3f)
                StartWandering();
        }
    }

    void StartWandering()
    {
        if (isDead || agent == null || !agent.enabled)
            return;

        currentState = AIState.Wander;
        waitTimer = 0f;

        Vector3 randomDirection =
            Random.insideUnitSphere * wanderRadius;

        randomDirection += transform.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.speed = wanderSpeed;
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    void ChasePlayer()
    {
        float distance = Vector3.Distance(
            transform.position, player.position);

        if (distance >= losePlayerRange)
        {
            StartSearching();
            return;
        }

        lastKnownPosition = player.position;

        // Attack when close enough
        if (distance <= attackRange)
        {
            agent.isStopped = true;

            attackTimer += Time.deltaTime;

            float requiredDelay = hasAttacked
                ? attackInterval
                : firstAttackDelay;

            if (attackTimer >= requiredDelay)
            {
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                }

                attackTimer = 0f;
                hasAttacked = true;
            }
        }
        else
        {
            // Reset attack when player moves away
            attackTimer = 0f;
            hasAttacked = false;

            agent.isStopped = false;
            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);
        }

        if (!CanSeePlayer())
            StartSearching();
    }

    void StartSearching()
    {
        currentState = AIState.Search;

        searchTimer = 0f;
        waitTimer = 0f;

        agent.speed = searchSpeed;
        agent.SetDestination(lastKnownPosition);
    }

    void SearchArea()
    {
        searchTimer += Time.deltaTime;

        if (searchTimer >= searchDuration)
        {
            StartWandering();
            return;
        }

        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.3f)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= searchWaitTime)
            {
                waitTimer = 0f;
                FindSearchPosition();
            }
        }
    }

    void FindSearchPosition()
    {
        Vector3 randomDirection =
            Random.insideUnitSphere * searchRadius;

        randomDirection += lastKnownPosition;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            randomDirection, out hit, searchRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
