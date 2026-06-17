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

        /// <summary>Gameplay raises when a swimmer finishes sinking; GameManager listens. Argument is whether the swimmer was evil (evil drownings don't count toward the loss).</summary>
        public static event Action<bool> PersonDrowned;

        /// <summary>GameManager raises when the non-evil drowned tally changes; HUD listens. Arguments are (count, limit).</summary>
        public static event Action<int, int> DrownCountChanged;

        /// <summary>GameManager raises when the drown limit is reached; UI listens to show the fail screen.</summary>
        public static event Action GameOver;

        public static void RaisePersonSaved(int reward) => PersonSaved?.Invoke(reward);

        public static void RaiseLifeguardDied(LifeguardInstance lifeguard) => LifeguardDied?.Invoke(lifeguard);

        public static void RaiseLifeguardReturned(LifeguardInstance lifeguard) => LifeguardReturned?.Invoke(lifeguard);

        public static void RaiseDeployRequested(LifeguardInstance lifeguard) => DeployRequested?.Invoke(lifeguard);

        public static void RaisePersonDrowned(bool isEvil) => PersonDrowned?.Invoke(isEvil);

        public static void RaiseDrownCountChanged(int count, int limit) => DrownCountChanged?.Invoke(count, limit);

        public static void RaiseGameOver() => GameOver?.Invoke();
    }
}