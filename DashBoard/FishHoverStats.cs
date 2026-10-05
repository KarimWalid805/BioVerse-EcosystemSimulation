using UnityEngine;

public class FishHoverStats : MonoBehaviour
{
    [Header("Fish Identity")]
    public string speciesName = "Clownfish";

    [Header("Live Stats")]
    public float currentEnergy = 100f;
    public float size = 1f;
    
    private float timeAlive = 0f;

    void Update()
    {
        // Track how long this specific fish has been alive
        timeAlive += Time.deltaTime;
        
        // (If energy drains over time, you would subtract it here too)
        // currentEnergy -= Time.deltaTime * 2f; 
    }

    
    void OnMouseEnter()
    {
        // Format the data into a nice looking string (\n means "new line")
        string details = $"<b>{speciesName}</b>\n" +
                         $"Time Alive: {timeAlive.ToString("F1")}s\n" +
                         $"Energy: {currentEnergy.ToString("F0")}/100\n" +
                         $"Size: {size.ToString("F2")}";

        // Tell the Tooltip Manager to show it!
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(details);
        }
    }

    // Unity automatically runs this when the mouse leaves the 3D object
    void OnMouseExit()
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}