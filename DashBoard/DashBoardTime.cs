using UnityEngine;
using TMPro; // (Change this to 'using TMPro;' if you are using TextMeshPro)
using System; // Required to read your computer's actual clock!

public class DashboardTime : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag your RealTimeText here")]
    public TextMeshProUGUI realTimeText; 
    
    [Tooltip("Drag your SimTimeText here")]
    public TextMeshProUGUI simTimeText;

    [Header("Simulation Settings")]
    [Tooltip("How much faster does simulation time pass? (1 = normal speed)")]
    public float simTimeMultiplier = 1f;

    // This hidden variable tracks the total simulated time
    private float totalSimulatedSeconds = 0f;

    void Update()
    {

        


     
        totalSimulatedSeconds += Time.deltaTime * simTimeMultiplier;

        // TimeSpan easily converts raw seconds into Minutes and Seconds
        TimeSpan simTime = TimeSpan.FromSeconds(totalSimulatedSeconds);

        if (simTimeText != null)
        {
         
            simTimeText.text = string.Format(" {0:D2}:{1:D2}", simTime.Minutes, simTime.Seconds);
        }
    }
}