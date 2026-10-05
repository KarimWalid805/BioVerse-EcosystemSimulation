using UnityEngine;
using UnityEngine.UI; // Required to modify the Image component!
using System.Collections;

public class UITogglePanel : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag your main Dashboard/Canvas panel here")]
    public RectTransform panelToSlide;
    
    [Header("Button Visuals")]
    [Tooltip("Drag the Image component of your Button here")]
    public Image buttonImage;
    [Tooltip("The arrow sprite to show when the panel is OPEN")]
    public Sprite arrowOpen;
    [Tooltip("The arrow sprite to show when the panel is CLOSED")]
    public Sprite arrowClosed;
    
    [Header("Animation Settings")]
    [Tooltip("How fast the panel slides in and out")]
    public float slideSpeed = 10f;

    // The script will calculate these automatically now!
    private Vector2 openPosition;
    private Vector2 closedPosition;

    private bool isOpen = true; 
    private Coroutine slideCoroutine;

    void Start()
    {
       
        openPosition = panelToSlide.anchoredPosition;

       
        closedPosition = new Vector2(openPosition.x + panelToSlide.rect.width, openPosition.y);

       
        if (buttonImage != null)
        {
            buttonImage.sprite = arrowOpen;
        }
    }

    public void TogglePanel()
    {
        isOpen = !isOpen;
        Vector2 targetPosition = isOpen ? openPosition : closedPosition;

       
        if (buttonImage != null)
        {
            
            buttonImage.sprite = isOpen ? arrowOpen : arrowClosed;
        }

        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
        }
        
        slideCoroutine = StartCoroutine(Slide(targetPosition));
    }

    private IEnumerator Slide(Vector2 targetPos)
    {
        while (Vector2.Distance(panelToSlide.anchoredPosition, targetPos) > 0.5f)
        {
            panelToSlide.anchoredPosition = Vector2.Lerp(panelToSlide.anchoredPosition, targetPos, Time.unscaledDeltaTime * slideSpeed);
            yield return null;
        }
        
        panelToSlide.anchoredPosition = targetPos;
    }
}