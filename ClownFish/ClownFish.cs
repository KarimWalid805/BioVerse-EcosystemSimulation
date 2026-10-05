using System.Collections;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System;

public class ClownFish : Fish
{
    [NonSerialized]
    private Hunt HuntScript;
    private GoHome GoHomeScript;
    private LayEggs LayEggsScript;
    private FertilizeEggs FertilizeScript; // <-- NEW REFERENCE
    private AnemoneWander AnemoneWanderScript;
    private Wander WanderScript;



    

    public float algaeEnergyPerSecond = 50f;
    public float homeRadius = 3f;
    public float fleeSpeed = 5f;
    
    //states
    private bool isHunting;
    private bool isBeingChased;
    private NavMeshAgent navAgent;
    public bool IsSafe;
  
    public bool isHomeless = false;

    [Header("Growth & Age")]
    [Tooltip("How many seconds it takes for a baby to reach full size")]
    public float timeToAdulthood = 60f;
    public bool isAdult = false;
    private float ageTimer = 0f;
    private float initialScale;
    private float targetMaxScale;

    [Header("Reproduction Roles")]
    [Tooltip("Check this box if this fish is the Male fertilizing eggs")]
    public bool isMale = false; // <-- NEW GENDER TOGGLE
    public float reproductionCooldown; 
    private float reproductionTimer;

    private float CheckInterval = 5f; // How often to check for nearby predators when being chased
    private bool isReproducing;
    public enum FishRank { Baby, Spare, King, Queen }
    public FishRank currentRank = FishRank.Baby;
    
    [Header("Gender Transition")]
    public bool isTransitioning = false;
    public float transitionDuration = 20f;
    private float transitionTimer = 0f; // Replaced "isLayingEggs" to cover both genders

    [Header("Surface Alignment")]
    public float rotationSpeed = 10f;
    public float maxRotationAngle = 30f;
    public LayerMask groundMask;

    public float TempCheckTImer = 0f;

    
    
    public float DistanceToPredator;

    [Header("Status UI")]
    [Tooltip("Drag the Slider from your fish here")]
    public Slider energyBar; 
    [Tooltip("Drag the Text from your fish here")]
    public Text stateText;

    public bool IsAtHome
    {
        get
        {
            if (GoHomeScript == null || GoHomeScript.SeaAnemoneTarget == null) return false;
            return Vector3.Distance(transform.position, GoHomeScript.SeaAnemoneTarget.position) <= homeRadius;
        }
    }
    
  

  

    void Start()
    {
        LinearDecayRate = 0.4f;
        HuntScript = GetComponent<Hunt>();
        GoHomeScript = GetComponent<GoHome>();
        AnemoneWanderScript = GetComponent<AnemoneWander>();
        LayEggsScript = GetComponent<LayEggs>();
        FertilizeScript = GetComponent<FertilizeEggs>(); // Get male script
        WanderScript = GetComponent<Wander>();
        navAgent = GetComponent<NavMeshAgent>();
     // Set initial cooldown
        LifeSpan = UnityEngine.Random.Range(250f, 300f); //Age where the fish dies out of old age, random between 250 and 350 seconds to add some variety to the population
        FindHomeAnemone();
        
       
           
        // Diverge based on gender
        if (isMale)
        {
            reproductionCooldown = 10f; // Males check if there are eggs to fertilize more often
        }
        else
        {
            reproductionCooldown = 60f; // Females have a longer reproduction cooldown
            if (LayEggsScript != null) LayEggsScript.enabled = true;
        }
        

        // Disable reproduction scripts on start
        if (LayEggsScript != null) LayEggsScript.enabled = false;
        if (FertilizeScript != null) FertilizeScript.enabled = false;

        navAgent.updateRotation = false;
        reproductionTimer = reproductionCooldown; 

        initialScale = transform.localScale.x;

        if (initialScale >= 0.3f)
        {
            isAdult = true;
            targetMaxScale = initialScale;
        }
        else
        {
            isAdult = false;
            // Pick a random max size between 80% (0.24f) and 100% (0.3f)
            targetMaxScale = UnityEngine.Random.Range(0.24f, 0.3f);
        }
        isHunting = Energy < huntThreshold;
        SetMode(isHunting);
    }

    
    public void SetBeingChased(bool chased)
    {
        isBeingChased = chased;

        if (chased)
        {
            isReproducing = false; 
            
            if (HuntScript != null) HuntScript.enabled = false;
            if (LayEggsScript != null) LayEggsScript.enabled = false;
            if (FertilizeScript != null) FertilizeScript.enabled = false;
            if (AnemoneWanderScript != null) AnemoneWanderScript.enabled = false;
            if (WanderScript != null) WanderScript.enabled = false;
            
            // Speed up the agent for the escape sprint
            if (navAgent != null) navAgent.speed = fleeSpeed;

            // --- THE FIX: Diverge based on home status ---
            if (isHomeless)
            {
                // Homeless fish cannot go home; disable it and calculation flee path manually
                if (GoHomeScript != null) GoHomeScript.enabled = false;
            }
            else
            {
                // Housed fish sprint straight to their home anemone safely
                if (GoHomeScript != null) GoHomeScript.enabled = true;
            }
        }
        else
        {
            // Reset speed when safe
            if (navAgent != null) navAgent.speed = 3.5f; // Set this to your default swimming speed
            
            // If they are still homeless after the chase, make sure they resume hunting/wandering to search
            if (isHomeless && HuntScript != null) HuntScript.enabled = true;
        }
    }

