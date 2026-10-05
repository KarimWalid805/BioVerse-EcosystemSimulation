using UnityEngine;
using UnityEngine.AI;

public class Wander : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Wander Boundaries")]
    public float minX = 50f;
    public float maxX = 100f;
    public float minZ = 0f;
    public float maxZ = 65f;

    [Header("Wait Settings")]
    [Tooltip("Minimum time the fish will wait before moving again")]
    public float minWaitTime = 5f;
    [Tooltip("Maximum time the fish will wait before moving again")]
    public float maxWaitTime = 10f;

    private bool isWaiting = false;
    private float waitTimer = 0f;
    private float timeToWait = 0f;

   void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    
    // This runs every single time SetMode() turns the wander script back ON!
    void OnEnable()
    {
        isWaiting = false;
        
        // Always ensure the brakes are cleanly released
        if (agent != null && agent.isOnNavMesh) 
        {
            agent.isStopped = false;
        }

        // Instantly pick a new spot so it doesn't freeze in place
        PickNewDestination(); 
    }
    void OnDisable()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }
    void Update()
    {
        
        // to prevent triggering the wait state while it's still sliding to a halt.
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && agent.velocity.sqrMagnitude < 0.1f)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = 0f;
                timeToWait = Random.Range(minWaitTime, maxWaitTime);
                
          
                agent.isStopped = true; 
                agent.ResetPath(); // Clear the current path to prevent it from trying to move to a now irrelevant destination.
            }
            else
            {
                waitTimer += Time.deltaTime;

                if (waitTimer >= timeToWait)
                {
                    PickNewDestination();
                    isWaiting = false;
                }
            }
        }
    }

    private void PickNewDestination()
    {
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);
        Vector3 randomSpot = new Vector3(randomX, transform.position.y, randomZ);

        if (NavMesh.SamplePosition(randomSpot, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
        else
        {
            
            isWaiting = true;
            timeToWait = 0.1f;
        }
    }
}