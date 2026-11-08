
using UnityEngine;
using UnityEngine.AI;

public class CarlBot : MonoBehaviour
{
    public enum AIState
    {
        Wander,
        Chase,
        Search
    }

    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;

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

    private bool reachedSearchPoint;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        StartWandering();
    }

    void Update()
    {
        if (player == null || agent == null)
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

        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        if (!CanSeePlayer())
        {
            StartSearching();
        }
    }

    void StartSearching()
    {
        currentState = AIState.Search;

        searchTimer = 0f;
        waitTimer = 0f;
        reachedSearchPoint = false;

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
