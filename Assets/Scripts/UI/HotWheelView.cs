using System.Collections.Generic;
using ComfyJam.Inventory;
using ComfyJam.Lifeguards;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ComfyJam.UI
{
    /// <summary>
    /// Radial deploy menu. Hold Interact to open, Previous/Next to cycle the highlight,
    /// release Interact to deploy the highlighted type. Slices can also be clicked.
    /// One slice per lifeguard type in the catalog, so adding a type is data only.
    /// </summary>
    public class HotWheelView : MonoBehaviour
    {
        [Header("Data")]
        [Tooltip("Lifeguard types shown on the wheel, one slice each.")]
        [SerializeField] private List<LifeguardTypeSO> _catalog = new();

        [Header("Scene refs")]
        [Tooltip("Wheel visuals, hidden until the player opens the wheel.")]
        [SerializeField] private GameObject _wheelRoot;
        [SerializeField] private HotWheelSegment _segmentPrefab;
        [SerializeField] private RectTransform _segmentContainer;

        [Header("Layout")]
        [Tooltip("Pixels from the wheel center to each slice.")]
        [Min(0f)]
        [SerializeField] private float _radius = 160f;

        [Header("Input")]
        [Tooltip("The shared InputSystem_Actions asset. We use Interact, Previous, and Next.")]
        [SerializeField] private InputActionAsset _inputActions;

        private readonly List<HotWheelSegment> _segments = new();
        private InputAction _interact;
        private InputAction _previous;
        private InputAction _next;
        private int _selectedIndex;
        private bool _isOpen;

        private void Awake()
        {
            _interact = _inputActions.FindAction("Interact", throwIfNotFound: true);
            _previous = _inputActions.FindAction("Previous", throwIfNotFound: true);
            _next = _inputActions.FindAction("Next", throwIfNotFound: true);
        }

        // Self-contained input for the jam. If a PlayerInput component later owns this
        // asset, move the Enable/Disable out and just keep the callback wiring.
        private void OnEnable()
        {
            _interact.started += OnInteractStarted;
            _interact.canceled += OnInteractCanceled;
            _previous.performed += OnPrevious;
            _next.performed += OnNext;

            _interact.Enable();
            _previous.Enable();
            _next.Enable();
        }

        private void OnDisable()
        {
            _interact.started -= OnInteractStarted;
            _interact.canceled -= OnInteractCanceled;
            _previous.performed -= OnPrevious;
            _next.performed -= OnNext;
        }

        // Managers exist by Start, so it is safe to read the roster here.
        private void Start()
        {
            BuildSegments();
            LifeguardRoster.Instance.RosterChanged += RefreshCounts;
            SetOpen(false);
        }

        private void OnDestroy()
        {
            if (LifeguardRoster.Instance != null)
            {
                LifeguardRoster.Instance.RosterChanged -= RefreshCounts;
            }
        }

        private void BuildSegments()
        {
            var types = new List<LifeguardTypeSO>();
            foreach (var type in _catalog)
            {
                if (type != null)
                {
                    types.Add(type);
                }
            }

            for (var i = 0; i < types.Count; i++)
            {
                var segment = Instantiate(_segmentPrefab, _segmentContainer);
                segment.Setup(types[i], OnSegmentClicked);
                PositionSegment((RectTransform)segment.transform, i, types.Count);
                _segments.Add(segment);
            }
        }

        // Evenly spaces slices around a circle, first slice at the top, going clockwise.
        private void PositionSegment(RectTransform rect, int index, int total)
        {
            var angle = Mathf.PI * 2f * index / total;
            rect.anchoredPosition = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * _radius;
        }

        private void OnInteractStarted(InputAction.CallbackContext context)
        {
            if (_segments.Count > 0)
            {
                SetOpen(true);
            }
        }

        private void OnInteractCanceled(InputAction.CallbackContext context)
        {
            if (_isOpen)
            {
                DeploySelected();
                SetOpen(false);
            }
        }

        private void OnPrevious(InputAction.CallbackContext context)
        {
            if (_isOpen)
            {
                Cycle(-1);
            }
        }

        private void OnNext(InputAction.CallbackContext context)
        {
            if (_isOpen)
            {
                Cycle(1);
            }
        }

        private void OnSegmentClicked(HotWheelSegment segment)
        {
            if (!_isOpen)
            {
                return;
            }

            SetSelected(_segments.IndexOf(segment));
            DeploySelected();
            SetOpen(false);
        }

        private void Cycle(int direction)
        {
            SetSelected((_selectedIndex + direction + _segments.Count) % _segments.Count);
        }

        private void SetSelected(int index)
        {
            _selectedIndex = index;
            for (var i = 0; i < _segments.Count; i++)
            {
                _segments[i].SetSelected(i == _selectedIndex);
            }
        }

        private void SetOpen(bool open)
        {
            _isOpen = open;
            if (_wheelRoot != null)
            {
                _wheelRoot.SetActive(open);
            }

            if (open)
            {
                RefreshCounts();
                SetSelected(Mathf.Clamp(_selectedIndex, 0, _segments.Count - 1));
            }
        }

        private void RefreshCounts()
        {
            var roster = LifeguardRoster.Instance;
            foreach (var segment in _segments)
            {
                segment.SetCount(roster.AvailableCount(segment.Type));
            }
        }

        private void DeploySelected()
        {
            var type = _segments[_selectedIndex].Type;
            if (!LifeguardRoster.Instance.TryDeploy(type, out _))
            {
                Debug.Log($"[HotWheel] No available {type.DisplayName} to deploy.");
            }
        }
    }
}
