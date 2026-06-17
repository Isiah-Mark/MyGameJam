using UnityEngine;

namespace ComfyJam.Core
{
    /// <summary>
    /// Owns the run's loss rule: counts non-evil swimmers who drown and ends the game once the
    /// limit is reached. Listens on the GameEvents boundary and raises GameOver for the UI.
    /// One per scene, accessed via GameManager.Instance.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>The one game-rule owner in the scene.</summary>
        public static GameManager Instance { get; private set; }

        [Tooltip("Non-evil swimmers that may drown before the run is lost.")]
        [Min(1)]
        [SerializeField] private int _drownLimit = 5;

        /// <summary>Non-evil swimmers drowned so far this run.</summary>
        public int DrownCount { get; private set; }

        /// <summary>Drownings allowed before the run ends.</summary>
        public int DrownLimit => _drownLimit;

        private bool _isGameOver;

        private void Awake()
        {
            // Standard singleton guard: keep the first, destroy any duplicates.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // A reloaded scene must never be born frozen if the last run ended paused.
            Time.timeScale = 1f;
        }

        private void OnEnable()
        {
            GameEvents.PersonDrowned += OnPersonDrowned;
        }

        private void OnDisable()
        {
            GameEvents.PersonDrowned -= OnPersonDrowned;
        }

        private void Start()
        {
            // Push the initial tally so the HUD shows "0 / limit" before anyone drowns.
            GameEvents.RaiseDrownCountChanged(DrownCount, _drownLimit);
        }

        private void OnPersonDrowned(bool isEvil)
        {
            // Evil swimmers are free to lose, and nothing counts once the run is over.
            if (isEvil || _isGameOver)
            {
                return;
            }

            DrownCount++;
            GameEvents.RaiseDrownCountChanged(DrownCount, _drownLimit);

            if (DrownCount >= _drownLimit)
            {
                _isGameOver = true;
                GameEvents.RaiseGameOver();
                Time.timeScale = 0f;
            }
        }
    }
}
