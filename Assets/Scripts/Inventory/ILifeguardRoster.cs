using System;
using ComfyJam.Lifeguards;

namespace ComfyJam.Inventory
{
    /// <summary>
    /// The single source of truth for every hired lifeguard and their state:
    /// how many exist, how many are out, and how many have been lost.
    /// </summary>
    public interface ILifeguardRoster
    {
        /// <summary>
        /// Hires a new lifeguard of the given type.
        /// Returns the created instance, or null if a roster cap blocks it.
        /// </summary>
        LifeguardInstance Add(LifeguardTypeSO type);

        /// <summary>
        /// Tries to send out one available lifeguard of the given type.
        /// Returns false if none are available.
        /// </summary>
        bool TryDeploy(LifeguardTypeSO type, out LifeguardInstance deployed);

        /// <summary>Marks a deployed lifeguard as back and available again.</summary>
        void ReportReturn(LifeguardInstance lifeguard);

        /// <summary>Marks a deployed lifeguard as dead. Permanent.</summary>
        void ReportDeath(LifeguardInstance lifeguard);

        /// <summary>How many idle lifeguards of the given type are ready to deploy.</summary>
        int AvailableCount(LifeguardTypeSO type);

        /// <summary>Total lifeguards currently out on a rescue.</summary>
        int DeployedCount { get; }

        /// <summary>Total lifeguards lost to death.</summary>
        int LostCount { get; }

        /// <summary>Total usable lifeguards, available plus deployed, excluding dead.</summary>
        int TotalUsable { get; }

        /// <summary>
        /// True when a roster cap is enabled and reached, so no more can be hired.
        /// Always false when the cap is disabled. Check this before charging the player.
        /// </summary>
        bool IsFull { get; }

        /// <summary>Raised after any roster change so the UI can refresh.</summary>
        event Action RosterChanged;
    }
}