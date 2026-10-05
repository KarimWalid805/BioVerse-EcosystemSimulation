using UnityEngine;
using UnityEngine.AI;

public class SlopeSnap : MonoBehaviour
{
    private NavMeshAgent navAgent;

    [Header("Settings")]
    public float rotationSpeed = 5f;
    public LayerMask groundMask;
    public float raycastDistance = 2f; // How far down to check for terrain

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.updateRotation = false; 
    }

    void Update()
    {
        SurfaceAlignment();
    }

    public void SurfaceAlignment()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        Vector3 flatForward = navAgent.velocity.sqrMagnitude > 0.01f ? navAgent.velocity : transform.forward;
        flatForward.y = 0; // Flatten it perfectly horizontally
        
  
        if (flatForward.sqrMagnitude < 0.001f)
        {
            return; // Abort this frame! Don't try to rotate if we have no valid direction.
        }
        
        flatForward.Normalize();

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundMask))
        {
            Vector3 slopedForward = Vector3.ProjectOnPlane(flatForward, hit.normal);

            
            if (slopedForward.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(slopedForward.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // If the fish is swimming high above the ground, just level it out normally
            Quaternion flatRotation = Quaternion.LookRotation(flatForward, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, flatRotation, rotationSpeed * Time.deltaTime);
        }
    }
}