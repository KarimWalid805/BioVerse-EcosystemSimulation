using System.Collections.Generic;
using UnityEngine;

public class AnemoneManager : MonoBehaviour
{
    public List<ClownFish> residents = new List<ClownFish>();
    public float checkInterval = 2f;
    private float timer = 0f;
    public int maxCapacity = 5;
    public GameObject familyEggs;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            UpdateHierarchy();
            timer = 0f;
        }
        
    }

    public void RegisterFish(ClownFish fish)
    {
        if (!residents.Contains(fish))
        {
            residents.Add(fish);
            UpdateHierarchy();
        }
    }

    public int GetResidentCount()
    {
        return residents.Count;
    }
    public void UpdateHierarchy()
    {
        residents.RemoveAll(fish => fish == null);
        if (residents.Count == 0) return;

        
        residents.Sort((a, b) => b.transform.localScale.x.CompareTo(a.transform.localScale.x));

        
        for (int i = 0; i < residents.Count; i++)
        {
            if (i == 0) { residents[i].AssignRank(ClownFish.FishRank.Queen); residents[i].isAdult = true; }
            else if (i == 1) { residents[i].AssignRank(ClownFish.FishRank.King); residents[i].isAdult = true; }
            else
            {
                if (residents[i].transform.localScale.x >= 0.3f) residents[i].AssignRank(ClownFish.FishRank.Spare);
                else residents[i].AssignRank(ClownFish.FishRank.Baby);
            }
        }

        // 3. EVICTION PROTOCOL (With Royal Immunity)
        while (residents.Count > maxCapacity)
        {
            ClownFish fishToEvict = null;
            
            // Loop backwards (starting with the smallest) to find a non-royal
            for (int i = residents.Count - 1; i >= 0; i--)
            {
                if (residents[i].currentRank != ClownFish.FishRank.Queen && residents[i].currentRank != ClownFish.FishRank.King)
                {
                    fishToEvict = residents[i];
                    break; 
                }
            }

            // If we found a spare/baby to kick out, evict them!
            if (fishToEvict != null)
            {
                residents.Remove(fishToEvict);
                fishToEvict.EvictFromAnemone();
            }
            else
            {
                // Failsafe: If everyone is a royal (e.g. capacity is 2), stop the loop to prevent crashes
                break; 
            }
        }
    }
}