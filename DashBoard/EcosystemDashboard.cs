using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.IO;

public class EcosystemDashboard : MonoBehaviour
{
    [Header("UI Text - Populations")]
    public TextMeshProUGUI ClownText;
    public TextMeshProUGUI LionText;
    public TextMeshProUGUI AlgaeText;
    public TextMeshProUGUI TempText;
    [Header("Environment Sliders")]
    public Slider temperatureSlider;
    public static float GlobalTemperature = 25f;
    [Header("UI Text - Ecology Metrics")]
    public TextMeshProUGUI shannonIndexText;
    public TextMeshProUGUI dominanceIndexText;
    private bool isPaused = false;
    [Header("Graph Settings")]
    public RectTransform graphContainer;
    public int maxDataPoints = 50;       // How many seconds of history to show
    public float sampleInterval = 1f;    // Take a data snapshot every 1 second
    private float timer = 0f;
    [Header("Temperature Visuals")]
public UnityEngine.UI.Image sliderHandleImage; 
public Gradient temperatureGradient;           

    [Header("Simulation State")]
    public GameObject beginButton;
    public float fastForwardSpeed = 4f;

   [Header("Button Icon Objects")]
[Tooltip("Drag the PlayIcon GameObject here")]
public GameObject playIconObject;

[Tooltip("Drag the StopIcon GameObject here")]
public GameObject stopIconObject;     // The Stop square / Pause bars
    
    private bool isPlaying = false;

    public float ExtraFastForwardSpeed => fastForwardSpeed * 2f; // A helper property for even faster speeds if needed
    private bool isFastForwarding = false;
    
    [Header("Graph Colors")]
    public Color clownColor = new Color(1f, 0.5f, 0f); // Orange
    public Color lionColor = Color.red;
    public Color algaeColor = Color.green;

    // The UI Graph History Lists (These delete old data)
    private List<int> clownHistory = new List<int>();
    private List<int> lionHistory = new List<int>();
    private List<int> algaeHistory = new List<int>();
    private List<GameObject> drawnLines = new List<GameObject>();

    // NEW: The "Black Box" Permanent History Lists (These never delete data!)
    [HideInInspector] public List<int> fullClownHistory = new List<int>();
    [HideInInspector] public List<int> fullLionHistory = new List<int>();
    [HideInInspector] public List<int> fullAlgaeHistory = new List<int>();
    // NEW: Statistical Trackers for the Final Report
    private int maxClown = 0, minClown = int.MaxValue;
    private int maxLion = 0, minLion = int.MaxValue;
    private int maxAlgae = 0, minAlgae = int.MaxValue;
    private long sumClown = 0, sumLion = 0, sumAlgae = 0;

