using UnityEngine;
using TMPro;

public class SwimmerManager : MonoBehaviour
{
    public static SwimmerManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI drownedText;
    public TextMeshProUGUI savedText;

    [Header("Drowned counter colours")]
    [Tooltip("Drowned counter colour when the day is safe (no drownings).")]
    public Color safeColor = Color.white;
    [Tooltip("Colour the drowned counter lerps to as drownings near the day's limit.")]
    public Color dangerColor = Color.red;

    public int DaySaved { get; private set; } = 0;
    public int DayDrowned { get; private set; } = 0;
    public int TotalSaved { get; private set; } = 0;
    public int TotalDrowned { get; private set; } = 0;

    // The current day's drown limit, pushed by DayManager so the HUD can show "X / limit".
    public int DrownLimit { get; private set; } = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ReportDrowned(bool isEvil)
    {
        // Evil swimmers don't count toward the drowned tally or the lose condition.
        if (isEvil)
        {
            ObjectiveManager.Instance?.OnEnemyDefeated(); // evil swimmer = enemy
            return;
        }

        DayDrowned++;
        TotalDrowned++;
        UpdateUI();
    }

    public void ReportSaved()
    {
        DaySaved++;
        TotalSaved++;
        UpdateUI();
        ObjectiveManager.Instance?.OnSwimmerSaved();
    }

    public void ResetDayStats()
    {
        DaySaved = 0;
        DayDrowned = 0;
        UpdateUI();
    }

    public void SetDrownLimit(int limit)
    {
        DrownLimit = limit;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (drownedText != null)
        {
            if (DrownLimit > 0)
            {
                drownedText.text = $"Drowned: {DayDrowned} / {DrownLimit}";
                float danger = Mathf.Clamp01((float)DayDrowned / DrownLimit);
                drownedText.color = Color.Lerp(safeColor, dangerColor, danger);
            }
            else
            {
                drownedText.text = $"Drowned: {DayDrowned}";
            }
        }
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