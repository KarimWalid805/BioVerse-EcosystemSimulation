using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Header("Time Settings")]
    public float fastForwardSpeed = 4f;

    public float ExtraFastForwardSpeed => fastForwardSpeed * 2f; 
    private bool isFastForwarding = false;

    void Update()
    {
     
        if (Input.GetKeyDown(KeyCode.F))
        {
            isFastForwarding = !isFastForwarding;

            if (isFastForwarding)
            {
                Time.timeScale = fastForwardSpeed;
                Debug.Log("Fast Forward: ON (" + fastForwardSpeed + "x)");
            }
            else
            {
                Time.timeScale = 1f; 
                Debug.Log("Fast Forward: OFF (1x)");
            }
        }

       
        if (Input.GetKeyDown(KeyCode.G))
        {
            Time.timeScale = ExtraFastForwardSpeed;
            isFastForwarding = true; 
            Debug.Log("Extra Fast Forward: ON (" + ExtraFastForwardSpeed + "x)");
        }
    }
}