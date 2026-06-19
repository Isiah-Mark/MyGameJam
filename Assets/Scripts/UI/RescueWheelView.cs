using System.Collections.Generic;
using ComfyJam.Inventory;
using ComfyJam.Lifeguards;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ComfyJam.UI
{
    /// <summary>
    /// Target-driven deploy wheel. Left-click a drowning swimmer to open the wheel over it, showing
    /// one slice per lifeguard type with its live available count from the roster; left-click a
    /// slice to deploy that type to the swimmer, right-click to cancel. Roster-backed, so adding a
    /// lifeguard type is data only.
    ///
    /// Input is polled directly through the Input System (Mouse.current) and slices are hit-tested
    /// by rectangle rather than going through the EventSystem/Button, so it does not depend on a
    /// working GraphicRaycaster on the world-space canvas.
    /// </summary>
    public class RescueWheelView : MonoBehaviour
    {
        /// <summary>The one rescue wheel in the scene.</summary>
        public static RescueWheelView Instance { get; private set; }

        [Header("Data")]
        [Tooltip("Lifeguard types shown on the wheel, one slice each.")]
        [SerializeField] private List<LifeguardTypeSO> _catalog = new();

        [Header("Scene refs")]
        [Tooltip("Wheel visuals, hidden until the player opens the wheel on a swimmer.")]
        [SerializeField] private GameObject _wheelRoot;
        [SerializeField] private HotWheelSegment _segmentPrefab;
        [SerializeField] private RectTransform _segmentContainer;

        [Header("Layout")]
        [Tooltip("Distance from the wheel center to each slice.")]
        [Min(0f)]
        [SerializeField] private float _radius = 160f;

        [Tooltip("Camera orthographic size at which the wheel shows at its authored size. " +
                 "The wheel counter-scales by zoom so it keeps a constant on-screen size.")]
        [Min(0.01f)]
        [SerializeField] private float _referenceOrthographicSize = 5f;

        private readonly List<HotWheelSegment> _segments = new();
        private Swimmer _target;
        private bool _isOpen;
        private Vector3 _baseScale = Vector3.one;

        private Camera EventCamera => Camera.main;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        // The roster exists by Start (Awake on the managers), so it is safe to read here.
        private void Start()
        {
            if (_wheelRoot != null)
            {
                _baseScale = _wheelRoot.transform.localScale;
            }

            BuildSegments();
            if (LifeguardRoster.Instance != null)
            {
                LifeguardRoster.Instance.RosterChanged += RefreshCounts;
            }

            SetOpen(false);
        }

        private void OnDestroy()
        {
            if (LifeguardRoster.Instance != null)
            {
                LifeguardRoster.Instance.RosterChanged -= RefreshCounts;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (Mouse.current == null)
            {
                return;
            }

            if (_isOpen)
            {
                // The target can sink while the wheel is open; cancel if it is gone.
                if (_target == null)
                {
                    Close();
                    return;
                }

                UpdateWheelTransform();

                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    Close();
                    return;
                }

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    var segment = SegmentUnderPointer();
                    if (segment != null)
                    {
                        DeploySegment(segment);
                    }
                    else
                    {
                        Close();
                    }
                }

                return;
            }

            // Closed: left-click a drowning swimmer to open the wheel on it.
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                var swimmer = DrowningSwimmerUnderPointer();
                if (swimmer != null)
                {
                    Open(swimmer);
                }
            }
        }

        /// <summary>Opens the wheel over <paramref name="target"/> to choose a lifeguard for it.</summary>
        public void Open(Swimmer target)
        {
            if (target == null)
            {
                return;
            }

            _target = target;
            UpdateWheelTransform();
            RefreshCounts();
            SetOpen(true);
        }

        // Keeps the wheel on its target and counter-scales by zoom so the wheel holds a constant
        // on-screen size: a world-space object's apparent size goes as worldScale / orthographicSize.
        private void UpdateWheelTransform()
        {
            if (_wheelRoot == null || _target == null)
            {
                return;
            }

            _wheelRoot.transform.position = _target.transform.position;

            var cam = EventCamera;
            if (cam != null && _referenceOrthographicSize > 0f)
            {
                _wheelRoot.transform.localScale = _baseScale * (cam.orthographicSize / _referenceOrthographicSize);
            }
        }

        // The drowning swimmer under the mouse, or null. Uses the same 2D point test the old click
        // path used so a swimmer's collider is what gets picked.
        private Swimmer DrowningSwimmerUnderPointer()
        {
            var cam = EventCamera;
            if (cam == null)
            {
                return null;
            }

            Vector2 world = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var hit = Physics2D.OverlapPoint(world);
            if (hit == null)
            {
                return null;
            }

            var swimmer = hit.GetComponent<Swimmer>();
            if (swimmer != null && swimmer.IsDrowning && !swimmer.IsBeingRescued)
            {
                return swimmer;
            }

            return null;
        }

        // The slice whose rectangle contains the mouse, or null.
        private HotWheelSegment SegmentUnderPointer()
        {
            var pointer = Mouse.current.position.ReadValue();
            foreach (var segment in _segments)
            {
                var rect = (RectTransform)segment.transform;
                if (RectTransformUtility.RectangleContainsScreenPoint(rect, pointer, EventCamera))
                {
                    return segment;
                }
            }

            return null;
        }

        private void BuildSegments()
        {
            for (var i = 0; i < _catalog.Count; i++)
            {
                var type = _catalog[i];
                if (type == null)
                {
                    continue;
                }

                var segment = Instantiate(_segmentPrefab, _segmentContainer);
                segment.Setup(type, DeploySegment);
                PositionSegment((RectTransform)segment.transform, _segments.Count, _catalog.Count);
                _segments.Add(segment);
            }
        }

        // Evenly spaces slices around a circle, first slice at the left, going clockwise.
        // With two types this lays them out left and right.
        private void PositionSegment(RectTransform rect, int index, int total)
        {
            var angle = Mathf.PI * 2f * index / total;
            rect.anchoredPosition = new Vector2(-Mathf.Cos(angle), -Mathf.Sin(angle)) * _radius;
        }

        // Deploys the slice's type to the current target. Guarded by _isOpen so the mouse-poll and
        // the (optional) Button click can't both fire it.
        private void DeploySegment(HotWheelSegment segment)
        {
            if (!_isOpen)
            {
                return;
            }

            var roster = LifeguardRoster.Instance;
            if (roster == null)
            {
                Debug.LogWarning("[RescueWheel] No LifeguardRoster in the scene.");
                return;
            }

            if (roster.AvailableCount(segment.Type) <= 0)
            {
                Debug.Log($"[RescueWheel] No available {segment.Type.DisplayName} to deploy.");
                return;
            }

            if (LifeguardDeployer.Instance == null)
            {
                Debug.LogWarning("[RescueWheel] No LifeguardDeployer in the scene; cannot deploy.");
                return;
            }

            LifeguardDeployer.Instance.Deploy(segment.Type, _target);
            Close();
        }

        private void RefreshCounts()
        {
            var roster = LifeguardRoster.Instance;
            if (roster == null)
            {
                return;
            }

            foreach (var segment in _segments)
            {
                segment.SetCount(roster.AvailableCount(segment.Type));
            }
        }

        private void Close()
        {
            _target = null;
            SetOpen(false);
        }

        private void SetOpen(bool open)
        {
            _isOpen = open;
            if (_wheelRoot != null)
            {
                _wheelRoot.SetActive(open);
            }
        }
    }
}
