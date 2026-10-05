using UnityEngine;
using UnityEngine.AI;

public class GoHome : MonoBehaviour
{
    private NavMeshAgent agent;
    private Collider fishCollider;
    public Transform seaAnemoneTarget;
    public Transform SeaAnemoneTarget;

    

    [Header("Anemone Wandering Settings")]
    [Tooltip("How close the fish needs to be to count as 'inside' the anemone")]
    public float anemoneRadius = 4f; 
    [Tooltip("How far from the center the fish can wander while inside")]
    public float wanderRadius = 2.5f;
    [Tooltip("How often the fish picks a new spot to swim to inside the anemone")]
    public float timeBetweenWanders = 3f;
    // A timer to stop the fish from stuttering
    private float pathTimer = 0f;
    
  

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
  



    void Update()
    {
        // If the Brain hasn't given us a target yet, just wait.
        if (agent == null || SeaAnemoneTarget == null) return;

        float distanceToAnemone = Vector3.Distance(transform.position, SeaAnemoneTarget.position);

        if (distanceToAnemone > anemoneRadius)
        {
            // Only update the path twice a second to stop AI stuttering
            pathTimer -= Time.deltaTime;
            if (pathTimer <= 0f)
            {
                agent.SetDestination(SeaAnemoneTarget.position);
                pathTimer = 0.5f;
            }
        }
    }

   

    
}