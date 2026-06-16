using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookUIManager : MonoBehaviour
{
    [SerializeField] ObjectiveList allObjectives;
    [SerializeField] ObjectiveEntryUI[] slots;       // drag all 3 slots here
    [SerializeField] TextMeshProUGUI pageLabel;
    [SerializeField] TextMeshProUGUI progressLabel;
    [SerializeField] Button prevButton;
    [SerializeField] Button nextButton;
    [SerializeField] CanvasGroup bookCanvasGroup;

    const int PER_PAGE = 3;
    int currentPage = 0;
    bool isOpen = false;

    void Start()
    {
        LoadProgress();
        ObjectiveEvents.OnAnyObjectiveUpdated += OnObjectiveUpdated;
        RenderPage();
    }

    void OnDestroy()
    {
        // always unsubscribe to avoid errors
        ObjectiveEvents.OnAnyObjectiveUpdated -= OnObjectiveUpdated;
    }

    void OnObjectiveUpdated(string id)
    {
        SaveProgress();
        RenderPage();
    }

    public void ToggleBook()
    {
        isOpen = !isOpen;
        bookCanvasGroup.alpha = isOpen ? 1f : 0f;
        bookCanvasGroup.interactable = isOpen;
        bookCanvasGroup.blocksRaycasts = isOpen;
    }

    void RenderPage()
    {
        var list = allObjectives.objectives;
        int start = currentPage * PER_PAGE;
        int totalPages = Mathf.CeilToInt((float)list.Count / PER_PAGE);

        for (int i = 0; i < slots.Length; i++)
        {
            int idx = start + i;
            if (idx < list.Count)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].Bind(list[idx]);
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }

        pageLabel.text = $"Page {currentPage + 1} of {totalPages}";
        int doneCount = list.Count(o => o.IsComplete);
        progressLabel.text = $"{doneCount} / {list.Count} done";

        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < totalPages - 1;
    }

    public void NextPage() { currentPage++; RenderPage(); }
    public void PrevPage() { currentPage--; RenderPage(); }

    void SaveProgress()
    {
        foreach (var obj in allObjectives.objectives)
            PlayerPrefs.SetInt("obj_" + obj.id, obj.current);
        PlayerPrefs.Save();
    }

    void LoadProgress()
    {
        foreach (var obj in allObjectives.objectives)
            obj.current = PlayerPrefs.GetInt("obj_" + obj.id, 0);
    }
}