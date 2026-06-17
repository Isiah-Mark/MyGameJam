using ComfyJam.Core;
using ComfyJam.Inventory;
using ComfyJam.Lifeguards;
using UnityEngine;

/// <summary>
/// The bridge between the roster/economy systems and the in-scene rescue gameplay. This is the
/// one place that knows about both a <see cref="LifeguardInstance"/> and a <see cref="Swimmer"/>.
/// The rescue wheel calls <see cref="Deploy"/>; the lifeguard reports back through a callback and
/// this raises the boundary events (PersonSaved / LifeguardReturned / LifeguardDied) that the
/// roster and wallet already listen for.
/// </summary>
public class LifeguardDeployer : MonoBehaviour
{
    /// <summary>The one deployer in the scene.</summary>
    public static LifeguardDeployer Instance { get; private set; }

    [Header("Spawning")]
    [Tooltip("Prefab with a Lifeguard component, spawned once per deploy.")]
    [SerializeField] private GameObject _lifeguardPrefab;

    [Tooltip("Where lifeguards spawn from. If empty, they spawn at the shore line below the camera.")]
    [SerializeField] private Transform _spawnAnchor;

    [Tooltip("Multiplier turning a type's Speed stat (1-10) into world move speed.")]
    [Min(0.01f)]
    [SerializeField] private float _speedScale = 1f;

    [Header("Economy")]
    [Tooltip("Currency awarded to the wallet for each swimmer saved.")]
    [Min(0)]
    [SerializeField] private int _saveReward = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Reserves an available lifeguard of <paramref name="type"/> from the roster and sends it out
    /// to rescue <paramref name="target"/>. Returns false (and reserves nothing) if the inputs are
    /// invalid or no lifeguard of that type is available.
    /// </summary>
    public bool Deploy(LifeguardTypeSO type, Swimmer target)
    {
        if (type == null)
        {
            Debug.LogWarning("[LifeguardDeployer] Deploy called with a null type.");
            return false;
        }

        if (target == null || !target.IsDrowning || target.IsBeingRescued)
        {
            Debug.Log("[LifeguardDeployer] Target is not a free drowning swimmer; ignoring deploy.");
            return false;
        }

        if (_lifeguardPrefab == null || LifeguardRoster.Instance == null)
        {
            Debug.LogWarning("[LifeguardDeployer] Missing lifeguard prefab or roster; cannot deploy.");
            return false;
        }

        if (!LifeguardRoster.Instance.TryDeploy(type, out var instance))
        {
            Debug.Log($"[LifeguardDeployer] No available {type.DisplayName} to deploy.");
            return false;
        }

        var lifeguardObject = Instantiate(_lifeguardPrefab, SpawnPosition(target), Quaternion.identity);
        var lifeguard = lifeguardObject.GetComponent<Lifeguard>();
        if (lifeguard == null)
        {
            Debug.LogError("[LifeguardDeployer] Lifeguard prefab has no Lifeguard component; returning instance.");
            Destroy(lifeguardObject);
            GameEvents.RaiseLifeguardReturned(instance);
            return false;
        }

        var speed = type.Speed * _speedScale;
        lifeguard.AssignTarget(target, speed, outcome => OnRescueComplete(outcome, instance, lifeguardObject));
        return true;
    }

    private void OnRescueComplete(RescueOutcome outcome, LifeguardInstance instance, GameObject lifeguardObject)
    {
        switch (outcome)
        {
            case RescueOutcome.Saved:
                GameEvents.RaisePersonSaved(_saveReward);
                GameEvents.RaiseLifeguardReturned(instance);
                break;
            case RescueOutcome.Aborted:
                GameEvents.RaiseLifeguardReturned(instance);
                break;
            case RescueOutcome.LifeguardLost:
                GameEvents.RaiseLifeguardDied(instance);
                break;
        }

        Destroy(lifeguardObject);
    }

    private Vector3 SpawnPosition(Swimmer target)
    {
        if (_spawnAnchor != null)
        {
            return _spawnAnchor.position;
        }

        var cam = Camera.main;
        var shoreY = cam != null ? -cam.orthographicSize - 2f : 0f;
        return new Vector3(target.transform.position.x, shoreY, 0f);
    }
}
