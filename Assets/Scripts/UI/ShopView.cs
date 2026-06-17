using System.Collections;
using System.Collections.Generic;
using ComfyJam.Economy;
using ComfyJam.Inventory;
using ComfyJam.Lifeguards;
using ComfyJam.Shop;
using TMPro;
using UnityEngine;

namespace ComfyJam.UI
{
    /// <summary>
    /// The hiring kiosk panel. Builds one hire button per lifeguard type, shows the current
    /// balance, and flashes a short message when a hire is rejected.
    /// </summary>
    public class ShopView : MonoBehaviour
    {
        [Header("Catalog")]
        [Tooltip("Lifeguard types the player can hire here.")]
        [SerializeField] private List<LifeguardTypeSO> _catalog = new();
        [SerializeField] private ShopItemButton _itemButtonPrefab;
        [SerializeField] private Transform _itemContainer;

        [Header("Labels")]
        [SerializeField] private TMP_Text _balanceText;
        [Tooltip("Optional crew footer, e.g. \"Crew  2 / 8\".")]
        [SerializeField] private TMP_Text _crewText;
        [SerializeField] private TMP_Text _feedbackText;
        [Min(0f)]
        [SerializeField] private float _feedbackSeconds = 2f;

        private readonly List<ShopItemButton> _buttons = new();
        private Coroutine _feedbackRoutine;

        // Managers are created in their Awake, which always runs before any Start,
        // so they are guaranteed to exist here. Unsubscribe happens in OnDestroy.
        private void Start()
        {
            BuildButtons();

            CurrencyManager.Instance.CurrencyChanged += OnCurrencyChanged;
            LifeguardRoster.Instance.RosterChanged += RefreshButtons;
            ShopController.Instance.HireFailed += ShowFeedback;

            ClearFeedback();
            OnCurrencyChanged(CurrencyManager.Instance.Balance);
        }

        private void OnDestroy()
        {
            if (CurrencyManager.Instance != null) CurrencyManager.Instance.CurrencyChanged -= OnCurrencyChanged;
            if (LifeguardRoster.Instance != null) LifeguardRoster.Instance.RosterChanged -= RefreshButtons;
            if (ShopController.Instance != null) ShopController.Instance.HireFailed -= ShowFeedback;
        }

        private void BuildButtons()
        {
            foreach (var type in _catalog)
            {
                if (type == null)
                {
                    continue;
                }

                var button = Instantiate(_itemButtonPrefab, _itemContainer);
                button.Setup(type, ShopController.Instance);
                _buttons.Add(button);
            }
        }

        private void OnCurrencyChanged(int balance)
        {
            _balanceText.text = $"${balance}";
            RefreshButtons();
        }

        // Update each button's enabled state and label (too poor, or roster full).
        private void RefreshButtons()
        {
            var balance = CurrencyManager.Instance.Balance;
            var roster = LifeguardRoster.Instance;
            foreach (var button in _buttons)
            {
                button.SetAffordance(balance >= button.Type.HireCost, roster.IsFull);
            }

            RefreshCrew();
        }

        // Shows "Crew  2 / 8" when the cap is on, or just the usable count when it is off.
        private void RefreshCrew()
        {
            if (_crewText == null)
            {
                return;
            }

            var roster = LifeguardRoster.Instance;
            _crewText.text = roster.MaxRoster == int.MaxValue
                ? $"Crew  {roster.TotalUsable}"
                : $"Crew  {roster.TotalUsable} / {roster.MaxRoster}";
        }

        private void ShowFeedback(string message)
        {
            _feedbackText.text = message;
            _feedbackText.gameObject.SetActive(true);

            if (_feedbackRoutine != null)
            {
                StopCoroutine(_feedbackRoutine);
            }

            _feedbackRoutine = StartCoroutine(HideFeedbackAfterDelay());
        }

        private IEnumerator HideFeedbackAfterDelay()
        {
            yield return new WaitForSeconds(_feedbackSeconds);
            ClearFeedback();
        }

        private void ClearFeedback()
        {
            _feedbackText.text = string.Empty;
            _feedbackText.gameObject.SetActive(false);
        }
    }
}