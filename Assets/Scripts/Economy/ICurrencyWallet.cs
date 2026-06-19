using System;

namespace ComfyJam.Economy
{
    /// <summary>
    /// Holds the player's currency and the rules for spending and earning it.
    /// </summary>
    public interface ICurrencyWallet
    {
        /// <summary>Current balance. Never negative.</summary>
        int Balance { get; }

        /// <summary>
        /// Tries to spend <paramref name="amount"/>.
        /// Returns false and changes nothing if the balance is too low.
        /// </summary>
        bool TrySpend(int amount);

        /// <summary>Adds currency, for example a reward for saving a person.</summary>
        void Add(int amount);

        /// <summary>Raised after the balance changes. Argument is the new balance.</summary>
        event Action<int> CurrencyChanged;
    }
}