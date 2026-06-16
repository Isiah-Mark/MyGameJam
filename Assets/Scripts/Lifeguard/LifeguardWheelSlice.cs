using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LifeguardWheelSlice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image sliceImage;
    public TextMeshProUGUI nameText;

    private Lifeguard lifeguard;
    private bool isAvailable;
    private Color baseColor;

    public void Setup(Lifeguard lg)
    {
        lifeguard = lg;
        isAvailable = lg.IsFree;
        baseColor = lg.data.color;

        sliceImage.color = isAvailable ? baseColor : Color.grey;
        nameText.text = lg.data.lifeguardName;
        nameText.color = isAvailable ? Color.white : new Color(1, 1, 1, 0.4f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isAvailable) return;
        sliceImage.color = Color.Lerp(baseColor, Color.white, 0.3f);
        transform.localScale = Vector3.one * 1.1f;
        LifeguardWheel.Instance.SetHoveredSlice(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        sliceImage.color = isAvailable ? baseColor : Color.grey;
        transform.localScale = Vector3.one;
        LifeguardWheel.Instance.SetHoveredSlice(null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isAvailable) return;
        LifeguardWheel.Instance.SelectFromSlice(this);
    }

    public void OnUnhover()
    {
        sliceImage.color = isAvailable ? baseColor : Color.grey;
        transform.localScale = Vector3.one;
    }

    public Lifeguard GetLifeguard() => lifeguard;
    public bool IsAvailable() => isAvailable;
}