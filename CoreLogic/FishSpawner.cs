using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class FishSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject anemonePrefab;
    public GameObject clownfishPrefab;
    public GameObject lionfishPrefab;

    [Header("Spawning Rules")]
    public int amountToSpawn = 5;
    public float minAnemoneSpacing = 20f; // Prevents anemones from spawning on top of each other
    public float fishSpawnDistance = 10f; // Spawns fish exactly 10 units away
    public float maxSlopeAngle = 5f;

    [Header("Map Boundaries")]
    public float minX = 20f, maxX = 100f;
    public float minZ = 0f, maxZ = 80f;

    void Start()
    {
        SpawnAnemones();
        SpawnAnemones();
        Vector3 randomPos = new Vector3 (Random.Range(minX, maxX), 100f, Random.Range(minZ, maxZ));
        SpawnLionfishCouple(randomPos);
        SpawnLionfishCouple(randomPos);
        SpawnLionfishCouple(randomPos);
        
    }

   private void SpawnAnemones()
    {
        List<Vector3> placedPositions = new List<Vector3>();
        int attempts = 0;

        // Failsafe loop to find valid spots
        while (placedPositions.Count < amountToSpawn && attempts < 200)
        {
            attempts++;
            Vector3 testPos = new Vector3(Random.Range(minX, maxX), 100f, Random.Range(minZ, maxZ));

            // Raycast down to find the floor
            if (Physics.Raycast(testPos, Vector3.down, out RaycastHit hit, 200f))
            {
              
                // Calculate the angle between the world's "Up" and the direction the ground is facing
                float groundSlopeAngle = Vector3.Angle(Vector3.up, hit.normal);

                // If the slope is steeper than our allowed maximum, skip this spot and try again!
                if (groundSlopeAngle > maxSlopeAngle)
                {
                    continue; 
                }

                // Check if it's too close to an already spawned anemone
                bool tooClose = false;
                foreach (Vector3 pos in placedPositions)
                {
                    if (Vector3.Distance(hit.point, pos) < minAnemoneSpacing)
                    {
                        tooClose = true; 
                        break;
                    }
                }

                if (!tooClose)
                {
                    placedPositions.Add(hit.point);
                    
                    // 1. Spawn the Anemone
                    GameObject newAnemone = Instantiate(anemonePrefab, hit.point, Quaternion.identity);
                    AnemoneManager manager = newAnemone.GetComponent<AnemoneManager>();
                    
                    // 2. Spawn the Couple 10 units away
                    SpawnClownFishCouple(hit.point, manager);
                }
            }
        }
        
        if (placedPositions.Count < amountToSpawn)
        {
            Debug.LogWarning($"Only spawned {placedPositions.Count} out of {amountToSpawn} anemones. Couldn't find enough flat, spaced-out ground!");
        }
    }

    private void SpawnClownFishCouple(Vector3 anemonePos, AnemoneManager manager)
    {
        // Calculate offsets 10 units away on the X and Z axes
        Vector3 femalePos = anemonePos + new Vector3(fishSpawnDistance, 0, 0);
        Vector3 malePos = anemonePos + new Vector3(0, 0, fishSpawnDistance); 

        SpawnSingleClownFish(femalePos, manager, false);
        SpawnSingleClownFish(malePos, manager, true);
    }

     private void SpawnLionfishCouple(Vector3 anemonePos)
    {
        
        Vector3 femalePos = GetValidRandomFloorPosition();
        Vector3 malePos = GetValidRandomFloorPosition();

        SpawnSingleLionFish(femalePos,  false);
        SpawnSingleLionFish(malePos,  true);
    }
    private Vector3 GetValidRandomFloorPosition()
    {
        Vector3 testPos = new Vector3(Random.Range(minX, maxX), 100f, Random.Range(minZ, maxZ));

        // Raycast straight down to hit the ground
        if (Physics.Raycast(testPos, Vector3.down, out RaycastHit hit, 200f))
        {
            return hit.point; // Return the exact floor position!
        }
        
        return Vector3.zero; // Failsafe
    }
    private void SpawnSingleClownFish(Vector3 roughPos, AnemoneManager manager, bool isMale)
    {
        // Snap the offset position safely to the NavMesh
        if (NavMesh.SamplePosition(roughPos, out NavMeshHit navHit, 5f, NavMesh.AllAreas))
        {
            GameObject fish = Instantiate(clownfishPrefab, navHit.position, Quaternion.identity);
            fish.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            
            ClownFish script = fish.GetComponent<ClownFish>();
            if (script != null)
            {
                script.isMale = isMale;
                script.AssignHome(manager); // Instantly registers them to their new home
            }
        }
    }
     private void SpawnSingleLionFish(Vector3 roughPos,  bool isMale)
    {
        // Snap the offset position safely to the NavMesh
        if (NavMesh.SamplePosition(roughPos, out NavMeshHit navHit, 5f, NavMesh.AllAreas))
        {
            GameObject fish = Instantiate(lionfishPrefab, navHit.position, Quaternion.identity);
            float fishSize;

            if (isMale)
            {
                fishSize = 1f; 
            }
            else
            {
                fishSize = 0.8f; 
            }
            fish.transform.localScale = new Vector3(fishSize, fishSize, fishSize);
             
            LionFish script = fish.GetComponent<LionFish>();
            if (script != null)
            {
                script.isMale = isMale;
            }
        }
    }
}