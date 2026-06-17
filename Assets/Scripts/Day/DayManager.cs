using UnityEngine;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Days")]
    public DayConfig[] days;

    [Header("References")]
    public SwimmerSpawner swimmerSpawner;
    public DayUI dayUI;

    private int currentDayIndex = 0;
    private float timer;
    private bool dayActive = false;

    public int CurrentDay => currentDayIndex + 1;
    public float TimeRemaining => timer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        dayUI.ShowDayIntro(CurrentDay, StartDay);
    }

    void Update()
    {
        if (!dayActive) return;

        timer -= Time.deltaTime;
        dayUI.UpdateTimer(Mathf.Max(0f, timer));

        if (timer <= 0f)
        {
            EndDay();
        }
    }

    void StartDay()
    {
        DayConfig config = days[currentDayIndex];
        timer = config.dayDuration;
        swimmerSpawner.Spawn(config.swimmerCount);
        SwimmerManager.Instance.ResetDayStats();
        dayActive = true;
    }

    void EndDay()
    {
        dayActive = false;
        timer = 0f;
        dayUI.UpdateTimer(0f);
        swimmerSpawner.ClearSwimmers();

        if (currentDayIndex >= days.Length - 1)
        {
            dayUI.ShowGameOver(
                SwimmerManager.Instance.TotalSaved,
                SwimmerManager.Instance.TotalDrowned,
                Replay
            );
        }
        else
        {
            dayUI.ShowDayStats(
                CurrentDay,
                SwimmerManager.Instance.DaySaved,
                SwimmerManager.Instance.DayDrowned,
                () =>
                {
                    currentDayIndex++;
                    dayUI.ShowDayIntro(CurrentDay, StartDay);
                }
            );
        }
    }

    void Replay()
    {
        currentDayIndex = 0;
        SwimmerManager.Instance.ResetTotalStats();
        dayUI.ShowDayIntro(CurrentDay, StartDay);
    }
}