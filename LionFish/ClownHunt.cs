using UnityEngine;
using UnityEngine.AI; 
using System;

public class ClownHunt : MonoBehaviour 
{
    [NonSerialized] public GameObject goal;
    public bool HasGoal => goal != null;
    private NavMeshAgent agent;

    [Header("Hunt Movement")]
    public float huntSpeed = 5f;
    public float chaseSpeed = 7f;
    public float arrivalDistance = 0.5f;

    private float scanTimer = 0f;
    private float defaultSpeed;

    [Header("Hunt Boundaries")]
    public float minX = 26f;
    public float maxX = 71f;
    public float minZ = -8f;
    public float maxZ = 81f;

    [Header("Vision Settings")]
    public float visionRange = 10f;   
    public int numberOfRays = 10;      
    public float visionAngle = 90f;
    public float goalSampleRadius = 8f;

    void Start() 
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null) defaultSpeed = agent.speed;
    }

    void OnEnable()
    {
        if (agent != null)
        {
            agent.speed = huntSpeed;
            agent.isStopped = false;
            agent.ResetPath();
        }
        goal = null;
    }

    void OnDisable()
    {
        if (agent != null)
        {
            agent.speed = defaultSpeed;
        }
    }

    private void Update() 
    {
        if (agent == null || !agent.isOnNavMesh) return;

        // 1. SCANNING PHASE (Looking for food)
        if (goal == null)
        {
            scanTimer -= Time.deltaTime;
            if (scanTimer <= 0f)
            {
                ScanForClownfish();
                scanTimer = 0.1f; 
            }
            agent.speed = huntSpeed;
        }

        // 2. CHASING PHASE (Target acquired)
        if (goal != null)
        {
            ClownFish clownFish = goal.GetComponent<ClownFish>();
            
            // If target fish is gone, safe, or home, give up the chase cleanly
            if (clownFish == null || clownFish.IsSafe || clownFish.IsAtHome)
            {
                if (clownFish != null) clownFish.SetBeingChased(false);
                goal = null;
                agent.ResetPath();
                return; 
            }

            agent.speed = chaseSpeed;

            // SMART FOLLOW LOGIC: Calculate how far the clownfish has moved 
            // away from the destination point the agent is CURRENTLY swimming toward.
            float targetDistanceFromCurrentPathEnd = Vector3.Distance(agent.destination, goal.transform.position);

            // Only issue a new SetDestination if the clownfish breaks a 1.5-meter threshold
            // and the agent isn't already busy processing a path calculation.
            if (targetDistanceFromCurrentPathEnd > 1.5f && !agent.pathPending)
            {
                if (NavMesh.SamplePosition(goal.transform.position, out NavMeshHit navHit, goalSampleRadius, agent.areaMask))
                {
                    agent.SetDestination(navHit.position);
                }
            }
            return; // Stay locked in chase mode execution
        }

        // 3. ROAMING PHASE (Standard wandering)
        if (goal == null && !agent.pathPending && agent.remainingDistance < arrivalDistance)
        {
            float randomX = UnityEngine.Random.Range(minX, maxX);
            float randomZ = UnityEngine.Random.Range(minZ, maxZ);
            Vector3 randomSpot = new Vector3(randomX, transform.position.y, randomZ);

            if (NavMesh.SamplePosition(randomSpot, out NavMeshHit hit, 10f, agent.areaMask))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
            }
        }
    }

    void ScanForClownfish()
    {
        float startingAngle = -visionAngle / 2f;
        float angleStep = visionAngle / (numberOfRays - 1);
        float laserThickness = 0.5f; 

        Vector3 rayOrigin = transform.position - (transform.up * 0.6f);
        for (int i = 0; i < numberOfRays; i++)
        {
            float currentAngle = startingAngle + (angleStep * i);
            Vector3 rayDirection = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            Debug.DrawRay(rayOrigin, rayDirection * visionRange, Color.cyan);

            if (Physics.SphereCast(rayOrigin, laserThickness, rayDirection, out RaycastHit hit, visionRange))
            {
                if (hit.collider.CompareTag("ClownFish"))
                {
                    ClownFish clownFish = hit.collider.GetComponent<ClownFish>();

                    if (clownFish != null && (clownFish.IsSafe || clownFish.IsAtHome))
                    {
                        continue;
                    }

                    goal = hit.collider.gameObject;
                    if (clownFish != null) clownFish.SetBeingChased(true);
                    
                    // Lock onto initial point instantly upon detection
                    if (NavMesh.SamplePosition(goal.transform.position, out NavMeshHit navHit, goalSampleRadius, agent.areaMask))
                    {
                        agent.isStopped = false;
                        agent.SetDestination(navHit.position);
                    }
                    return; 
                }
            }
        }
    }
}