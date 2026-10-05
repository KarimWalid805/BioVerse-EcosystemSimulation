using UnityEngine;
using UnityEngine.EventSystems; // Required for mouse hover detection


public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Drag your hidden TooltipBox here")]
    public GameObject tooltipBox;

    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipBox != null)
        {
            tooltipBox.SetActive(true); // Show the tooltip
        }
    }

    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipBox != null)
        {
            tooltipBox.SetActive(false); // Hide the tooltip
        }
    }
}