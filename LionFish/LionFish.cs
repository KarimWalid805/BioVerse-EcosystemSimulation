using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class LionFish : Fish
{
    
    private ClownHunt HuntScript;
    private Wander WanderScript;
    private bool isHunting;
    private NavMeshAgent navAgent;
    private FLionReproduction FLionReproductionScript;
    private MLionReproduction MLionReproductionScript;


    public float maleCooldown = 30f;
    public float femaleCooldown = 90f; 
    private float reproductionTimer;
    //states
    private int totalEaten = 0;
    private bool Detected;

     [Header("Growth & Age")]
    [Tooltip("How many seconds it takes for a baby to reach full size")]
    public float timeToAdulthood = 60f;
    public bool isAdult = false;
    private float ageTimer = 0f;
    private float initialScale;
    private float targetMaxScale;

    private float MaleMaxScale = 1f;
    private float maleTargetMaxScale;

    private float TempCheckTImer;
        private float CheckInterval = 5f; // How often to check for temp change
    private float FemaleMaxScale = 0.6f;
    private float FemaleTargetMaxScale;

    
    public float LDefaultSpeed;
    

    [Header("Status UI")]
    [Tooltip("Drag the Slider from your fish here")]
    public Slider energyBar; 
    [Tooltip("Drag the Text from your fish here")]
    public Text stateText;

     [Header("Reproduction Roles")]
    [Tooltip("Check this box if this fish is the Male fertilizing eggs")]
    public bool isMale = false; 
    public float reproductionCooldown; 
    
    public bool isReproducing;
    public bool isCourting;
    
  
    void Start()
    {
        Fish fish = GetComponent<Fish>();
        LinearDecayRate = 0.3f;
        HuntScript = GetComponent<ClownHunt>();
        WanderScript = GetComponent<Wander>();
        FLionReproductionScript = GetComponent<FLionReproduction>();
        MLionReproductionScript = GetComponent<MLionReproduction>();
        navAgent = GetComponent<NavMeshAgent>();
        LDefaultSpeed = navAgent.speed;
        if (FLionReproductionScript != null) FLionReproductionScript.enabled = false;
        if (MLionReproductionScript != null) MLionReproductionScript.enabled = false;
        navAgent.updateRotation = false;
        initialScale = transform.localScale.x;
    LifeSpan = UnityEngine.Random.Range(400f, 500f); 

    if (initialScale > 0.5f)
    {
        isAdult = true;
        targetMaxScale = initialScale; // Stay the size you currently are
    }
    
    else
    {
        isAdult = false;

        if (isMale)
        {
            // Males grow to be somewhere between 0.8 and 1.0
            targetMaxScale = UnityEngine.Random.Range(0.8f, 1f); 
        }
        else
        {
            // Females grow to be somewhere between 0.6 and 0.8
            targetMaxScale = UnityEngine.Random.Range(0.6f, 0.8f); 
        }
    }

        isHunting = Energy < huntThreshold;
       
        SetMode(isHunting);

        
    }

 protected override void Update()
    {
        // Keep shared fish hunger logic in the base class.
        base.Update();
        float currentTemp = EcosystemDashboard.GlobalTemperature;
        if (TempCheckTImer >= CheckInterval) // Check every 5 seconds
        { 
        TempCheckTImer = 0f; // Reset timer
        float exponent = 2.5f; 

        if (currentTemp <= 27f)
        {
            // InverseLerp from 27 down to 20. 
            // Result: 0.0 at 27°C (optimal), 1.0 at 20°C (extreme stress)
            float stress = Mathf.InverseLerp(27f, 20f, currentTemp);
            
            // Apply the exponential curve
            float curvedStress = Mathf.Pow(stress, exponent);
            
            // Lerp from the base interval (5s) UP to the max interval (20s)
            LinearDecayRate = Mathf.Lerp(0.45f, 0.7f, curvedStress);
            
        }
        else
        {
            // InverseLerp from 27 up to 35. 
            // Result: 0.0 at 27°C (optimal), 1.0 at 35°C (extreme stress)
            float stress = Mathf.InverseLerp(27f, 35f, currentTemp);
            
            // Apply the exponential curve
            float curvedStress = Mathf.Pow(stress, exponent);
            
            //hunger rate increases from 0.4f up to 0.6f as it gets hotter
            LinearDecayRate = Mathf.Lerp(0.45f, 0.7f, curvedStress);
            Debug.Log(LinearDecayRate);
        }
        }
        SlopeSnap surfaceSnap = GetComponent<SlopeSnap>();
        surfaceSnap.SurfaceAlignment();
        

        Age += Time.deltaTime;
        if (Age >= LifeSpan)
        {
            Destroy(gameObject);
            return;
        }
  // 1. Handle Growing Up
        if (!isAdult)
        {
            HandleGrowth();
        }

        if (reproductionTimer > 0f)
        {
            reproductionTimer -= Time.deltaTime;
        }

        // Start hunting only when energy falls below threshold.
        if (!isHunting && Energy < huntThreshold)
        {
          
            isHunting = true;
            SetMode(true);
        }


        // Return to wander only when full.
        if (isHunting && Energy >= huntThreshold)
        {
            isHunting = false;
            SetMode(false);
        }

         // 3. Trigger Reproduction (Laying or Fertilizing)
        if (!isHunting && !isReproducing && reproductionTimer <= 0f && Energy >= 70f && isAdult)
        {    
            StartReproduction();
            reproductionCooldown = 60f; 
        }

        if (Energy <= 1f)
        {
            Destroy(gameObject);
             Debug.Log($"{gameObject.name} died of starvation.");
        }

        // If current hunt target is a ClownFish that is safe at home, abandon it.
        // if (HuntScript != null && HuntScript.goal != null)
        // {
        //     ClownFish clownFish = HuntScript.goal.GetComponent<ClownFish>();
        //     if (clownFish != null && clownFish.IsAtHome)
        //     {
              
        //         HuntScript.goal = null;
        //     }
        // }
        UpdateStatusUI();
    }
    private void StartReproduction()
    {
        isReproducing = true;
        
        if (HuntScript != null) HuntScript.enabled = false;
        if (WanderScript != null) WanderScript.enabled = false;
        
        
            
        
        if (isMale)
        {
           
            
            MLionReproductionScript.enabled = true;
        }
        if (!isMale)
        {
           
            FLionReproductionScript.enabled = true;
        }
    }
    

        public void DetectedByMale(bool detected)
    {
        Detected = detected;

        if (detected)
        {
                // Only females should react to being detected by a male
                if (!isMale)
                {
                    isReproducing = true;

                    if (HuntScript != null) HuntScript.enabled = false;
                    if (WanderScript != null) WanderScript.enabled = false;
                    if (FLionReproductionScript != null) FLionReproductionScript.enabled = true;

                   
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
    public void FinishedReproduction()
    {
        isReproducing = false;
        isCourting = false;
            
        if (MLionReproductionScript != null) MLionReproductionScript.enabled = false;
        if (FLionReproductionScript != null) FLionReproductionScript.enabled = false;
        if (WanderScript != null) WanderScript.enabled = true;
        // Apply the correct cooldown based on gender!
        reproductionTimer = isMale ? maleCooldown : femaleCooldown;
        SetMode(isHunting);
        
        Debug.Log(gameObject.name + $" finished reproducing. On cooldown for {reproductionTimer} seconds.");
    }
    private void UpdateStatusUI()
    {
        // 1. Update the Slider bar
        if (energyBar != null)
        {
            // Divide current energy by max to get a percentage between 0 and 1
            energyBar.value = Energy / MaxEnergy; 
        }

        // 2. Update the Text based on priority of what the fish is doing
        if (stateText != null)
        {
            string prefix;
            if (isMale)
            {
                prefix = "[Male] ";
            } else
            {
                prefix = "[Female] ";  
            }
            
            if (isCourting) // <-- NEW UI STATE
            {
                stateText.text = prefix + (isMale ? "Displaying for Female..." : "Evaluating Male...");
                stateText.color = Color.yellow;
            }
            else if (isReproducing)
            {
                stateText.text = prefix + (isMale ? "Searching for mate" : "Searching for mate");
                stateText.color = Color.magenta;
            }
           else if (isHunting)
            {
                stateText.text = prefix + "Hunting for ClownFish";
                stateText.color = Color.red;
            }          
            else
            {
                stateText.text = prefix + "Wandering";
                stateText.color = Color.white;
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        // Check if the trigger touched a Clownfish
        if (other.gameObject.CompareTag("ClownFish"))
        {
            MarkEating();
            totalEaten++;
            Energy = MaxEnergy;
            Destroy(other.gameObject); 
            
        }
    }
 private void SetMode(bool huntEnabled)
    {
        if (isReproducing) return;
        if (HuntScript != null) HuntScript.enabled = huntEnabled;
        if (WanderScript != null) WanderScript.enabled = !huntEnabled;
    }

void OnMouseEnter()
    {
        string details = $"<b>Lionfish (Predator)</b>\n" +
                         $"Age: {Age.ToString("F1")}s\n" +
                         $"Energy: {Energy.ToString("F0")}/100\n" +
                         $"Fish Eaten: {totalEaten}" + "\n" +
                         $"Size: {transform.localScale.x.ToString("F2")}\n";
                         

        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(details);
        }
    }

    void OnMouseExit()
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
    
}