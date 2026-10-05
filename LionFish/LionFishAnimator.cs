using UnityEngine;

public class FishAnimatorHelper : MonoBehaviour
{
    private Animator animator;
    private Vector3 lastPosition;

    void Start()
    {
        
        animator = GetComponentInChildren<Animator>();
        lastPosition = transform.position;
    }

    void Update()
    {
        if (animator == null) return;

       
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        
      
        float currentSpeed = distanceMoved / Time.deltaTime;

    
        animator.SetFloat("Speed", currentSpeed);

      
        lastPosition = transform.position;
    }
}