    protected override void Update()
    {
    
        base.Update();
        float currentTemp = EcosystemDashboard.GlobalTemperature;
        TempCheckTImer += Time.deltaTime;

        if (TempCheckTImer >= CheckInterval) // Check every 5 seconds
        {
            float exponent = 2.5f; 
            TempCheckTImer = 0f; // Reset timer
            
            if (currentTemp <= 27f)
            {
            // InverseLerp from 27 down to 20. 
            // Result: 0.0 at 27°C (optimal), 1.0 at 20°C (extreme stress)
            float stress = Mathf.InverseLerp(27f, 20f, currentTemp);
            
            // Apply the exponential curve
            float curvedStress = Mathf.Pow(stress, exponent);
            
            // Lerp from the base interval (5s) UP to the max interval (20s)
            LinearDecayRate = Mathf.Lerp(0.4f, 0.8f, curvedStress);
                if (!isMale)
                {
                reproductionCooldown = Mathf.Lerp(60f, 120f, curvedStress); 
                } else
                {
                    reproductionCooldown = Mathf.Lerp(10f, 20f, curvedStress);
                }
            
            }
            else
            {
            // InverseLerp from 27 up to 35. 
            // Result: 0.0 at 27°C (optimal), 1.0 at 35°C (extreme stress)
            float stress = Mathf.InverseLerp(27f, 35f, currentTemp);
            
            // Apply the exponential curve
            float curvedStress = Mathf.Pow(stress, exponent);
            
            //hunger rate increases from 0.4f up to 0.6f as it gets hotter
            LinearDecayRate = Mathf.Lerp(0.4f, 0.8f, curvedStress);
                if (!isMale)
                {
                reproductionCooldown = Mathf.Lerp(60f, 100f, curvedStress);    
                } else
                {
                    reproductionCooldown = Mathf.Lerp(10f, 15f, curvedStress);
                }
            }
        }   
    
    

       
    
        SlopeSnap surfaceSnap = GetComponent<SlopeSnap>();
        Age += Time.deltaTime;
        if (Age >= LifeSpan)
        {
            Destroy(gameObject);
            return;
        }
        if (surfaceSnap != null) surfaceSnap.SurfaceAlignment();

        // 1. Handle Growing Up
        if (!isAdult)
        {
            HandleGrowth();
        }

        // 2. Handle Reproduction Timer
        if (!isReproducing)
        {
            reproductionTimer -= Time.deltaTime;
        }
        bool CanBreed = currentRank == FishRank.Queen || currentRank == FishRank.King;

    
        // 3. Trigger Reproduction (Laying or Fertilizing)
        if (IsAtHome && !isHunting && !isBeingChased && !isReproducing && reproductionTimer <= 0f && isAdult && Energy >= 70f && !isTransitioning && CanBreed && !isHomeless)
        {    
            StartReproduction();
        }
        // If the fish was evicted and is currently homeless, use raycasts to look for a vacancy!
        if (isHomeless)
        {
            ActiveHomeSearch();
        }
        
        if (isBeingChased && isHomeless)
        {
            FleeFromNearestPredator();
        }

        // Handle Hunger
       if (!isHunting && Energy < huntThreshold && !isBeingChased)
        {
            isHunting = true;
            isReproducing = false; 
            SetMode(true);
        }

        if (isHunting && Energy >= 99f)
        {
            isHunting = false;
            SetMode(false);
        }

        if (IsAtHome && !isReproducing)
        {
            CheckAndLeaveSafety();
        }

        if (Energy <= 1f)
        {
            Destroy(gameObject);
            Debug.Log($"{gameObject.name} died of starvation.");
        }
        // Handle Gender Transition Timer
        if (isTransitioning)
        {
            transitionTimer -= Time.deltaTime;
            if (transitionTimer <= 0f)
            {
                isTransitioning = false;
                isMale = false; 
                reproductionCooldown = 60f; // <--- UPDATE THE COOLDOWN
            }
        }
    
    }
private void FleeFromNearestPredator()
    {
        // old performance heavy method (finds every single lionfish in the scene, even the ones on the other side of the map)
        // LionFish[] allPredators = FindObjectsByType<LionFish>();
        // if (allPredators.Length == 0) return;
        //  if (navAgent != null) navAgent.speed = fleeSpeed;
        // // 1. Find the closest threat
        // LionFish closestPredator = null;
        // float closestDist = Mathf.Infinity;
        // foreach (LionFish predator in allPredators)
        // {
        //     float dist = Vector3.Distance(transform.position, predator.transform.position);
        //     if (dist < closestDist)
        //     {
        //         closestDist = dist;
        //         closestPredator = predator;
        //     }
        // }

        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, 3f);
        
