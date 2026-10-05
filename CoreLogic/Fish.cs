using UnityEngine;
using System;
public class Fish : MonoBehaviour
{
    [Header("Fish Properties")]


    //Enery-related properties
    public float Energy = 100f;
    [NonSerialized]
    public float MaxEnergy = 100f;
    public float MinEnergy = 0f;
    public float ExponentialDecayRate = 0.05f;
    public float LinearDecayRate = 0.5f;
    public float huntThreshold = 50f;
    protected float eatingPauseTimer = 0f;
    public float LifeSpan; // Total lifespan in seconds
    public float Age = 0f; // Current age

   

    




    protected virtual void Update()
    {
        if (eatingPauseTimer > 0f)
        {
            eatingPauseTimer -= Time.deltaTime;
        }
        else
        {
            // Exponential decay toward MinEnergy. 
            //Energy *= Mathf.Exp(-ExponentialDecayRate * Time.deltaTime);

            // Linear decay toward MinEnergy.
            Energy = Energy - LinearDecayRate * Time.deltaTime;
        }

        Energy = Mathf.Clamp(Energy, MinEnergy, MaxEnergy);
    }

    public virtual void EatFood()
    {
      
        Energy += 10f;
        Energy = Mathf.Clamp(Energy, MinEnergy, MaxEnergy);

    }

    protected void MarkEating(float pauseSeconds = 0.2f)
    {
        if (pauseSeconds > eatingPauseTimer)
        {
            eatingPauseTimer = pauseSeconds;
        }
    }

  
}
