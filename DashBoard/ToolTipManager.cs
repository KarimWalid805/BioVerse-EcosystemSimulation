using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("UI References")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI infoText;

    public Vector2 mouseOffset = new Vector2(15f, -15f); 

    void Awake()
    {
        // Set up the Singleton
        Instance = this;
        tooltipPanel.SetActive(false);
    }

    void Update()
    {
  
        if (tooltipPanel.activeSelf)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tooltipPanel.transform.parent.GetComponent<RectTransform>(), 
                Input.mousePosition, 
                null, 
                out localPoint);

            tooltipPanel.transform.localPosition = localPoint + mouseOffset;
        }
    }

    // Fish will call this when hovered
    public void ShowTooltip(string fishData)
    {
        infoText.text = fishData;
        tooltipPanel.SetActive(true);
    }

    // Fish will call this when the mouse leaves
    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}