        LionFish closestPredator = null;
        float closestDist = Mathf.Infinity;

        //  Look through the few things we found
        foreach (Collider col in nearbyObjects)
        {
            if (col.CompareTag("LionFish"))
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestPredator = col.GetComponent<LionFish>();
                }
            }
        }
        if (closestPredator != null)
        {
            //  Calculate the vector pointing DIRECTLY AWAY from the predator
            Vector3 fleeDirection = (transform.position - closestPredator.transform.position).normalized;

            //  Project a destination point 8 units away in that safe direction
            Vector3 targetFleePoint = transform.position + fleeDirection * 3f;

            // Snap it to the NavMesh so the fish doesn't try to swim through a wall
            if (NavMesh.SamplePosition(targetFleePoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                navAgent.SetDestination(hit.position);
            }
        }
    }
    private void HandleGrowth()
    {
        ageTimer += Time.deltaTime;

        // Calculate current scale based on how much time has passed
        float currentScale = Mathf.Lerp(initialScale, targetMaxScale, ageTimer / timeToAdulthood);
        
        // Apply the scale to the fish
        transform.localScale = new Vector3(currentScale, currentScale, currentScale);

        // Check if fully grown
        if (ageTimer >= timeToAdulthood)
        {
            isAdult = true;
            
        }
    }
    public void EvictFromAnemone()
    {
        Debug.Log($"<color=orange>{gameObject.name} was evicted due to overpopulation!</color>");
        
        // Wipe their target and safety status
        if (GoHomeScript != null) GoHomeScript.SeaAnemoneTarget = null;
        IsSafe = false;
        
        // Immediately start searching for a new home
        FindHomeAnemone();
    }
    public void AssignHome(AnemoneManager homeManager)
    {
        if (homeManager != null)
        {
            //  Tell the Anemone manager to add this fish to the list
            homeManager.RegisterFish(this);
            
            //  Tell the GoHome script exactly where to swim
            if (GoHomeScript != null)
            {
                GoHomeScript.SeaAnemoneTarget = homeManager.transform;
            }
            
            Debug.Log($"<color=green>{gameObject.name} was spawned and instantly assigned to a home!</color>");
        }
    }
    private void FindHomeAnemone()
    {
        GameObject[] allAnemones = GameObject.FindGameObjectsWithTag("SeaAnemone");
        AnemoneManager bestAnemone = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject anemone in allAnemones)
        {
            AnemoneManager manager = anemone.GetComponent<AnemoneManager>();
            
            // check if the manager has less than 5 fish
            if (manager != null && manager.GetResidentCount() < 5)
            {
                float distance = Vector3.Distance(transform.position, anemone.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    bestAnemone = manager;
                }
            }
        }

        if (bestAnemone != null)
        {
            bestAnemone.RegisterFish(this);
            if (GoHomeScript != null) GoHomeScript.SeaAnemoneTarget = bestAnemone.transform;
        }
        else
        {
            isHomeless = true;
            // If every single anemone is full, they stay homeless and wander
            Debug.LogWarning($"{gameObject.name} is homeless! All anemones are at max capacity.");
            // Wipe the target and turn OFF the GoHome script so it stops trying to swim to 'null'
            if (GoHomeScript != null) 
            {
                GoHomeScript.SeaAnemoneTarget = null;
                GoHomeScript.enabled = false;
            }

            // Turn ON the Hunt or Wander script so it physically swims around the map while searching
            if (WanderScript != null) WanderScript.enabled = true;
        }
    }
  
