using ComfyJam.Economy;
using ComfyJam.Inventory;
using TMPro;
using UnityEngine;

namespace ComfyJam.UI
{
    /// <summary>
    /// Shows live roster counts: how many lifeguards are idle, out on a rescue, and lost.
    /// </summary>
    public class RosterView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _availableText;
        [SerializeField] private TMP_Text _deployedText;
        [SerializeField] private TMP_Text _lostText;
        [Tooltip("Optional. Shows the current currency balance, e.g. \"$100\".")]
        [SerializeField] private TMP_Text _balanceText;

        private void Start()
        {
            LifeguardRoster.Instance.RosterChanged += Refresh;
            CurrencyManager.Instance.CurrencyChanged += OnCurrencyChanged;
            Refresh();
            OnCurrencyChanged(CurrencyManager.Instance.Balance);
        }

        private void OnDestroy()
        {
            if (LifeguardRoster.Instance != null)
            {
                LifeguardRoster.Instance.RosterChanged -= Refresh;
            }

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.CurrencyChanged -= OnCurrencyChanged;
            }
        }

        private void OnCurrencyChanged(int balance)
        {
            if (_balanceText != null)
            {
                _balanceText.text = $"${balance}";
            }
        }

        private void Refresh()
        {
            var roster = LifeguardRoster.Instance;
            var available = roster.TotalUsable - roster.DeployedCount;
            _availableText.text = $"Available: {available}";
            _deployedText.text = $"Deployed: {roster.DeployedCount}";
            _lostText.text = $"Lost: {roster.LostCount}";
        }
    }
}