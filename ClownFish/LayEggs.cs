using UnityEngine;
using UnityEngine.AI;

public class LayEggs : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Drag your Egg Prefab asset here")]
    public GameObject eggPrefab;
    
    [Tooltip("How far OUTSIDE the anemone should the fish swim to lay eggs?")]
    public float extraDistanceToLay = 4f; 
    
    [Tooltip("What layers count as the floor?")]
    public LayerMask groundMask;

    [Tooltip("Keep this very low (like 0.05) so it barely hovers to prevent clipping")]
    public float spawnHeightOffset = 0.05f; 

    private NavMeshAgent navAgent;
    private ClownFish clownFish;
    private GoHome goHomeScript;
    private bool isTraveling;
    private float stuckTimer = 0f;
    public float maxTimeToReachSpot = 10f; 

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        clownFish = GetComponent<ClownFish>();
        goHomeScript = GetComponent<GoHome>();
    }

    void OnEnable()
    {
        stuckTimer = 0f; 
        AnemoneWander anemoneWander = GetComponent<AnemoneWander>();
        anemoneWander.enabled = false;
        if (goHomeScript != null && goHomeScript.SeaAnemoneTarget != null)
        {
            // 1. Calculate the actual radius of the anemone so we know how big it is
            float anemoneRadius = 2f; // Fallback default
            Collider anemoneCol = goHomeScript.SeaAnemoneTarget.GetComponent<Collider>();
            if (anemoneCol != null)
            {
                anemoneRadius = Mathf.Max(anemoneCol.bounds.extents.x, anemoneCol.bounds.extents.z);
               
            }

            // 2. The total distance is the anemone's size PLUS our chosen extra distance
            float totalLayDistance = anemoneRadius + extraDistanceToLay + Random.Range(-1f, 2f);

            // 3. Pick a random direction
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            Vector3 targetPos = goHomeScript.SeaAnemoneTarget.position + (randomDir * totalLayDistance);
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, 3f, NavMesh.AllAreas))
            {
                navAgent.SetDestination(hit.position);
            }
            else
            {
                navAgent.SetDestination(targetPos);
            }
            
            isTraveling = true;
         
        }
        else
        {
            clownFish.FinishedReproduction(); 
        }
    }

    void Update()
    {
        if (isTraveling)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer > maxTimeToReachSpot)
            {
                Debug.LogWarning("Clownfish got stuck! Aborting path and laying eggs here.");
                SpawnEggs();
                return;
            }

            if (!navAgent.pathPending)
            {
                if (navAgent.remainingDistance <= navAgent.stoppingDistance + 0.1f)
                {
                    if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude < 0.2f)
                    {
                      
                        SpawnEggs();
                    }
                }
            }
        }
    }

    private void SpawnEggs()
    {
        isTraveling = false;
        
        // 1. Shoot a laser slightly above the fish straight down
        Vector3 rayStart = transform.position + (Vector3.up * 1f);
        RaycastHit hit;

        // 2. Check if the laser hits the "Ground"
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 50f, groundMask))
        {
            // Calculate the exact rotation needed to sit flush on the slope
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            
            // Set position exactly at the hit point + a tiny offset to stop z-fighting (flickering)
            Vector3 spawnPos = hit.point + (hit.normal * spawnHeightOffset);

            GameObject newEggs = Instantiate(eggPrefab, spawnPos, slopeRotation);
            AnemoneManager myManager = goHomeScript.SeaAnemoneTarget.GetComponent<AnemoneManager>();
            if (myManager != null)
            {
                myManager.familyEggs = newEggs;
            }
            
            clownFish.Energy -= 20f; // Laying eggs costs energy
        }
        else
        {
            // Fallback: If the raycast somehow misses, just spawn it at the fish's location
            Instantiate(eggPrefab, transform.position, Quaternion.identity);
            Debug.LogWarning("ClownFish: Raycast missed the ground. Spawned eggs at default position.");
        }
        
        clownFish.FinishedReproduction();
    }
}