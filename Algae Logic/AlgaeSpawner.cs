using UnityEngine;

public class AlgaeSpawner : MonoBehaviour
{
    public GameObject algaePrefab;
    private float dynamicInterval;
    [Header("Spawn Settings")]
    [Tooltip("The absolute maximum number of algae allowed in the tank at one time")]
    public int maxAlgaeAllowed = 100; // <-- NEW SETTING
    private float spawnTimer = 0f;
    public float spawnInterval = 5f;
    private float TempCheckTImer;
    private float CheckInterval = 5f; // How often to check for temp change
    float StartingAlgaeCount;

    [Header("Spawn Area")]
    public float minX = 26f;
    public float maxX = 71f;
    public float minZ = -8f;
    public float maxZ = 81f;

    void Start()
    {
        
        
       for (int i = 0; i < 70; i++) 
        {
            SpawnSingleAlgae();
        }
      
    }
void Update()
    {
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
            dynamicInterval = Mathf.Lerp(5f, 20f, curvedStress);
        
        }
        else
        {
            // InverseLerp from 27 up to 35. 
            // Result: 0.0 at 27°C (optimal), 1.0 at 35°C (extreme stress)
            float stress = Mathf.InverseLerp(27f, 35f, currentTemp);
            
            // Apply the exponential curve
            float curvedStress = Mathf.Pow(stress, exponent);
            
            // Lerp from the base interval (5s) UP to the max interval (20s)
            dynamicInterval = Mathf.Lerp(5f, 20f, curvedStress);
            
        }
        
    }
    
    spawnTimer += Time.deltaTime;
        
    if (spawnTimer >= dynamicInterval)
    {
        spawnTimer = 0f;
        SpawnSingleAlgae();
    }
    
    }
    void SpawnSingleAlgae()
    {
        
        // Count how many red algae currently exist in the scene
        GameObject[] currentAlgae = GameObject.FindGameObjectsWithTag("RedAlgae");
        
        // If we have reached or exceeded the limit, stop and do nothing this cycle!
        if (currentAlgae.Length >= maxAlgaeAllowed)
        {
            
            return; 
        }

        // 1. Pick a random X and Z coordinate
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);

        // 2. Start a raycast high up in the sky at that coordinate
        Vector3 rayStart = new Vector3(randomX, 100f, randomZ);

        // 3. Shoot the laser straight down to hit the Terrain
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 200f))
        {
            // Make sure we actually hit the ground and not a fish!
            if (hit.collider.CompareTag("Terrain"))
            {
                // Spawn the algae exactly where the laser hit the dirt
                GameObject newAlgae = Instantiate(algaePrefab, hit.point, Quaternion.identity);
                newAlgae.tag = "RedAlgae";
                
                // This makes the plant tilt with the hill.
                newAlgae.transform.up = hit.normal;
                
                // Give it a random spin so they don't all look identical
                newAlgae.transform.Rotate(0, Random.Range(0, 360f), 0, Space.Self);
            }
        }
    }
}