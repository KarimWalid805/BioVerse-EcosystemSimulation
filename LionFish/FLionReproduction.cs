using UnityEngine;
using UnityEngine.AI; 
using System.Collections;

public class FLionReproduction : MonoBehaviour 
{
    public LionFish lionFish;
    private NavMeshAgent navAgent;

    [Header("Wander Boundaries")]
    public float minX = 26f;
    public float maxX = 122f;
    public float minZ = -8f;
    public float maxZ = 81f;
    private float timeToWait = 0f;
    private bool isWaiting;
    private float waitTimer = 0f;
    [Header("Dance Settings")]
    public float seaSurfaceY = 40f;
    public bool isDancing = false;
    public bool isEvaluating = false;

    [Header("Evaluation Settings")]
    public float evaluationTime = 10f;
    [Tooltip("Male size that gives the lowest chance of mating")]
    public float minMaleSize = 0.6f;
    [Tooltip("Male size that gives the highest chance of mating")]
    public float maxMaleSize = 1f;
    public GameObject eggPrefab;
    public float minWaitTime = 5f;
    [Tooltip("Maximum time the fish will wait before moving again")]
    public float maxWaitTime = 10f;
   
    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        lionFish = GetComponent<LionFish>();
    }

    private void OnEnable()
    {
        isDancing = false;
        isEvaluating = false;
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
        // Watchdog: if the NavMeshAgent lost its path but we're not intentionally waiting/dancing,
        // try a small nudge so the agent picks a nearby destination and keeps moving.
        if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh && !navAgent.pathPending && !navAgent.hasPath && !isWaiting && !isDancing && !isEvaluating)
        {
            NudgeAgent();
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
// The Male calls this function when he spots her!
    public void StartEvaluation(MLionReproduction suitor)
    {
        if (isEvaluating || isDancing) return; // Already busy
        StartCoroutine(CourtshipRoutine(suitor));
    }
    private IEnumerator CourtshipRoutine(MLionReproduction suitor)
    {
        isEvaluating = true;
        
        // Tell both main brains to freeze and update UI
        lionFish.isCourting = true;
        suitor.lionFish.isCourting = true;

        // Stop both fish from swimming away
        if (navAgent.isActiveAndEnabled) navAgent.isStopped = true;
        NavMeshAgent suitorAgent = suitor.GetComponent<NavMeshAgent>();
        if (suitorAgent.isActiveAndEnabled) suitorAgent.isStopped = true;

      

        // Wait evaluationTime seconds but abort early if energy gets low
        float elapsed = 0f;
        while (elapsed < evaluationTime)
        {
            if (lionFish.Energy < lionFish.huntThreshold)
            {
               
                if (suitor != null) suitor.HandleRejection();
                AbortReproduction("low energy during evaluation");
                yield break;
            }

            if (suitor != null && suitor.lionFish != null && suitor.lionFish.Energy < suitor.lionFish.huntThreshold)
            {
              
                if (suitor != null) suitor.HandleRejection();
                AbortReproduction("suitor low energy during evaluation");
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- MATH: Calculate the probability based on his size ---
        float maleSize = suitor.transform.localScale.x; 
        
        // This calculates a percentage from 0.0 (smallest) to 1.0 (biggest)
        float probability = Mathf.InverseLerp(minMaleSize, maxMaleSize, maleSize);
        
        // Make sure even small males have at least a tiny 10% chance, and big males max at 95%
        probability = Mathf.Clamp(probability, 0.15f, 0.80f); 

        float diceRoll = UnityEngine.Random.value;
        Debug.Log($"Male size is {maleSize}. Probability to accept: {probability * 100}%. Rolled: {diceRoll * 100}");

        if (diceRoll <= probability)
        {
           
            
            suitor.StartReproductionDance(this.transform);
          
            this.StartReproductionDance(suitor.transform);
        }
        else
        {
          
            
            // Unfreeze both fish
            isEvaluating = false;
            lionFish.isCourting = false;
            suitor.lionFish.isCourting = false;
            
            if (navAgent.isActiveAndEnabled) navAgent.isStopped = false;
            if (suitorAgent.isActiveAndEnabled) suitorAgent.isStopped = false;

            // Tell the male he was rejected so he can move on
            suitor.HandleRejection();

            // Clean up female reproduction state so she can resume normal behaviour
            lionFish.FinishedReproduction();
        }
    }
 
    public void StartReproductionDance(Transform partnerTransform)
    {
        isDancing = true;
        lionFish.isCourting = false;
        navAgent.enabled = false; 
       
        StartCoroutine(SwimUpwardsRoutine(partnerTransform));
    }

    public void LionLayEggs()
    {
      

        // Calculate positions exactly 2 units to the right and left of the fish
        Vector3 rightPosition = transform.position + (transform.right * 0.5f);
        

        // Spawn one egg at the right position 
        Instantiate(eggPrefab, rightPosition, Quaternion.identity);
        
        lionFish.Energy -= 30f; // Laying eggs is exhausting!
        lionFish.FinishedReproduction(); // Tell the main script we are no longer reproducing
        StartCoroutine(ReturnToReefRoutine());
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

            // Abort if energy falls too low during the ascent
            if (lionFish.Energy < lionFish.huntThreshold)
            {
                Debug.Log("Female Lionfish: Energy low during dance, aborting.");
                if (partnerTransform != null)
                {
                    MLionReproduction partner = partnerTransform.GetComponent<MLionReproduction>();
                    if (partner != null) partner.AbortReproduction("partner aborted: female low energy");
                }

                AbortReproduction("low energy during dance");
                yield break;
            }

            yield return null;
        }

        Vector3 flatRotation = transform.eulerAngles;
        flatRotation.x = 0;
        transform.eulerAngles = flatRotation;

       
        // Here we would trigger the egg laying and fertilization process
        LionLayEggs();
        
    }
   private IEnumerator ReturnToReefRoutine()
    {
      

        // 1. Shoot a laser straight down to figure out exactly where the floor is
        Vector3 targetFloor = transform.position;
        targetFloor.y = 0; // Fallback in case the raycast misses
        
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 200f))
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
        
        lionFish.isReproducing = false; // Tell the main script we are no longer reproducing
        lionFish.FinishedReproduction(); // Tell the main script we are no longer reproducing
        this.enabled = false;           // Turn this whole male script off until he gets hungry/horny again

     
        
    }

    // Abort helper to cancel reproduction and restore normal behaviour
    public void AbortReproduction(string reason)
    {
        StopAllCoroutines();
        isEvaluating = false;
        isDancing = false;

        if (navAgent != null)
        {
            navAgent.enabled = true;
            navAgent.isStopped = false;
        }

       
        lionFish.FinishedReproduction();
    }

    // Small nudge to push the NavMeshAgent off a stuck state by giving it a very nearby destination.
    private void NudgeAgent()
    {
        if (navAgent == null || !navAgent.isOnNavMesh) return;

        navAgent.isStopped = false;

        // Try a short step forward first
        Vector3 nudgeTarget = transform.position + transform.forward * 1f;
        if (NavMesh.SamplePosition(nudgeTarget, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            navAgent.SetDestination(hit.position);
            return;
        }

        // Fallback: pick any random nearby valid navmesh position
        PickNewDestination();
    }

   


   
}