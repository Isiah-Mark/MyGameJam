using UnityEngine;
using System.Collections.Generic;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("All days in order")]
    public List<ObjectiveList> dayObjectiveLists;

    // the currently active objectives for this day
    private List<ObjectiveData> activeObjectives = new List<ObjectiveData>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // reset all ScriptableObject data on every scene load
        foreach (var dayList in dayObjectiveLists)
        {
            foreach (var obj in dayList.objectives)
            {
                obj.current = 0;
            }
        }
    }

    // called by DayManager when a new day starts
    public void LoadDay(int dayNumber)
    {
        // find the objective list for this day
        ObjectiveList dayList = dayObjectiveLists.Find(d => d.day == dayNumber);

        if (dayList == null)
        {
            Debug.LogWarning("No objectives found for day " + dayNumber);
            return;
        }

        // reset all objectives for the new day
        activeObjectives = dayList.objectives;
        foreach (var obj in activeObjectives)
            obj.current = 0;

        // tell the book UI to refresh with new objectives
        BookUIManager.Instance?.LoadObjectives(activeObjectives);
    }

    // called from SwimmerManager.ReportSaved()
    public void OnSwimmerSaved()
    {
        IncrementByCategory(ObjectiveCategory.Rescue);
    }

    // called from SwimmerManager.ReportDrowned() when isEvil
    public void OnEnemyDefeated()
    {
        IncrementByCategory(ObjectiveCategory.Threat);
    }

    public void OnItemCollected()
    {
        IncrementByCategory(ObjectiveCategory.Collect);
    }

    void IncrementByCategory(ObjectiveCategory category)
    {
        foreach (var obj in activeObjectives)
        {
            if (obj.category == category && !obj.IsComplete)
            {
                obj.Increment();
                break; // only increment the first incomplete one of that type
            }
        }
    }
}