    void Start()
    {
        // Freeze time the exact millisecond the scene loads!
        Time.timeScale = 0f;
        // Ensure the button shows the Play icon when the scene starts
        if (playIconObject != null && stopIconObject != null)
    {
        playIconObject.SetActive(true);   // Turn Play ON
        stopIconObject.SetActive(false);  // Turn Stop OFF
    }
        if (TempText != null)
        {
            TempText.text = "Temperature: " + GlobalTemperature.ToString("F1") + "°C";
        }
        if (temperatureSlider != null)
        {
            temperatureSlider.minValue = 20f;
            temperatureSlider.maxValue = 35f;
            temperatureSlider.value = 27f; // Forces the handle to start at 27
        }
        GlobalTemperature = 27f;
    }
public void FastForwardShortcuts()
{
    // Toggle fast forward when you press the 'F' key
        if (Input.GetKeyDown(KeyCode.F))
        {
            isFastForwarding = !isFastForwarding;

            if (isFastForwarding)
            {
                // Speed up time
                Time.timeScale = fastForwardSpeed;
                Debug.Log("Fast Forward: ON (" + fastForwardSpeed + "x)");
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                // Even faster time
                Time.timeScale = ExtraFastForwardSpeed;
                Debug.Log("Extra Fast Forward: ON (" + ExtraFastForwardSpeed + "x)");
            }
            else
            {
                // Return to normal time
                Time.timeScale = 1f; 
                Debug.Log("Fast Forward: OFF (1x)");
            }
        }
}
public void FastForwardButton()
{
    // Toggle fast forward when you press the 'F' key
       
            isFastForwarding = !isFastForwarding;

            if (isFastForwarding)
            {
                // Speed up time
                Time.timeScale = fastForwardSpeed;
                Debug.Log("Fast Forward: ON (" + fastForwardSpeed + "x)");
            }
            else
            {
                // Return to normal time
                Time.timeScale = 1f; 
                Debug.Log("Fast Forward: OFF (1x)");
            }
        
}
    public void BeginSimulation()
    {
        Time.timeScale = 1f; 
        beginButton.SetActive(false); 
    }
    void Update()
    {
   
        timer += Time.deltaTime;
        float safeInterval = Mathf.Max(1f, sampleInterval);
        if (timer >= safeInterval)
        {
            timer = 0f;
            TakeSnapshot();
        }
    }
    public void OnTemperatureSliderChanged()
    {
        
        GlobalTemperature = temperatureSlider.value;
        TempText.text = "Temperature: " + GlobalTemperature.ToString("F1") + "°C";
        if (sliderHandleImage != null)
    {
        sliderHandleImage.color = temperatureGradient.Evaluate(temperatureSlider.normalizedValue);
    }
    }
public void ResetSimulation()
{
    isPlaying = !isPlaying; // Flip the state 

    if (isPlaying)
    {
        Time.timeScale = 1f; // Unfreeze time
        
        // Turn Play OFF, turn Stop ON
        if (playIconObject != null && stopIconObject != null)
        {
            playIconObject.SetActive(false);
            stopIconObject.SetActive(true);
        }
    }
    else
    {
        // 1. Clear the Black Box data
        fullClownHistory.Clear();
        fullLionHistory.Clear();
        fullAlgaeHistory.Clear();

        // 2. Reset the statistical trackers
        maxClown = maxLion = maxAlgae = 0;
        minClown = minLion = minAlgae = int.MaxValue;
        sumClown = sumLion = sumAlgae = 0;

        // 3. Reload the current scene to reset everything
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
    public void TogglePause()
{
    isPaused = !isPaused; // Flip the switch

    if (isPaused)
    {
        // 1. Freeze time
        Time.timeScale = 0f; 
        Debug.Log("Simulation Paused.");

        // Visuals: Show the PLAY icon (to indicate they can resume)
        if (playIconObject != null) playIconObject.SetActive(true);
        if (stopIconObject != null) stopIconObject.SetActive(false);
    }
    else
    {
        // 2. Smart Unpause
        if (isFastForwarding)
        {
            Time.timeScale = fastForwardSpeed;
            Debug.Log("Simulation Resumed: Fast Forward (" + fastForwardSpeed + "x)");
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("Simulation Resumed: Normal (1x)");
        }

        // Visuals: Show the STOP/PAUSE icon (to indicate it is running)
        if (playIconObject != null) playIconObject.SetActive(false);
        if (stopIconObject != null) stopIconObject.SetActive(true);
    }
}
   
    private void TakeSnapshot()
    {
        // 1. Count the basic populations
        GameObject[] clownObjects = GameObject.FindGameObjectsWithTag("ClownFish");
        int clownCount = clownObjects.Length;
        int lionCount = GameObject.FindGameObjectsWithTag("LionFish").Length;
        int algaeCount = GameObject.FindGameObjectsWithTag("RedAlgae").Length;

        // NEW: Update Stats Math
        if (clownCount > maxClown) maxClown = clownCount;
        if (clownCount < minClown) minClown = clownCount;
        sumClown += clownCount;

        if (lionCount > maxLion) maxLion = lionCount;
        if (lionCount < minLion) minLion = lionCount;
        sumLion += lionCount;

        if (algaeCount > maxAlgae) maxAlgae = algaeCount;
        if (algaeCount < minAlgae) minAlgae = algaeCount;
        sumAlgae += algaeCount;

        // Save to the permanent Black Box
        fullClownHistory.Add(clownCount);
        fullLionHistory.Add(lionCount);
        fullAlgaeHistory.Add(algaeCount);

        // ... Keep the rest of your original method (Updating texts, calling CalculateIndices, and DrawGraph) ...
        
        if (ClownText != null) ClownText.text = "Clownfish Population: " + clownCount;
        if (LionText != null) LionText.text = "Lionfish Population: " + lionCount;
        if (AlgaeText != null) AlgaeText.text = "Algae Population: " + algaeCount;

        AddDataToHistory(clownHistory, clownCount);
        AddDataToHistory(lionHistory, lionCount);
        AddDataToHistory(algaeHistory, algaeCount);

        CalculateIndices(clownCount, lionCount, algaeCount);
        DrawGraph();
    }

    private void AddDataToHistory(List<int> list, int value)
    {
        list.Add(value);
        if (list.Count > maxDataPoints)
        {
            list.RemoveAt(0); // Remove the oldest point so the graph scrolls horizontally
        }
    }

    private void CalculateIndices(int c, int l, int a)
    {
        float total = c + l + a;
        if (total == 0) return; // Prevent dividing by zero if everything dies

        // --- SHANNON INDEX ---
        float pC = c / total;
        float pL = l / total;
        float pA = a / total;

        float shannon = 0f;
        if (pC > 0) shannon -= pC * Mathf.Log(pC);
        if (pL > 0) shannon -= pL * Mathf.Log(pL);
        if (pA > 0) shannon -= pA * Mathf.Log(pA);

        if (shannonIndexText != null) shannonIndexText.text = "Shannon Index: " + shannon.ToString("F2");

        // --- DOMINANCE INDEX (Simpson's D) ---
        float dominance = (pC * pC) + (pL * pL) + (pA * pA);
        if (dominanceIndexText != null) dominanceIndexText.text = "Dominance: " + dominance.ToString("F2");
    }

    // ==========================================
    // GRAPH DRAWING LOGIC
    // ==========================================

    private void DrawGraph()
    {
        // Destroy old lines
        foreach (GameObject line in drawnLines) Destroy(line);
        drawnLines.Clear();

        // Find the absolute highest number right now so the graph fits the box perfectly
        int maxPopulation = 10; // Minimum ceiling so the graph doesn't glitch if there's only 1 fish
        maxPopulation = Mathf.Max(maxPopulation, GetMaxValue(clownHistory));
        maxPopulation = Mathf.Max(maxPopulation, GetMaxValue(lionHistory));
        maxPopulation = Mathf.Max(maxPopulation, GetMaxValue(algaeHistory));

        // Draw the 3 lines
        DrawLineGraph(clownHistory, clownColor, maxPopulation);
        DrawLineGraph(lionHistory, lionColor, maxPopulation);
        DrawLineGraph(algaeHistory, algaeColor, maxPopulation);
    }

    private void DrawLineGraph(List<int> dataList, Color color, int maxPop)
    {
        if (dataList.Count < 2) return;

        float width = graphContainer.rect.width;
        float height = graphContainer.rect.height;
        float xSpacing = width / (maxDataPoints - 1); // Exact space between dots

        for (int i = 0; i < dataList.Count - 1; i++)
        {
            Vector2 pointA = new Vector2(i * xSpacing, ((float)dataList[i] / maxPop) * height);
            Vector2 pointB = new Vector2((i + 1) * xSpacing, ((float)dataList[i + 1] / maxPop) * height);

            CreateLineSegment(pointA, pointB, color);
        }
    }

    private void CreateLineSegment(Vector2 pointA, Vector2 pointB, Color color)
    {
        GameObject lineObj = new GameObject("GraphLine", typeof(Image));
        lineObj.transform.SetParent(graphContainer, false);
        drawnLines.Add(lineObj);

        Image img = lineObj.GetComponent<Image>();
        img.color = color;

        RectTransform rect = lineObj.GetComponent<RectTransform>();
        Vector2 dir = (pointB - pointA).normalized;
        float distance = Vector2.Distance(pointA, pointB);

        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(distance, 3f); // 3f is line thickness
        rect.anchoredPosition = pointA + dir * distance * 0.5f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rect.localEulerAngles = new Vector3(0, 0, angle);
    }

    private int GetMaxValue(List<int> list)
    {
        int max = 0;
        foreach (int val in list) if (val > max) max = val;
        return max;
    }

    public void ExportFullGraphDocument()
    {
        // 1. Calculate Averages
        int snapshots = fullClownHistory.Count;
        int avgClown = snapshots > 0 ? (int)(sumClown / snapshots) : 0;
        int avgLion = snapshots > 0 ? (int)(sumLion / snapshots) : 0;
        int avgAlgae = snapshots > 0 ? (int)(sumAlgae / snapshots) : 0;

        // 2. Count Ranks LIVE
        int totalKings = 0;
        int totalQueens = 0;
        int totalSpares = 0;
        int totalBabies = 0;

        GameObject[] allClowns = GameObject.FindGameObjectsWithTag("ClownFish");
        foreach (GameObject fish in allClowns)
        {
            ClownFish script = fish.GetComponent<ClownFish>();
            if (script != null)
            {
                if (script.currentRank == ClownFish.FishRank.King) totalKings++;
                else if (script.currentRank == ClownFish.FishRank.Queen) totalQueens++;
                else if (script.currentRank == ClownFish.FishRank.Spare) totalSpares++;
                else totalBabies++;
            }
        }

        int currentTotalLiving = allClowns.Length + GameObject.FindGameObjectsWithTag("LionFish").Length + GameObject.FindGameObjectsWithTag("RedAlgae").Length;

        // 3. Prep Data for Chart
        List<int> timeSteps = new List<int>();
        for (int i = 0; i < snapshots; i++) timeSteps.Add(i);

        string timeData = string.Join(",", timeSteps);
        string clownData = string.Join(",", fullClownHistory);
        string lionData = string.Join(",", fullLionHistory);
        string algaeData = string.Join(",", fullAlgaeHistory);

        // 4. Build the HTML Document
        string htmlContent = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <title>Ecosystem Simulation Report</title>
            <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
            <style>
                body {{ font-family: Arial, sans-serif; background-color: #f4f4f9; padding: 40px; }}
                .container {{ background: white; padding: 20px; border-radius: 10px; box-shadow: 0px 4px 10px rgba(0,0,0,0.1); max-width: 1000px; margin: auto; }}
                h1 {{ color: #333; text-align: center; }}
                .stats-grid {{ display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; margin-bottom: 30px; text-align: center; }}
                .stat-box {{ background: #eee; padding: 15px; border-radius: 8px; }}
                .rank-box {{ background: #fff3cd; padding: 15px; border-radius: 8px; margin-top: 20px; text-align: center; font-weight: bold; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <h1>Ecosystem Full Simulation Report</h1>
                
                <div class='stats-grid'>
                    <div class='stat-box'>
                        <h3>Clownfish Stats</h3>
                        <p>Max: {maxClown} | Min: {(minClown == int.MaxValue ? 0 : minClown)}</p>
                        <p>Average: {avgClown}</p>
                    </div>
                    <div class='stat-box'>
                        <h3>Lionfish Stats</h3>
                        <p>Max: {maxLion} | Min: {(minLion == int.MaxValue ? 0 : minLion)}</p>
                        <p>Average: {avgLion}</p>
                    </div>
                    <div class='stat-box'>
                        <h3>Algae Stats</h3>
                        <p>Max: {maxAlgae} | Min: {(minAlgae == int.MaxValue ? 0 : minAlgae)}</p>
                        <p>Average: {avgAlgae}</p>
                    </div>
                </div>

                <div class='rank-box'>
                    Current Clownfish Hierarchy: {totalQueens} Queens | {totalKings} Kings | {totalSpares} Spares | {totalBabies} Babies
                    <br><br>
                    Total Living Entities (All Species): {currentTotalLiving}
                </div>

                <canvas id='myChart' style='margin-top:30px;'></canvas>
            </div>
            <script>
                var ctx = document.getElementById('myChart').getContext('2d');
                var myChart = new Chart(ctx, {{
                    type: 'line',
                    data: {{
                        labels: [{timeData}],
                        datasets: [
                            {{ label: 'Clownfish', data: [{clownData}], borderColor: 'orange', fill: false, tension: 0.1 }},
                            {{ label: 'Lionfish', data: [{lionData}], borderColor: 'red', fill: false, tension: 0.1 }},
                            {{ label: 'Algae', data: [{algaeData}], borderColor: 'green', fill: false, tension: 0.1 }}
                        ]
                    }},
                    options: {{ responsive: true, scales: {{ y: {{ beginAtZero: true }} }} }}
                }});
            </script>
        </body>
        </html>";

        // Save and Open
        string fileName = "SimulationReport_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".html";
        string filePath = Path.Combine(Application.dataPath, fileName);
        File.WriteAllText(filePath, htmlContent);
        Debug.Log("📄 Full Simulation Document Generated: " + filePath);
        Application.OpenURL("file:///" + filePath.Replace("\\", "/"));
    }
}