using UnityEngine;
using System.Collections; 

public class CFishEgg : MonoBehaviour
{
    public bool isFertilized = false;

    [Header("Hatching Settings")]
    public GameObject clownfishPrefab;
    public float hatchTime;
    public int numberOfBabies;
    [Tooltip("How big the babies are compared to the adult. ")]
    public float babyScale = 0.1f; 
    public int currentMinBabies; // Tracks the lowest possible babies based on temp
    public int currentMaxBabies; // Tracks the highest possible babies based on temp
    public int finalNumberOfBabies; // The final random result
    public float currentIncubationTimer = 0f;
    private float eggSpeedGenetics; // A value from 0 to 1 that determines how fast this specific egg hatches, based on its "genes". 0 = slow, 1 = fast.
    private float TempCheckTImer;
    private float CheckInterval = 5f; // How often to check for temp change
    public void Fertilize()
    {
        if (isFertilized) return; 
        isFertilized = true;
        eggSpeedGenetics = Random.value;
        Renderer eggRenderer = GetComponent<Renderer>();
        if (eggRenderer != null)
        {
            eggRenderer.material.color = Color.green; 
        }

       
        StartCoroutine(HatchRoutine());
    }

private void Update()
    {
        float currentTemp = EcosystemDashboard.GlobalTemperature;

         TempCheckTImer += Time.deltaTime;

        if (TempCheckTImer >= CheckInterval) // Check every 5 seconds
        {
        float exponent = 3f; 
        TempCheckTImer = 0f; // Reset timer
        if (isFertilized)
        {
             
            float curvedStress;
            if (currentTemp <= 27f)
            {
                // Stress goes from 0 (at 27C) up to 1 (at 20C)
                float stress = Mathf.InverseLerp(27f, 20f, currentTemp);
                curvedStress = Mathf.Pow(stress, exponent);  
            }
            else
            {
                // Stress goes from 0 (at 27C) up to 1 (at 35C)
                float stress = Mathf.InverseLerp(27f, 35f, currentTemp);
                curvedStress = Mathf.Pow(stress, exponent);
            }
            // Determine the absolute fastest and slowest this egg COULD hatch at the current temperature
            float currentMinHatch = Mathf.Lerp(70f, 150f, curvedStress);
            float currentMaxHatch = Mathf.Lerp(120f, 200f, curvedStress);

            // Apply this specific egg's genetic speed to the dynamic bounds
            hatchTime = Mathf.Lerp(currentMinHatch, currentMaxHatch, eggSpeedGenetics);
                // Babies go from a healthy amount (6) down to barely surviving (1)
                currentMinBabies = Mathf.RoundToInt(Mathf.Lerp(3f, 1f, curvedStress));
                currentMaxBabies = Mathf.RoundToInt(Mathf.Lerp(4f, 2f, curvedStress));
        }
    }
    }
    private IEnumerator HatchRoutine()
    {
        while (currentIncubationTimer < hatchTime)
        {
            currentIncubationTimer += Time.deltaTime;
            yield return null; 
        }

        
        finalNumberOfBabies = Random.Range(currentMinBabies, currentMaxBabies); // Randomly decide how many babies to hatch 
        for (int i = 0; i < finalNumberOfBabies; i++)
        {
            // Calculate spawn position
            Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0.1f, Random.Range(-0.5f, 0.5f));
            Vector3 spawnPosition = transform.position + randomOffset;

            //  Instantiate the fish and save a reference to it in 'newBaby'
            GameObject newBaby = Instantiate(clownfishPrefab, spawnPosition, Quaternion.identity);
            
            //  Set the scale to 30% (or whatever you set in the Inspector)
            newBaby.transform.localScale = new Vector3(babyScale, babyScale, babyScale);
            

            //  Grab the ClownFish script off the newly spawned baby
            ClownFish babyClownFish = newBaby.GetComponent<ClownFish>();
            babyClownFish.Energy = 60f; //baby clownfish start with a bit of energy so they don't immediately die of hunger before they can eat their first algae
            if (babyClownFish != null)
            {
                
                babyClownFish.isMale = true; // Default to male
                
                
            }
        }

        // Destroy the egg shell object
        Destroy(gameObject);
    }
}