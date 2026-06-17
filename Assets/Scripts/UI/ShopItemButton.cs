using ComfyJam.Lifeguards;
using ComfyJam.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ComfyJam.UI
{
    /// <summary>
    /// One hire button in the shop. Shows a lifeguard type's icon, name, and cost,
    /// and asks the shop to hire that type when clicked.
    /// </summary>
    public class ShopItemButton : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private Button _button;
        [Tooltip("Label inside the Hire button. Shows Hire / Too pricey / Crew full.")]
        [SerializeField] private TMP_Text _hireLabel;
        [Tooltip("Optional. Shows the type's Speed stat, e.g. \"Speed: 7\". Leave empty to keep the card minimal.")]
        [SerializeField] private TMP_Text _speedText;

        /// <summary>The lifeguard type this button hires.</summary>
        public LifeguardTypeSO Type { get; private set; }

        /// <summary>Fills in the visuals and wires the click to hire the given type.</summary>
        public void Setup(LifeguardTypeSO type, IShop shop)
        {
            Type = type;
            _icon.sprite = type.Icon;
            _nameText.text = type.DisplayName;
            _costText.text = type.HireCost.ToString();

            // Optional stat label; leave the field empty to keep the card minimal.
            if (_speedText != null)
            {
                _speedText.text = $"Speed: {type.Speed}";
            }

            _button.onClick.AddListener(() => shop.TryHire(type));
        }

        /// <summary>
        /// Updates the button's enabled state and label for the current wallet/roster.
        /// Crew-full wins over affordability so the player sees the blocking reason.
        /// </summary>
        public void SetAffordance(bool canAfford, bool crewFull)
        {
            _button.interactable = canAfford && !crewFull;
            if (_hireLabel != null)
            {
                _hireLabel.text = crewFull ? "Crew full" : (canAfford ? "Hire" : "Too pricey");
            }
        }
    }
}