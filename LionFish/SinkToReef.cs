using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SinkToReef : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Set this to your Ground/Terrain layer so they don't hit other fish!")]
    public LayerMask groundMask; 

    void Start()
    {
        StartCoroutine(DiveRoutine());
    }

    private IEnumerator DiveRoutine()
    {
        // 1. Grab components
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        LionFish brain = GetComponent<LionFish>();
        
        // 2. Freeze the brain and agent
        if (agent != null) agent.enabled = false;
        if (brain != null) brain.enabled = false;

        // 3. Find the floor directly beneath the baby
        Vector3 targetFloor = transform.position;
        targetFloor.y = 0; // Failsafe
        
        
       if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 200f, groundMask, QueryTriggerInteraction.Ignore))
        {
            targetFloor = hit.point;
        }

        // 4. Swim downwards smoothly
        while (Vector3.Distance(transform.position, targetFloor) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetFloor, 3f * Time.deltaTime);
            
            Vector3 direction = (targetFloor - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            }
            
            yield return null;
        }

        // 5. Flatten out on the floor
       if (NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, 10f, NavMesh.AllAreas))
        {
            // Successfully found the floor! Snap perfectly to it.
            transform.position = navHit.position;
        }
       
        // 7. Turn the brain back on!
        if (agent != null) agent.enabled = true;
        if (brain != null) brain.enabled = true;

        // 8. Destroy this specific script
        Destroy(this);
    }
}