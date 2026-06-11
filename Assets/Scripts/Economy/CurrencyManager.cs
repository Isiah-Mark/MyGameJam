using System;
using ComfyJam.Core;
using UnityEngine;

namespace ComfyJam.Economy
{
    /// <summary>
    /// Singleton wallet. Tracks the player's currency, spends it for hires, and earns it
    /// when gameplay reports a saved person. Access from anywhere via CurrencyManager.Instance.
    /// </summary>
    public class CurrencyManager : MonoBehaviour, ICurrencyWallet
    {
        /// <summary>The one wallet in the scene.</summary>
        public static CurrencyManager Instance { get; private set; }

        [Tooltip("Currency the player starts each run with.")]
        [Min(0)]
        [SerializeField] private int _startingBalance = 100;

        /// <inheritdoc />
        public int Balance { get; private set; }

        /// <inheritdoc />
        public event Action<int> CurrencyChanged;

        private void Awake()
        {
            // Standard singleton guard: keep the first, destroy any duplicates.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Balance = _startingBalance;
        }

        private void OnEnable()
        {
            GameEvents.PersonSaved += OnPersonSaved;
        }

        private void OnDisable()
        {
            GameEvents.PersonSaved -= OnPersonSaved;
        }

        /// <inheritdoc />
        public bool TrySpend(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[CurrencyManager] Ignoring TrySpend of negative amount {amount}.");
                return false;
            }

            if (amount > Balance)
            {
                return false;
            }

            Balance -= amount;
            CurrencyChanged?.Invoke(Balance);
            return true;
        }

        /// <inheritdoc />
        public void Add(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[CurrencyManager] Ignoring Add of negative amount {amount}.");
                return;
            }

            if (amount == 0)
            {
                return;
            }

            Balance += amount;
            CurrencyChanged?.Invoke(Balance);
        }

        private void OnPersonSaved(int reward) => Add(reward);
    }
}