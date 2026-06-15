using UnityEngine;
using TMPro;

public class SwimmerManager : MonoBehaviour
{
    public static SwimmerManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI drownedText;

    public int DrownedCount { get; private set; } = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ReportDrowned()
    {
        DrownedCount++;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (drownedText != null)
            drownedText.text = $"Drowned: {DrownedCount}";
    }
}