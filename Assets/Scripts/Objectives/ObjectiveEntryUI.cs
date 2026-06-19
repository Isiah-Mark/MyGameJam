using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveEntryUI : MonoBehaviour
{
    [SerializeField] Image checkboxImage;
    [SerializeField] TextMeshProUGUI labelText;
    [SerializeField] Slider progressBar;
    [SerializeField] Image categoryBadge;

    public Sprite checkedSprite;
    public Sprite uncheckedSprite;

    private ObjectiveData bound;

    public void Bind(ObjectiveData data)
    {
        bound = data;
        labelText.text = data.label;
        checkboxImage.sprite = data.IsComplete ? checkedSprite : uncheckedSprite;
        progressBar.value = (float)data.current / data.goal;
        categoryBadge.color = GetCategoryColor(data.category);

        // strike through text if complete
        labelText.fontStyle = data.IsComplete
            ? TMPro.FontStyles.Strikethrough
            : TMPro.FontStyles.Normal;
    }

    Color GetCategoryColor(ObjectiveCategory cat)
    {
        switch (cat)
        {
            case ObjectiveCategory.Rescue: return new Color(0.21f, 0.54f, 0.87f);
            case ObjectiveCategory.Threat: return new Color(0.89f, 0.30f, 0.29f);
            case ObjectiveCategory.Collect: return new Color(0.39f, 0.60f, 0.13f);
            default: return Color.gray;
        }
    }
}