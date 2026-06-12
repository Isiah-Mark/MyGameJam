using System.Collections.Generic;
using ComfyJam.Core;
using ComfyJam.Economy;
using ComfyJam.Inventory;
using ComfyJam.Lifeguards;
using ComfyJam.Shop;
using UnityEngine;

namespace ComfyJam.Debugging
{
    /// <summary>
    /// Stand-in for the gameplay/AI side so the shop, roster, and economy can be tested
    /// with no UI and no teammate code. Like the real gameplay it listens for
    /// DeployRequested and tracks who is out, then lets you fire saves, deaths, and
    /// returns from the component's right-click (context) menu, including in play mode.
    /// </summary>
    public class GameplaySimulator : MonoBehaviour
    {
        [Tooltip("Lifeguard type used by the Hire and Deploy test actions.")]
        [SerializeField] private LifeguardTypeSO _testType;

        [Tooltip("Currency granted by the Save Person test action.")]
        [Min(0)]
        [SerializeField] private int _saveReward = 10;

        // Lifeguards currently out, captured from DeployRequested (newest last).
        private readonly List<LifeguardInstance> _deployedOut = new();

        private void OnEnable()
        {
            GameEvents.DeployRequested += OnDeployRequested;
        }

        private void OnDisable()
        {
            GameEvents.DeployRequested -= OnDeployRequested;
        }

        private void OnDeployRequested(LifeguardInstance lifeguard)
        {
            _deployedOut.Add(lifeguard);
            Debug.Log($"[Simulator] Deploy requested for lifeguard #{lifeguard.Id} ({lifeguard.Type.DisplayName}).");
        }

        [ContextMenu("Save Person (earn reward)")]
        private void SavePerson()
        {
            GameEvents.RaisePersonSaved(_saveReward);
            Debug.Log($"[Simulator] Person saved, +{_saveReward}. Balance is now {CurrencyManager.Instance.Balance}.");
        }

        [ContextMenu("Hire One")]
        private void HireOne()
        {
            if (!RequireTestType())
            {
                return;
            }

            var hired = ShopController.Instance.TryHire(_testType);
            Debug.Log(hired ? "[Simulator] Hired one lifeguard." : "[Simulator] Hire failed.");
            LogStatus();
        }

        [ContextMenu("Deploy One")]
        private void DeployOne()
        {
            if (!RequireTestType())
            {
                return;
            }

            if (!LifeguardRoster.Instance.TryDeploy(_testType, out _))
            {
                Debug.Log("[Simulator] Deploy failed, none available.");
            }

            LogStatus();
        }

        [ContextMenu("Kill Last Deployed")]
        private void KillLastDeployed()
        {
            if (TryTakeLastDeployed(out var lifeguard))
            {
                GameEvents.RaiseLifeguardDied(lifeguard);
                Debug.Log($"[Simulator] Lifeguard #{lifeguard.Id} died.");
                LogStatus();
            }
        }

        [ContextMenu("Return Last Deployed")]
        private void ReturnLastDeployed()
        {
            if (TryTakeLastDeployed(out var lifeguard))
            {
                GameEvents.RaiseLifeguardReturned(lifeguard);
                Debug.Log($"[Simulator] Lifeguard #{lifeguard.Id} returned.");
                LogStatus();
            }
        }

        [ContextMenu("Log Status")]
        private void LogStatus()
        {
            var roster = LifeguardRoster.Instance;
            var wallet = CurrencyManager.Instance;
            var available = _testType != null ? roster.AvailableCount(_testType) : 0;
            Debug.Log($"[Simulator] Balance {wallet.Balance} | Available {available} | " +
                      $"Deployed {roster.DeployedCount} | Lost {roster.LostCount} | Usable {roster.TotalUsable}");
        }

        private bool TryTakeLastDeployed(out LifeguardInstance lifeguard)
        {
            if (_deployedOut.Count == 0)
            {
                Debug.Log("[Simulator] No deployed lifeguards to act on.");
                lifeguard = null;
                return false;
            }

            var lastIndex = _deployedOut.Count - 1;
            lifeguard = _deployedOut[lastIndex];
            _deployedOut.RemoveAt(lastIndex);
            return true;
        }

        private bool RequireTestType()
        {
            if (_testType != null)
            {
                return true;
            }

            Debug.LogWarning("[Simulator] Assign a Test Type in the Inspector first.");
            return false;
        }
    }
}