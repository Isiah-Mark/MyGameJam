using UnityEngine;
using TMPro;
using ComfyJam.Core;

public class SwimmerManager : MonoBehaviour
{
    public static SwimmerManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI drownedText;
    public TextMeshProUGUI savedText;

    [Header("Drowned counter colours")]
    [Tooltip("Colour of the drowned counter when the player is safe (no drownings).")]
    public Color safeColor = Color.white;
    [Tooltip("Colour the drowned counter lerps to as the player nears the loss limit.")]
    public Color dangerColor = Color.red;

    public int DrownedCount { get; private set; } = 0;
    public int SavedCount { get; private set; } = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        GameEvents.DrownCountChanged += OnDrownCountChanged;
    }

    void OnDisable()
    {
        GameEvents.DrownCountChanged -= OnDrownCountChanged;
    }

    public void ReportSaved()
    {
        SavedCount++;
        UpdateSavedUI();
    }

    // Driven by GameManager via the GameEvents boundary; reflects only non-evil drownings.
    void OnDrownCountChanged(int count, int limit)
    {
        DrownedCount = count;
        if (drownedText != null)
        {
            drownedText.text = $"Drowned: {count} / {limit}";
            float danger = limit > 0 ? Mathf.Clamp01((float)count / limit) : 0f;
            drownedText.color = Color.Lerp(safeColor, dangerColor, danger);
        }
    }

    void UpdateSavedUI()
    {
        if (savedText != null)
            savedText.text = $"Saved: {SavedCount}";
    }
}
