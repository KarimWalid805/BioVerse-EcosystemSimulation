using UnityEngine;

public class AlgaeGrowth : MonoBehaviour
{
    [Header("Growth")]
    [Tooltip("How much it grows per second")]
    public float growSpeed = 0.004f;

    [Header("Being Eaten")]
    [Tooltip("How much it shrinks per second while a fish is touching it")]
    public float shrinkSpeed = 0.05f;
    [Tooltip("How long after the fish leaves before it starts growing back")]
    public float eatingStateHoldSeconds = 0.2f;
    private float TempCheckTImer;
    private float CheckInterval = 5f; // How often to check for temp change
    private Vector3 finalSize;
    private float eatingTimer;
    public bool IsBeingEaten => eatingTimer > 0f;

    void Start()
    {
        // Remember how big the plant is supposed to be
        finalSize = transform.localScale;
        
        // Shrink it down to zero immediately when it spawns
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        float currentTemp = EcosystemDashboard.GlobalTemperature;
        float dynamicGrowSpeed;
        TempCheckTImer += Time.deltaTime;

        if (TempCheckTImer >= CheckInterval) // Check every 5 seconds
        {
             
            TempCheckTImer = 0f;
            if (currentTemp <= 27f)
            {
                // From 20 to 27 degrees: Growth speed goes from slow (0.001) to fast (0.01)
                float heatPercentage = Mathf.InverseLerp(20f, 27f, currentTemp);
                dynamicGrowSpeed = Mathf.Lerp(0.001f, 0.01f, heatPercentage);
            }
            else
            {
                // From 27 to 35 degrees: Growth speed drops from fast (0.01) to medium-slow (0.003)
                float heatPercentage = Mathf.InverseLerp(27f, 35f, currentTemp);
                dynamicGrowSpeed = Mathf.Lerp(0.01f, 0.003f, heatPercentage);
            }
        }
        // 1. Manage the timer
        if (eatingTimer > 0f)
        {
            eatingTimer -= Time.deltaTime;
        }

        // 2. Handle the shrinking and growing smoothly!
        if (IsBeingEaten)
        {
            // MoveTowards shrinks it at a perfectly steady, constant rate
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, shrinkSpeed * Time.deltaTime);

            // Destroy if it gets tiny enough to be invisible
            if (transform.localScale.x <= 0.05f)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // If not being eaten, slowly grow back to full size
            transform.localScale = Vector3.MoveTowards(transform.localScale, finalSize, growSpeed * Time.deltaTime);
        }
    }

    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("ClownFish"))
        {
           
            eatingTimer = eatingStateHoldSeconds;
        }
    }

  
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("ClownFish"))
        {
            eatingTimer = eatingStateHoldSeconds;
        }
    }
}