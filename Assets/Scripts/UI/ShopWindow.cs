using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ComfyJam.UI
{
    /// <summary>
    /// Open/close chrome for the shop panel. Drives a CanvasGroup's alpha and a fade +
    /// scale-pop on unscaled time so it animates even while the game is paused. The panel
    /// is never SetActive(false) during the tween, that would kill the running coroutine,
    /// we toggle alpha + blocksRaycasts instead. ShopView handles the panel's contents.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ShopWindow : MonoBehaviour
    {
        [Header("Tween targets")]
        [Tooltip("The panel RectTransform that scales during the pop. Usually this object.")]
        [SerializeField] private RectTransform _panel;

        [Header("Animation")]
        [Min(0f)]
        [SerializeField] private float _tweenDuration = 0.25f;
        [Tooltip("Scale the panel shrinks to while closed; it pops back to 1 on open.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float _closedScale = 0.85f;

        [Header("Behaviour")]
        [Tooltip("Freeze gameplay (Time.timeScale = 0) while the shop is open.")]
        [SerializeField] private bool _pauseWhileOpen = true;
        [Tooltip("Close the shop when the player presses Escape.")]
        [SerializeField] private bool _closeOnEscape = true;

        [Header("Optional auto-wired buttons")]
        [Tooltip("Full-screen scrim behind the panel; clicking it closes the shop.")]
        [SerializeField] private Button _scrimButton;
        [Tooltip("The header X / footer Close button.")]
        [SerializeField] private Button _closeButton;

        private CanvasGroup _canvasGroup;
        private Coroutine _animRoutine;

        /// <summary>True while the shop is open (including during the open/close tween).</summary>
        public bool IsOpen { get; private set; }

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_panel == null)
            {
                _panel = (RectTransform)transform;
            }

            if (_scrimButton != null) _scrimButton.onClick.AddListener(CloseShop);
            if (_closeButton != null) _closeButton.onClick.AddListener(CloseShop);
        }

        private void Start()
        {
            // Start hidden without a tween so the first frame is clean.
            SetInstant(false);
        }

        private void Update()
        {
            if (_closeOnEscape && IsOpen && Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseShop();
            }
        }

        // --- Public API: wire these to buttons or call from gameplay. ---
        public void OpenShop() => Toggle(true);
        public void CloseShop() => Toggle(false);
        public void ToggleShop() => Toggle(!IsOpen);

        private void Toggle(bool open)
        {
            if (IsOpen == open)
            {
                return;
            }

            IsOpen = open;
            _canvasGroup.blocksRaycasts = open;
            _canvasGroup.interactable = open;

            if (_animRoutine != null)
            {
                StopCoroutine(_animRoutine);
            }

            _animRoutine = StartCoroutine(Animate(open));

            if (_pauseWhileOpen)
            {
                Time.timeScale = open ? 0f : 1f;
            }
        }

        private IEnumerator Animate(bool open)
        {
            var fromAlpha = _canvasGroup.alpha;
            var toAlpha = open ? 1f : 0f;
            var fromScale = _panel.localScale;
            var toScale = open ? Vector3.one : Vector3.one * _closedScale;

            var t = 0f;
            while (t < _tweenDuration)
            {
                // Unscaled so the pop still plays while gameplay is paused.
                t += Time.unscaledDeltaTime;
                var k = Mathf.SmoothStep(0f, 1f, t / _tweenDuration);
                _canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, k);
                _panel.localScale = Vector3.Lerp(fromScale, toScale, k);
                yield return null;
            }

            _canvasGroup.alpha = toAlpha;
            _panel.localScale = toScale;
            _animRoutine = null;
        }

        private void SetInstant(bool open)
        {
            IsOpen = open;
            _canvasGroup.alpha = open ? 1f : 0f;
            _canvasGroup.blocksRaycasts = open;
            _canvasGroup.interactable = open;
            _panel.localScale = open ? Vector3.one : Vector3.one * _closedScale;
        }
    }
}
