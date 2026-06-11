using System;
using ComfyJam.Lifeguards;

namespace ComfyJam.Shop
{
    /// <summary>
    /// The hiring kiosk. Turns currency into hired lifeguards.
    /// </summary>
    public interface IShop
    {
        /// <summary>
        /// Tries to hire one lifeguard of the given type: checks the cap and the balance,
        /// spends the cost, and adds it to the roster. Returns true on success.
        /// </summary>
        bool TryHire(LifeguardTypeSO type);

        /// <summary>Raised when a hire is rejected. Argument is a reason the UI can show.</summary>
        event Action<string> HireFailed;
    }
}