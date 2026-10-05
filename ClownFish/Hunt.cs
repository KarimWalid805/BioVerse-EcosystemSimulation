using UnityEngine;
using UnityEngine.AI; 
using System;
public class Hunt : MonoBehaviour {
[NonSerialized]
    public GameObject goal;
    public bool HasGoal => goal != null;
    private NavMeshAgent agent;

    [Header("Hunt Movement")]
    public float huntSpeed = 3f;
    public float arrivalDistance = 0.5f;

    private float defaultSpeed;

    [Header("Hunt Boundaries")]
    public float minX = 26f;
    public float maxX = 71f;
    public float minZ = -8f;
    public float maxZ = 81f;

    [Header("Vision Settings")]
    public float visionRange = 5f;   // How far the fish can see
    public int numberOfRays = 5;      // How many lasers to shoot
    public float visionAngle = 300f;
    public float goalSampleRadius = 8f;

            
    void Start() {
        agent = GetComponent<NavMeshAgent>();
        
        defaultSpeed = agent.speed;
        ScanForAlgae();
    }

    void OnEnable()
    {
        if (agent != null)
        {
            agent.speed = huntSpeed;
            ClownFish ClownFish = GetComponent<ClownFish>();
            BoxCollider fishCollider = GetComponent<BoxCollider>();
            if (fishCollider != null) fishCollider.enabled = true;
            ClownFish.IsSafe = false;
            
            AnemoneWander anemoneWander = GetComponent<AnemoneWander>();
            GoHome goHomeScript = GetComponent<GoHome>();
            FertilizeEggs fertilizeEggsScript = GetComponent<FertilizeEggs>();
            LayEggs layEggsScript = GetComponent<LayEggs>();
            if (anemoneWander != null) anemoneWander.enabled = false;
            if (goHomeScript != null) goHomeScript.enabled = false;
            if (fertilizeEggsScript != null) fertilizeEggsScript.enabled = false;
            if (layEggsScript != null) layEggsScript.enabled = false;
            

        }

        // Clear stale goal when hunt mode starts again.
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
        agent.speed = huntSpeed;

        if (goal != null)
        {
            Vector3 targetPosition = goal.transform.position;
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit navHit, goalSampleRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
            }

            return;
        }

        // No target yet: keep scanning and roam like Wander.
        if (goal == null)
        {
          ScanForAlgae();
            if (goal == null && !agent.pathPending && agent.remainingDistance < arrivalDistance)
            {
                float randomX = UnityEngine.Random.Range(minX, maxX);
                float randomZ = UnityEngine.Random.Range(minZ, maxZ);
                Vector3 randomSpot = new Vector3(randomX - 5f, transform.position.y, randomZ - 5f);

                if (NavMesh.SamplePosition(randomSpot, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            } 

            return;
        }

    }

      

 

void ScanForAlgae()
    {
       float startingAngle = -visionAngle / 2f;
        float angleStep = visionAngle / (numberOfRays - 1);
        
        // Make the laser half a meter thick!
        float laserThickness = 0.5f; 

        for (int i = 0; i < numberOfRays; i++)
        {
            float currentAngle = startingAngle + (angleStep * i);
            Vector3 rayDirection = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            
            Debug.DrawRay(transform.position, rayDirection * visionRange, Color.cyan);

            // UPGRADED: Using SphereCast instead of Raycast!
            if (Physics.SphereCast(transform.position, laserThickness, rayDirection, out RaycastHit hit, visionRange))
            {
                if (hit.collider.CompareTag("RedAlgae"))
                {
                    goal = hit.collider.gameObject;
                   
                    Vector3 targetPosition = goal.transform.position;
                    if (NavMesh.SamplePosition(targetPosition, out NavMeshHit navHit, goalSampleRadius, NavMesh.AllAreas))
                  
                    {
                        agent.SetDestination(navHit.position);
                    }

                    
                    
                    return; 
                }
            }
        }
    }
}

