using UnityEngine;
using System.Collections; 

public class LFishEgg : MonoBehaviour
{
    public bool isFertilized = false;

    [Header("Hatching Settings")]
    public GameObject lionFishPrefab;
    public float hatchTime = 10f;
    [Tooltip("How many babies to hatch. This will be randomized based on environmental conditions.")]
    public int numberOfBabies;
    [Tooltip("How big the babies are compared to the adult. ")]
    public float babyScale = 0.1f; 

    public int currentMinBabies; 
    public int currentMaxBabies; 
    public int finalNumberOfBabies; 
    public float currentIncubationTimer = 0f; 
    public float currentTemp = 27f; 
    public float TempCheckTImer = 0f;
    private float CheckInterval = 5f;
    private float eggSpeedGenetics;
    [Header("Ocean Current Settings")]
    [Tooltip("How fast the ocean current pushes the eggs")]
    public float driftSpeed = 0.5f; 
    private Vector3 driftDirection;

    [Header("Hunt Boundaries")]
    public float minX = 26f;
    public float maxX = 122f;
    public float minZ = -8f;
    public float maxZ = 81f;
    void Start()
    {
        // Pick ONE initial random direction to float, and never change it manually again!
        PickNewDriftDirection();
    }
    void PickNewDriftDirection()
    {
        // Pick a random angle (0 to 360 degrees)
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        
        // Convert that angle into an X and Z direction (Y is 0 so it stays perfectly flat)
        driftDirection = new Vector3(Mathf.Cos(randomAngle), 0f, Mathf.Sin(randomAngle)).normalized;
    }
    public void Fertilize()
    {
        if (isFertilized) return; 
        isFertilized = true;
        eggSpeedGenetics = Random.value;
        Renderer eggRenderer = GetComponent<Renderer>();
        if (eggRenderer != null)
        {
            eggRenderer.material.color = Color.white; 
        }

      
        StartCoroutine(HatchRoutine());
    }
    void Update()
    {
        currentTemp = EcosystemDashboard.GlobalTemperature;
        TempCheckTImer += Time.deltaTime;
        if (TempCheckTImer >= CheckInterval)
        if (isFertilized)
        {
            float exponent = 3f; 
            TempCheckTImer = 0f;
            float curvedStress = 0f;

            if (currentTemp <= 27f)
            {
                float stress = Mathf.InverseLerp(27f, 20f, currentTemp);
                curvedStress = Mathf.Pow(stress, exponent);
            }
            else
            {
                float stress = Mathf.InverseLerp(27f, 35f, currentTemp);
                curvedStress = Mathf.Pow(stress, exponent);
            }

            float currentMinHatch = Mathf.Lerp(10f, 20f, curvedStress);
            float currentMaxHatch = Mathf.Lerp(15f, 30f, curvedStress);
            hatchTime = Mathf.Lerp(currentMinHatch, currentMaxHatch, eggSpeedGenetics);

            currentMinBabies = Mathf.RoundToInt(Mathf.Lerp(3f, 1f, curvedStress));
            currentMaxBabies = Mathf.RoundToInt(Mathf.Lerp(5f, 2f, curvedStress)); 
        }
        // 1. Figure out exactly where the egg is trying to go next
        Vector3 nextPosition = transform.position + (driftDirection * driftSpeed * Time.deltaTime);

        // 2. Did it hit the Left or Right wall? Reverse the X direction!
        if (nextPosition.x <= minX || nextPosition.x >= maxX)
        {
            driftDirection.x *= -1; 
        }

        // 3. Did it hit the Top or Bottom wall? Reverse the Z direction!
        if (nextPosition.z <= minZ || nextPosition.z >= maxZ)
        {
            driftDirection.z *= -1;
        }

        // 4. Safely move the egg
        transform.position += driftDirection * driftSpeed * Time.deltaTime;
        
        // Failsafe: Teleport them back inside if they somehow spawn outside the box!
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedZ = Mathf.Clamp(transform.position.z, minZ, maxZ);
        transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
    }

    private IEnumerator HatchRoutine()
    {
        yield return new WaitForSeconds(hatchTime);

      
        finalNumberOfBabies = Random.Range(currentMinBabies, currentMaxBabies); // Randomly decide how many babies to hatch 
        for (int i = 0; i < finalNumberOfBabies; i++)
        {
            // Calculate spawn position
            Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), 0.1f, Random.Range(-2f, 2f));
            Vector3 spawnPosition = transform.position + randomOffset;

            // 1. Instantiate the fish and save a reference to it in 'newBaby'
            GameObject newBaby = Instantiate(lionFishPrefab, spawnPosition, Quaternion.identity);
            newBaby.AddComponent<SinkToReef>();
            // 2- Set the scale to 30% (or whatever you set in the Inspector)
            newBaby.transform.localScale = new Vector3(babyScale, babyScale, babyScale);
            

            // 2. Grab the LionFish script off the newly spawned baby
            LionFish babyLionFish = newBaby.GetComponent<LionFish>();
            babyLionFish.Energy = 60f; //baby lionfish start with a bit of energy so they don't immediately die of hunger before they can eat their first clownfish
            if (babyLionFish != null)
            {
                babyLionFish.Energy = 60f;
                babyLionFish.isMale = Random.value < 0.4f; // Default to male or female
            }
        }

        // Destroy the egg shell object
        Destroy(gameObject);
    }
}