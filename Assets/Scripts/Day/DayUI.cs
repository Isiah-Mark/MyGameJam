using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DayUI : MonoBehaviour
{
    [Header("Timer")]
    public TextMeshProUGUI timerText;

    [Header("Day Intro")]
    public GameObject introPanel;
    public TextMeshProUGUI introDayText;
    public Button introButton;

    [Header("Day Stats")]
    public GameObject statsPanel;
    public TextMeshProUGUI statsDayText;
    public TextMeshProUGUI statsSavedText;
    public TextMeshProUGUI statsDrownedText;
    public Button statsButton;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverSavedText;
    public TextMeshProUGUI gameOverDrownedText;
    public Button replayButton;

    void Start()
    {
        introPanel.SetActive(false);
        statsPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void UpdateTimer(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void ShowDayIntro(int day, Action onContinue)
    {
        introPanel.SetActive(true);
        introDayText.text = $"Day {day}";
        introButton.onClick.RemoveAllListeners();
        introButton.onClick.AddListener(() =>
        {
            introPanel.SetActive(false);
            onContinue?.Invoke();
        });
    }

    public void ShowDayStats(int day, int saved, int drowned, Action onContinue)
    {
        statsPanel.SetActive(true);
        statsDayText.text = $"Day {day} Complete";
        statsSavedText.text = $"Saved: {saved}";
        statsDrownedText.text = $"Drowned: {drowned}";
        statsButton.onClick.RemoveAllListeners();
        statsButton.onClick.AddListener(() =>
        {
            statsPanel.SetActive(false);
            onContinue?.Invoke();
        });
    }

    public void ShowGameOver(int totalSaved, int totalDrowned, Action onReplay)
    {
        gameOverPanel.SetActive(true);
        gameOverSavedText.text = $"Total Saved: {totalSaved}";
        gameOverDrownedText.text = $"Total Drowned: {totalDrowned}";
        replayButton.onClick.RemoveAllListeners();
        replayButton.onClick.AddListener(() =>
        {
            gameOverPanel.SetActive(false);
            onReplay?.Invoke();
        });
    }
}