private void ActiveHomeSearch()
    {
       
        float visionAngle = 300f;
        int numberOfRays = 10;
        float visionRange = 30f;
        float laserThickness = 0.5f; 

        float startingAngle = -visionAngle / 2f;
        float angleStep = visionAngle / (numberOfRays - 1);

        for (int i = 0; i < numberOfRays; i++)
        {
            float currentAngle = startingAngle + (angleStep * i);
            Vector3 rayDirection = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            
            // Shoot the laser!
            if (Physics.SphereCast(transform.position, laserThickness, rayDirection, out RaycastHit hit, visionRange))
            {
                if (hit.collider.CompareTag("SeaAnemone"))
                {
                    AnemoneManager manager = hit.collider.GetComponent<AnemoneManager>();
                    
                    // Does this anemone have space for me?
                    if (manager != null && manager.GetResidentCount() < manager.maxCapacity)
                    {
                        Debug.Log($"<color=green>{gameObject.name} found a new home using Raycasts!</color>");
                        
                  
                        isHomeless = false;
                        
                  
                        manager.RegisterFish(this);
                        
                
                        if (GoHomeScript != null) 
                        {
                            GoHomeScript.SeaAnemoneTarget = manager.transform;
                            GoHomeScript.enabled = true;
                        }
                        
                        
                        if (WanderScript != null) WanderScript.enabled = false;
                        
                        return; 
                    }
                }
            }
        }
    }
