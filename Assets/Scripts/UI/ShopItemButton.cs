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

        /// <summary>The lifeguard type this button hires.</summary>
        public LifeguardTypeSO Type { get; private set; }

        /// <summary>Fills in the visuals and wires the click to hire the given type.</summary>
        public void Setup(LifeguardTypeSO type, IShop shop)
        {
            Type = type;
            _icon.sprite = type.Icon;
            _nameText.text = type.DisplayName;
            _costText.text = type.HireCost.ToString();
            _button.onClick.AddListener(() => shop.TryHire(type));
        }

        /// <summary>Enables or disables the button, for example when the player cannot afford it.</summary>
        public void SetInteractable(bool value) => _button.interactable = value;
    }
}