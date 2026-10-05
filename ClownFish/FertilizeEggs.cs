using UnityEngine;
using UnityEngine.AI;

public class FertilizeEggs : MonoBehaviour
{
    private NavMeshAgent navAgent;
    private ClownFish clownFish;
    private GoHome goHomeScript; 
    private CFishEgg targetEgg;
    
    private bool isTraveling;
    private float stuckTimer = 0f;
    public float maxTimeToReachSpot = 15f; 

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
        if (anemoneWander != null) anemoneWander.enabled = false; 

        FindFamilyEggs();

        if (targetEgg != null)
        {
            navAgent.SetDestination(targetEgg.transform.position);
            isTraveling = true;
            
        }
        else
        {
            // No family eggs found (or they are already fertilized), go back to normal fish stuff
            clownFish.FinishedReproduction();
        }
    }

   

    
    private void FindFamilyEggs()
    {
        targetEgg = null;

        // 1. Where do we live?
        if (goHomeScript == null || goHomeScript.SeaAnemoneTarget == null) return;

        // 2. Ask our specific Anemone Manager if our Queen laid eggs
        AnemoneManager myManager = goHomeScript.SeaAnemoneTarget.GetComponent<AnemoneManager>();
        
        if (myManager != null && myManager.familyEggs != null)
        {
            // 3. Grab the egg script attached to the family eggs
            CFishEgg familyEggScript = myManager.familyEggs.GetComponent<CFishEgg>();
            
            // 4. Check if they still need fertilizing
            if (familyEggScript != null && !familyEggScript.isFertilized)
            {
                targetEgg = familyEggScript;
            }
        }
    }

    void Update()
    {
        if (isTraveling && targetEgg != null)
        {
            stuckTimer += Time.deltaTime;

            // Failsafe if he gets stuck on a rock
            if (stuckTimer > maxTimeToReachSpot)
            {
                Debug.LogWarning("King ClownFish got stuck! Giving up on these eggs.");
                clownFish.FinishedReproduction();
                return;
            }

            // Standard arrival check
            if (!navAgent.pathPending)
            {
                Vector3 FishPosition = transform.position;
                Vector3 targetPosition = targetEgg.transform.position;

                FishPosition.y = 0f; // Ignore vertical distance
                targetPosition.y = 0f; // Ignore vertical distance
                
                float flatDistanceToEggs = Vector3.Distance(FishPosition, targetPosition);
                if (flatDistanceToEggs <= navAgent.stoppingDistance + 1.0f || navAgent.remainingDistance <= navAgent.stoppingDistance + 0.1f)
                {
                    if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude < 0.2f || flatDistanceToEggs <= navAgent.stoppingDistance)
                    {
                        DoFertilize();
                    }
                }
            }
        }
        else if (isTraveling && targetEgg == null)
        {
            // If the eggs were destroyed while he was swimming to them
            clownFish.FinishedReproduction();
        }
    }

    private void DoFertilize()
    {
        isTraveling = false;
        targetEgg.Fertilize();
        clownFish.Energy -= 20f; // Fertilizing costs some energy too
       
     
        AnemoneManager myManager = goHomeScript.SeaAnemoneTarget.GetComponent<AnemoneManager>();
        if (myManager != null)
        {
          
            myManager.familyEggs = null; 
        }

        clownFish.FinishedReproduction();
    }
}