private void UpdateStatusUI()
    {
        // Update the Slider bar
        if (energyBar != null)
        {
            // Divide current energy by max to get a percentage between 0 and 1
            energyBar.value = Energy / MaxEnergy; 
        }

        //  Update the Text based on priority of what the fish is doing
        if (stateText != null)
        {
            // Prefix the text with the fish's current rank!
            string prefix = $"[{currentRank.ToString()}] ";
  
            if (isBeingChased)
            {
                stateText.text = prefix +"Fleeing Predator!";
                stateText.color = Color.red; // Optional: make text red when running
            }
            else if (isReproducing)
            {
                stateText.text = prefix + (isMale ? "Fertilizing Eggs..." : "Laying Eggs...");
                stateText.color = Color.magenta;
            }
            else if (isHunting)
            {
                stateText.text = prefix + "Hunting for Algae";
                stateText.color = Color.yellow;
            }
            else if (IsSafe)
            {
                stateText.text = prefix + "Resting at Anemone";
                stateText.color = Color.green;
            }
            else
            {
                stateText.text = prefix + "Swimming";
                stateText.color = Color.white;
            }
        }
    }
    private void StartReproduction()
    {
        isReproducing = true;
        
        if (HuntScript != null) HuntScript.enabled = false;
        if (GoHomeScript != null) GoHomeScript.enabled = false;
        
        // Use Rank instead of isMale to guarantee the right script turns on
        if (currentRank == FishRank.King)
        {
            if (FertilizeScript != null) FertilizeScript.enabled = true;
        }
        else if (currentRank == FishRank.Queen)
        {
            if (LayEggsScript != null) LayEggsScript.enabled = true;
        }
    }

    public void FinishedReproduction()
    {
        isReproducing = false;
        reproductionTimer = reproductionCooldown; 
        
        if (LayEggsScript != null) LayEggsScript.enabled = false;
        if (FertilizeScript != null) FertilizeScript.enabled = false;
        
        SetMode(isHunting); 
        if (IsSafe && AnemoneWanderScript != null)
        {
            AnemoneWanderScript.enabled = true;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("RedAlgae"))
        {
            MarkEating(); 
            Energy += algaeEnergyPerSecond * Time.deltaTime;
            Energy = Mathf.Clamp(Energy, MinEnergy, MaxEnergy);
        }
    }

    private void SetMode(bool huntEnabled)
    {
        if (isReproducing) return; 

        if (HuntScript != null) HuntScript.enabled = huntEnabled;
        
        

        if (GoHomeScript != null) 
        {
            GoHomeScript.enabled = !huntEnabled && !IsSafe;
        }
            // Update the UI floating above the fish every frame
        UpdateStatusUI();
    }
 
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("SeaAnemone")) 
        {
            IsSafe = true;
            if (isBeingChased)
        {
            SetBeingChased(false);
        }
           
           if (WanderScript != null) WanderScript.enabled = false;
           // Safely turn on the wander script
        if (AnemoneWanderScript != null && !isReproducing)
        {
            AnemoneWanderScript.enabled = true; 
        }

        if (navAgent != null)
        {
            navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance; 
        }
          
        }
    }
   

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("SeaAnemone")) 
{
            IsSafe = false;

            if (navAgent != null) navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            
            if (AnemoneWanderScript != null) AnemoneWanderScript.enabled = false; 
           
        }
    }

    public float leaveSafetyDistance = 3f;

    public void CheckAndLeaveSafety()
    {
        // 1. Find every Lionfish currently in the scene
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, 3f);

        // 2. If there are no Lionfish left, make the distance infinite (so the Clownfish feels safe) and stop checking
        if (nearbyObjects.Length == 0)
        {
            DistanceToPredator = Mathf.Infinity;
            return;
        }

        // 3. Set a starting "closest" distance to something massive
        float closestDistance = Mathf.Infinity;

        // 4. Loop through every Lionfish to find which one is actually the closest
        foreach (Collider nearbyObject in nearbyObjects)
        {
            if (nearbyObject.gameObject.CompareTag("LionFish")) 
            {
                LionFish predator = nearbyObject.GetComponent<LionFish>();
                float distance = Vector3.Distance(transform.position, predator.transform.position);

                if (distance < closestDistance)
                {
                closestDistance = distance;
            }
        }
        }
    
        DistanceToPredator = closestDistance;
        
        if (DistanceToPredator > leaveSafetyDistance && Energy > huntThreshold && IsAtHome)
        {
            IsSafe = true;
            isHunting = false;
            SetMode(false);
        }
        else if (DistanceToPredator < leaveSafetyDistance && Energy < huntThreshold && IsAtHome)
        {
            IsSafe = true;
            isHunting = false;
            SetMode(false);
        } 
        else if (DistanceToPredator > leaveSafetyDistance && Energy < huntThreshold && IsAtHome)
        {
            IsSafe = false;
            isHunting = true;
            SetMode(true);
        }
         
    }
    
    public void AssignRank(FishRank newRank)
    {
        if (currentRank == newRank) return; 

        if (newRank == FishRank.Queen && currentRank != FishRank.Baby)
        {
            isTransitioning = true;
            transitionTimer = transitionDuration;
            isMale = true; 
        }
        else if (newRank == FishRank.Queen && currentRank == FishRank.Baby)
        {
            isTransitioning = false;
            isMale = false;
            reproductionCooldown = 60f; 
        }
        else if (newRank == FishRank.King)
        {
            isTransitioning = false;
            isMale = true;
            reproductionCooldown = 10f; 
        }
        else
        {
            isTransitioning = false;
            isMale = true;
        }

        currentRank = newRank;
        UpdateStatusUI();
    }

   void OnMouseEnter()
    {
        // Pull the data directly from this script's live variables!
        // I included your currentRank variable here to show how powerful this is:
        string details = $"<b>Clownfish (Prey)</b>\n" +
                         $"Rank: {currentRank}\n" +
                         $"Age: {Age.ToString("F1")}s\n" +
                         $"Energy: {Energy.ToString("F0")}/100\n" +
                         $"Size: {transform.localScale.x.ToString("F2")}\n";

        // Send it to the global manager
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(details);
        }
    }

    void OnMouseExit()
    {
        // Hide the tooltip when the mouse leaves
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}