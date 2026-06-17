using System;
using ComfyJam.Lifeguards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ComfyJam.UI
{
    /// <summary>
    /// One slice of the hot wheel: a lifeguard type's icon, how many are available,
    /// and a highlight shown when it is the current selection. Clicking it deploys it.
    /// </summary>
    public class HotWheelSegment : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _countText;
        [Tooltip("Object shown only while this slice is the current selection.")]
        [SerializeField] private GameObject _highlight;
        [Tooltip("Optional. Lets the player click the slice to deploy it.")]
        [SerializeField] private Button _button;

        /// <summary>The lifeguard type this slice deploys.</summary>
        public LifeguardTypeSO Type { get; private set; }

        /// <summary>Fills in the visuals and wires the click to the wheel's deploy handler.</summary>
        public void Setup(LifeguardTypeSO type, Action<HotWheelSegment> onClicked)
        {
            Type = type;
            _icon.sprite = type.Icon;

            // Optional: the wheel also hit-tests slices by mouse, so a Button is not required.
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (_button != null)
            {
                _button.onClick.AddListener(() => onClicked?.Invoke(this));
            }

            SetSelected(false);
        }

        /// <summary>Shows the available count, dimming the icon when none are ready.</summary>
        public void SetCount(int count)
        {
            _countText.text = count.ToString();

            var color = _icon.color;
            color.a = count > 0 ? 1f : 0.4f;
            _icon.color = color;
        }

        public void SetSelected(bool selected)
        {
            if (_highlight != null)
            {
                _highlight.SetActive(selected);
            }
        }
    }
}
