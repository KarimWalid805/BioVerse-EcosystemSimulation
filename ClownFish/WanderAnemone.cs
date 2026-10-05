using UnityEngine;
using UnityEngine.AI;

public class AnemoneWander : MonoBehaviour
{
    [Header("Anemone Settings")]
    [Tooltip("How long the fish waits before picking a new spot")]
    public float wanderWaitTime = 3f;

    private float wanderRadius;
    private NavMeshAgent agent;
    private GoHome goHomeScript; 
    private float timer;
    private bool radiusCalculated = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        goHomeScript = GetComponent<GoHome>();
    }

    void OnEnable()
    {
        // Instantly force a new wander spot when the fish arrives home
        timer = wanderWaitTime; 
    }

    void Update()
    {
        // 1. Wait until the Spawner/ClownFish script gives us our house
        if (goHomeScript == null || goHomeScript.SeaAnemoneTarget == null) return;

        // 2. Calculate the radius ONCE based on our specific home
        if (!radiusCalculated)
        {
            CalculateRadius(goHomeScript.SeaAnemoneTarget);
        }

        // 3. Normal Wander logic
        timer += Time.deltaTime;
        if (timer >= wanderWaitTime || (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance))
        {
            PickNewWanderSpot(goHomeScript.SeaAnemoneTarget.position);
            timer = 0f; 
        }
    }

    private void CalculateRadius(Transform anemoneTransform)
    {
        Collider anemoneCollider = anemoneTransform.GetComponent<Collider>();
        if (anemoneCollider != null)
        {
            // Measure the anemone we were specifically assigned to
            wanderRadius = Mathf.Max(anemoneCollider.bounds.extents.x, anemoneCollider.bounds.extents.z);
            wanderRadius *= 0.5f; 
        }
        else
        {
            Debug.LogWarning("AnemoneWander: Sea Anemone has no collider! Defaulting to radius 4.");
            wanderRadius = 4f; 
        }
        radiusCalculated = true; // Never calculate again unless we get evicted
    }

    private void PickNewWanderSpot(Vector3 centerPosition)
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection.y = 0f; 

        // Add the offset so they stay at THEIR specific house
        Vector3 targetPosition = centerPosition + randomDirection;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // Call this if the fish gets evicted and needs to recalculate its new home's size later
    public void ResetRadius()
    {
        radiusCalculated = false;
    }
}