using System;
using ComfyJam.Lifeguards;

namespace ComfyJam.Core
{
    /// <summary>
    /// The single boundary between the shop/inventory systems and the gameplay/AI systems.
    /// Each event notes who raises it and who listens. Always unsubscribe in OnDisable,
    /// these are static so handlers would otherwise leak between play sessions.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>Gameplay raises when a person is rescued; Economy listens. Argument is the reward.</summary>
        public static event Action<int> PersonSaved;

        /// <summary>Gameplay raises when a deployed lifeguard is killed; Roster listens.</summary>
        public static event Action<LifeguardInstance> LifeguardDied;

        /// <summary>Gameplay raises when a deployed lifeguard finishes a rescue and returns; Roster listens.</summary>
        public static event Action<LifeguardInstance> LifeguardReturned;

        /// <summary>Hot wheel raises to ask gameplay to spawn a deployed lifeguard; Gameplay listens.</summary>
        public static event Action<LifeguardInstance> DeployRequested;

        public static void RaisePersonSaved(int reward) => PersonSaved?.Invoke(reward);

        public static void RaiseLifeguardDied(LifeguardInstance lifeguard) => LifeguardDied?.Invoke(lifeguard);

        public static void RaiseLifeguardReturned(LifeguardInstance lifeguard) => LifeguardReturned?.Invoke(lifeguard);

        public static void RaiseDeployRequested(LifeguardInstance lifeguard) => DeployRequested?.Invoke(lifeguard);
    }
}