using UnityEngine;

namespace ComfyJam.UI
{
    /// <summary>
    /// Enables UI panels at runtime so they can be left disabled in the editor, keeping the
    /// authoring scene uncluttered. Runs in Awake (before any Start), so each panel's own
    /// Awake/Start hide-and-subscribe logic still fires this frame. Put this on an always-active
    /// object (e.g. the Canvas) and list the panels to wake.
    /// </summary>
    public class UIBootstrap : MonoBehaviour
    {
        [Tooltip("Panels to enable on startup. Leave them disabled in the editor; they manage their own visibility once active.")]
        [SerializeField] private GameObject[] _panels;

        private void Awake()
        {
            foreach (var panel in _panels)
            {
                if (panel != null)
                {
                    panel.SetActive(true);
                }
            }
        }
    }
}
