using UnityEngine;
using TMPro;

public class SwimmerManager : MonoBehaviour
{
    public static SwimmerManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI drownedText;
    public TextMeshProUGUI savedText;

    public int DaySaved { get; private set; } = 0;
    public int DayDrowned { get; private set; } = 0;
    public int TotalSaved { get; private set; } = 0;
    public int TotalDrowned { get; private set; } = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ReportDrowned()
    {
        DayDrowned++;
        TotalDrowned++;
        UpdateUI();
    }

    public void ReportSaved()
    {
        DaySaved++;
        TotalSaved++;
        UpdateUI();
    }

    public void ResetDayStats()
    {
        DaySaved = 0;
        DayDrowned = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (drownedText != null)
            drownedText.text = $"Drowned: {DayDrowned}";
        if (savedText != null)
            savedText.text = $"Saved: {DaySaved}";
    }

    public void ResetTotalStats()
    {
        TotalSaved = 0;
        TotalDrowned = 0;
        ResetDayStats();
    }
}