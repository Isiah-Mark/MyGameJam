using System.Collections;
using ComfyJam.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ComfyJam.UI
{
    /// <summary>
    /// Fail screen. Listens for GameEvents.GameOver, fades in a "You failed" overlay on unscaled
    /// time (gameplay is paused by then), then waits for the player to restart, via the Retry
    /// button or by pressing anything, which reloads the scene from the start.
    /// Modeled on ShopWindow's CanvasGroup tween.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class GameOverView : MonoBehaviour
    {
        [Header("Tween targets")]
        [Tooltip("The panel RectTransform that scales during the fade. Usually this object.")]
        [SerializeField] private RectTransform _panel;

        [Header("Animation")]
        [Min(0f)]
        [SerializeField] private float _tweenDuration = 0.35f;
        [Tooltip("Scale the panel starts at while hidden; it pops to 1 as it fades in.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float _hiddenScale = 0.85f;

        [Header("Restart")]
        [Tooltip("Optional Retry button; clicking it reloads the scene.")]
        [SerializeField] private Button _retryButton;
        [Tooltip("Grace period after the overlay appears before 'press anything' restarts, so the death input doesn't instantly skip it.")]
        [Min(0f)]
        [SerializeField] private float _inputDelay = 0.5f;

        private CanvasGroup _canvasGroup;
        private bool _acceptInput;
        private bool _restarting;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_panel == null)
            {
                _panel = (RectTransform)transform;
            }

            if (_retryButton != null) _retryButton.onClick.AddListener(Restart);
        }

        private void Start()
        {
            // Start hidden and inert; subscribe once managers exist.
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            _panel.localScale = Vector3.one * _hiddenScale;

            GameEvents.GameOver += OnGameOver;
        }

        private void OnDestroy()
        {
            GameEvents.GameOver -= OnGameOver;
        }

        private void Update()
        {
            if (_acceptInput && !_restarting && AnyKeyPressed())
            {
                Restart();
            }
        }

        private void OnGameOver()
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            StartCoroutine(ShowOverlay());
        }

        private IEnumerator ShowOverlay()
        {
            var fromScale = _panel.localScale;
            var t = 0f;
            while (t < _tweenDuration)
            {
                // Unscaled: the game is frozen (Time.timeScale = 0) when we fail.
                t += Time.unscaledDeltaTime;
                var k = Mathf.SmoothStep(0f, 1f, t / _tweenDuration);
                _canvasGroup.alpha = k;
                _panel.localScale = Vector3.Lerp(fromScale, Vector3.one, k);
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _panel.localScale = Vector3.one;

            // Brief grace, then any input restarts (the Retry button works the whole time).
            yield return new WaitForSecondsRealtime(_inputDelay);
            _acceptInput = true;
        }

        private void Restart()
        {
            if (_restarting)
            {
                return;
            }

            _restarting = true;
            // Restore time before reloading so the fresh scene runs normally.
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private static bool AnyKeyPressed()
        {
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true;
            if (Mouse.current != null &&
                (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)) return true;
            if (Gamepad.current != null &&
                (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame)) return true;
            return false;
        }
    }
}
