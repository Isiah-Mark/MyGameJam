using System;
using System.Collections.Generic;
using ComfyJam.Core;
using ComfyJam.Lifeguards;
using UnityEngine;

namespace ComfyJam.Inventory
{
    /// <summary>
    /// Singleton roster. The single source of truth for every hired lifeguard and their
    /// state. Access from anywhere via LifeguardRoster.Instance. Dead lifeguards stay in
    /// the list (so we can count losses) but never count as usable or available again.
    /// </summary>
    public class LifeguardRoster : MonoBehaviour, ILifeguardRoster
    {
        /// <summary>The one roster in the scene.</summary>
        public static LifeguardRoster Instance { get; private set; }

        [Tooltip("When on, hiring stops once Max Roster usable lifeguards exist.")]
        [SerializeField] private bool _enforceCap = false;

        [Tooltip("Maximum usable lifeguards when the cap is enabled.")]
        [Min(1)]
        [SerializeField] private int _maxRoster = 5;

        private readonly List<LifeguardInstance> _lifeguards = new();
        private int _nextId = 1;

        /// <inheritdoc />
        public event Action RosterChanged;

        /// <inheritdoc />
        public int DeployedCount => CountByState(LifeguardState.Deployed);

        /// <inheritdoc />
        public int LostCount => CountByState(LifeguardState.Dead);

        /// <inheritdoc />
        public int TotalUsable => _lifeguards.Count - LostCount;

        /// <inheritdoc />
        public bool IsFull => _enforceCap && TotalUsable >= _maxRoster;

        /// <inheritdoc />
        public int MaxRoster => _enforceCap ? _maxRoster : int.MaxValue;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _lifeguards.Clear();
            _nextId = 1;
        }

        private void OnEnable()
        {
            GameEvents.LifeguardDied += ReportDeath;
            GameEvents.LifeguardReturned += ReportReturn;
        }

        private void OnDisable()
        {
            GameEvents.LifeguardDied -= ReportDeath;
            GameEvents.LifeguardReturned -= ReportReturn;
        }

        /// <inheritdoc />
        public LifeguardInstance Add(LifeguardTypeSO type)
        {
            if (type == null)
            {
                Debug.LogWarning("[LifeguardRoster] Add called with a null type.");
                return null;
            }

            if (IsFull)
            {
                return null;
            }

            var lifeguard = new LifeguardInstance(_nextId++, type);
            _lifeguards.Add(lifeguard);
            RosterChanged?.Invoke();
            return lifeguard;
        }

        /// <inheritdoc />
        public bool TryDeploy(LifeguardTypeSO type, out LifeguardInstance deployed)
        {
            deployed = _lifeguards.Find(l => l.Type == type && l.State == LifeguardState.Available);
            if (deployed == null)
            {
                return false;
            }

            deployed.State = LifeguardState.Deployed;
            RosterChanged?.Invoke();

            // Ask the gameplay side to spawn the actual swimming lifeguard.
            GameEvents.RaiseDeployRequested(deployed);
            return true;
        }

        /// <inheritdoc />
        public void ReportReturn(LifeguardInstance lifeguard)
        {
            if (!TryValidateDeployed(lifeguard, nameof(ReportReturn)))
            {
                return;
            }

            lifeguard.State = LifeguardState.Available;
            RosterChanged?.Invoke();
        }

        /// <inheritdoc />
        public void ReportDeath(LifeguardInstance lifeguard)
        {
            if (!TryValidateDeployed(lifeguard, nameof(ReportDeath)))
            {
                return;
            }

            lifeguard.State = LifeguardState.Dead;
            RosterChanged?.Invoke();
        }

        /// <inheritdoc />
        public int AvailableCount(LifeguardTypeSO type)
        {
            var count = 0;
            foreach (var lifeguard in _lifeguards)
            {
                if (lifeguard.Type == type && lifeguard.State == LifeguardState.Available)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountByState(LifeguardState state)
        {
            var count = 0;
            foreach (var lifeguard in _lifeguards)
            {
                if (lifeguard.State == state)
                {
                    count++;
                }
            }

            return count;
        }

        // Shared guard for ReportReturn/ReportDeath: the lifeguard must be known and deployed.
        private bool TryValidateDeployed(LifeguardInstance lifeguard, string caller)
        {
            if (lifeguard == null)
            {
                Debug.LogWarning($"[LifeguardRoster] {caller} called with a null lifeguard.");
                return false;
            }

            if (!_lifeguards.Contains(lifeguard))
            {
                Debug.LogWarning($"[LifeguardRoster] {caller} called with a lifeguard not in this roster.");
                return false;
            }

            if (lifeguard.State != LifeguardState.Deployed)
            {
                Debug.LogWarning($"[LifeguardRoster] {caller} expected a Deployed lifeguard but it was {lifeguard.State}.");
                return false;
            }

            return true;
        }
    }
}