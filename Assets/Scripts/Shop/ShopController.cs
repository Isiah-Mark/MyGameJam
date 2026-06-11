using System;
using ComfyJam.Economy;
using ComfyJam.Inventory;
using ComfyJam.Lifeguards;
using UnityEngine;

namespace ComfyJam.Shop
{
    /// <summary>
    /// Singleton hiring kiosk. Checks the cap and the balance, spends the cost, and adds
    /// the new lifeguard to the roster. Access from anywhere via ShopController.Instance.
    /// </summary>
    public class ShopController : MonoBehaviour, IShop
    {
        /// <summary>The one shop in the scene.</summary>
        public static ShopController Instance { get; private set; }

        /// <inheritdoc />
        public event Action<string> HireFailed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <inheritdoc />
        public bool TryHire(LifeguardTypeSO type)
        {
            if (type == null)
            {
                Debug.LogWarning("[ShopController] TryHire called with a null type.");
                return false;
            }

            var roster = LifeguardRoster.Instance;
            var wallet = CurrencyManager.Instance;
            if (roster == null || wallet == null)
            {
                Debug.LogError("[ShopController] Missing CurrencyManager or LifeguardRoster in the scene.");
                return false;
            }

            // Check the cap before taking any money.
            if (roster.IsFull)
            {
                HireFailed?.Invoke("Roster is full");
                return false;
            }

            // TrySpend both checks and deducts, so the player is only charged on success.
            if (!wallet.TrySpend(type.HireCost))
            {
                HireFailed?.Invoke("Not enough money");
                return false;
            }

            var hired = roster.Add(type);
            if (hired == null)
            {
                // Should not happen because we checked IsFull, but refund to stay safe.
                wallet.Add(type.HireCost);
                Debug.LogError("[ShopController] Roster rejected an add after payment; refunded.");
                return false;
            }

            return true;
        }
    }
}