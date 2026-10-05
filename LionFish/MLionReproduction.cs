using UnityEngine;
using UnityEngine.AI; 
using System.Collections;

public class MLionReproduction : MonoBehaviour 
{
    public LionFish lionFish;
    public LayerMask groundMask;
    private NavMeshAgent navAgent;
    private LFishEgg targetEgg;
    
    [Header("Vision Settings")]
    public float visionRange = 10f; 
    public int numberOfRays = 10;   
    public float visionAngle = 90f;
    public float seaSurfaceY = 40f;

        [Header("Wander Boundaries")]
    public float minX = 26f;
    public float maxX = 122f;
    public float minZ = -8f;
    public float maxZ = 81f;

    public bool isDancing = false;
    private bool recentlyRejected = false;
    private float timeToWait = 0f;
    private bool isWaiting;
        private float waitTimer = 0f;
            [Tooltip("Minimum time the fish will wait before moving again")]
    public float minWaitTime = 2f;
    [Tooltip("Maximum time the fish will wait before moving again")]
    public float maxWaitTime = 5f;
    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        lionFish = GetComponent<LionFish>();
    }

    void OnDisable()
    {
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = false;
        }
        
    }
    private void OnEnable()
    {
        
        isDancing = false;
        recentlyRejected = false;
         isWaiting = false;
        
        // Always ensure the brakes are cleanly released
        if (navAgent != null && navAgent.isOnNavMesh) 
        {
            navAgent.isStopped = false;
        }

        // Instantly pick a new spot so it doesn't freeze in place
        PickNewDestination(); 
    }

    void Update()
    {
        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance && navAgent.velocity.sqrMagnitude < 0.1f)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = 0f;
                timeToWait = Random.Range(minWaitTime, maxWaitTime);
                
                // 2. THE BRAKES: Explicitly tell the NavMesh engine to stop calculating micro-movements!
                navAgent.isStopped = true; 
                navAgent.ResetPath(); // Clear the current path to prevent it from trying to move to a now irrelevant destination.
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
        // If energy falls below hunt threshold while searching, abort reproduction and hunt
        if (lionFish.Energy < lionFish.huntThreshold)
        {
            AbortReproduction("low energy while searching");
            return;
        }

        // Watchdog: if the NavMeshAgent lost its path but we're not intentionally waiting/dancing,
        // try to pick a new destination so the fish keeps moving.
        if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh && !navAgent.pathPending && !navAgent.hasPath && !isWaiting && !isDancing && !recentlyRejected)
        {
            PickNewDestination();
        }

        // Only look for females if he isn't busy courting, dancing, or nursing a broken heart
        if (!lionFish.isCourting && !isDancing && !recentlyRejected)
        {
            FindNearestFemale();
        }
         
    }

    void FindNearestFemale()
    {
        float startingAngle = -visionAngle / 2f;
        float angleStep = visionAngle / (numberOfRays - 1);
        float laserThickness = 0.5f; 

        for (int i = 0; i < numberOfRays; i++)
        {
            // Abort search if energy drops mid-search
            if (lionFish.Energy < lionFish.huntThreshold)
            {
                AbortReproduction("low energy during search");
                return;
            }
            float currentAngle = startingAngle + (angleStep * i);
            Vector3 rayDirection = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            
            if (Physics.SphereCast(transform.position, laserThickness, rayDirection, out RaycastHit hit, visionRange))
            {
                if (hit.collider.CompareTag("LionFish"))
                {
                    LionFish targetFish = hit.collider.GetComponent<LionFish>();
                    
                    // If it's a female...
                    if (targetFish != null && !targetFish.isMale)
                    {
                        FLionReproduction femaleScript = targetFish.GetComponent<FLionReproduction>();
                        
                        // Check if she is currently available
                        if (femaleScript != null && !femaleScript.isEvaluating && !femaleScript.isDancing)
                        {
                           
                          
                            // Ask her to evaluate me!
                            femaleScript.StartEvaluation(this);
                            return; 
                        }
                    }
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
            // 3. THE GAS PEDAL: Release the brakes so it can move to the new spot
            navAgent.isStopped = false;
            navAgent.SetDestination(hit.position);
        }
        else
        {
            // 4. FAILSAFE: If the random spot was inside a rock and failed, 
            // force it to try again in 0.1 seconds so it doesn't get permanently stuck waiting!
            isWaiting = true;
            timeToWait = 0.1f;
        }
    }

    public void HandleRejection()
    {
        StartCoroutine(RejectionCooldown());
        navAgent.isStopped = false;

        // Clean up male reproduction state so he can resume normal behaviour
        lionFish.FinishedReproduction();
    }

    private IEnumerator RejectionCooldown()
    {
        recentlyRejected = true;
        yield return new WaitForSeconds(5f);
        recentlyRejected = false;
    }
private void FindNearestUnfertilizedEggs()
    {
        // Find every batch of eggs in the scene
        LFishEgg[] allEggs = FindObjectsByType<LFishEgg>();
        float closestDistance = Mathf.Infinity;
        targetEgg = null;

        // Loop through them to find the closest unfertilized ones
        foreach (LFishEgg egg in allEggs)
        {
            if (!egg.isFertilized)
            {
                Vector3 FishPosition = transform.position;
                Vector3 targetPosition = egg.transform.position;

                FishPosition.y = 0f; // Ignore vertical distance
                targetPosition.y = 0f; // Ignore vertical distance
                float distanceToEgg = Vector3.Distance(FishPosition, targetPosition);
                if (distanceToEgg < closestDistance)
                {
                    closestDistance = distanceToEgg;
                    targetEgg = egg;
                    // Disable the normal wandering behavior while we're fertilizing eggs
                }
            }
        }
    }
    public void StartReproductionDance(Transform partnerTransform)
    {
        isDancing = true;
        lionFish.isCourting = false; 
        navAgent.enabled = false; 
        
      
        StartCoroutine(SwimUpwardsRoutine(partnerTransform));
    }


    private IEnumerator SwimUpwardsRoutine(Transform partnerTransform)
    {
       float targetHeight = seaSurfaceY - 5f;
        
        // 1. Find the exact midpoint between the two fish to act as the invisible center pole
        Vector3 spiralCenter = (transform.position + partnerTransform.position) / 2f;
        
        // 2. Figure out the radius (how wide the spiral is) based on their current distance
        Vector3 offset = transform.position - spiralCenter;
        float currentAngle = Mathf.Atan2(offset.z, offset.x);
        float spiralRadius = offset.magnitude;
        
        // Failsafe: Don't let them crash into each other if they are touching
        if (spiralRadius < 0.5f) spiralRadius = 0.5f;

        float currentY = transform.position.y;
        float ascendSpeed = 3f;     // How fast they move up
        float rotationSpeed = 2f;   // How fast they spin around each other

        while (currentY < targetHeight)
        {
            // Move the Y axis up
            currentY += ascendSpeed * Time.deltaTime;
            
            // Spin the angle
            currentAngle += rotationSpeed * Time.deltaTime;

            // Calculate the new X and Z orbit positions
            float newX = spiralCenter.x + Mathf.Cos(currentAngle) * spiralRadius;
            float newZ = spiralCenter.z + Mathf.Sin(currentAngle) * spiralRadius;

            Vector3 nextPos = new Vector3(newX, currentY, newZ);

            // Point the fish in the direction it's spiraling, with a slight upward tilt
            Vector3 lookDirection = (nextPos - transform.position).normalized;
            lookDirection += Vector3.up * 0.5f; 
            
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 4f);
            }

            // Apply position
            transform.position = nextPos;

            yield return null;
        }

        Vector3 flatRotation = transform.eulerAngles;
        flatRotation.x = 0;
        transform.eulerAngles = flatRotation;

        StartCoroutine(FindAndFertilizeEggsRoutine());
    }
    private IEnumerator FindAndFertilizeEggsRoutine()
    {
        // 1. Wait half a second to ensure the female's spawn code actually finished
        yield return new WaitForSeconds(0.5f);

        FindNearestUnfertilizedEggs();

        if (targetEgg != null)
        {
            

            // 2. Swim towards the egg without using the NavMesh!
            // We keep swimming until we are within 1 meter of the eggs.
            while (Vector3.Distance(transform.position, targetEgg.transform.position) > 1f)
            {
                // Move forward smoothly
                transform.position = Vector3.MoveTowards(transform.position, targetEgg.transform.position, lionFish.LDefaultSpeed * Time.deltaTime);

                // Point the nose of the fish at the eggs
                Vector3 direction = (targetEgg.transform.position - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
                }

                yield return null;
            }

            // 3. We arrived! Fertilize them.
            // (Make sure your LFishEgg script has a public Fertilize() method!)
           
            targetEgg.Fertilize();
            lionFish.Energy -= 10f; // Laying eggs is exhausting!
            
          
        }
        else
        {
            Debug.LogWarning("Male Lionfish: I couldn't find any unfertilized eggs up here!");
        }

        // 4. Our job is done. Time to dive back down to the reef!
        StartCoroutine(ReturnToReefRoutine());
    }

    private IEnumerator ReturnToReefRoutine()
    {
       

        // 1. Shoot a laser straight down to figure out exactly where the floor is
        Vector3 targetFloor = transform.position;
        targetFloor.y = 0; // Fallback in case the raycast misses
        
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 200f, groundMask))
        {
            targetFloor = hit.point;
        }

        // 2. Swim straight down
        while (Vector3.Distance(transform.position, targetFloor) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetFloor, lionFish.LDefaultSpeed * Time.deltaTime);
            
            // Point the fish's nose straight down
            Vector3 direction = (targetFloor - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            }
            
            yield return null;
        }

        // 3. We hit the floor! Flatten the fish out.
        Vector3 flatRotation = transform.eulerAngles;
        flatRotation.x = 0;
        transform.eulerAngles = flatRotation;

        // 4. Snap back onto the NavMesh and turn the brain back on!
        navAgent.enabled = true;
        isDancing = false;
        
        lionFish.FinishedReproduction(); // Tell the main script we are no longer reproducing
        this.enabled = false;           // Turn this whole male script off until he gets hungry/horny again

       
    }

    // Abort helper to cancel reproduction and restore normal behaviour
    public void AbortReproduction(string reason)
    {
        StopAllCoroutines();
        isDancing = false;

        if (navAgent != null)
        {
            navAgent.enabled = true;
            navAgent.isStopped = false;
        }

        Debug.Log("Male Lionfish: Aborting reproduction - " + reason);
        lionFish.FinishedReproduction();
    }
}