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

        private void Start()
        {
            LifeguardRoster.Instance.RosterChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (LifeguardRoster.Instance != null)
            {
                LifeguardRoster.Instance.RosterChanged -= Refresh;
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