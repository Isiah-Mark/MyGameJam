using UnityEngine;

namespace ComfyJam.Lifeguards
{
    /// <summary>
    /// Create assets via Assets > Create > ComfyJam > Lifeguard Type.
    /// Adding a new lifeguard type later is just a new asset, no code changes.
    /// </summary>
    [CreateAssetMenu(fileName = "LifeguardType", menuName = "ComfyJam/Lifeguard Type")]
    public class LifeguardTypeSO : ScriptableObject
    {
        [Tooltip("Identifier used in code and logs, e.g. basic-lifeguard.")]
        [SerializeField] private string _id;

        [Tooltip("Name shown to the player in the shop and hot wheel.")]
        [SerializeField] private string _displayName;

        [Tooltip("Icon shown in the shop button and hot wheel segment.")]
        [SerializeField] private Sprite _icon;

        [Tooltip("Currency cost to hire one of this type.")]
        [Min(0)]
        [SerializeField] private int _hireCost;

        [Header("Stats (1-10)")]
        [Tooltip("How quickly this lifeguard reaches a drowning swimmer.")]
        [Range(1, 10)]
        [SerializeField] private int _speed = 5;

        [Tooltip("How many saves before exhaustion; low stamina raises death risk on hard rescues.")]
        [Range(1, 10)]
        [SerializeField] private int _stamina = 5;

        [Tooltip("Success chance on tough/deep rescues.")]
        [Range(1, 10)]
        [SerializeField] private int _skill = 5;

        /// <summary>Identifier used in code and logs.</summary>
        public string Id => _id;

        /// <summary>Name shown to the player.</summary>
        public string DisplayName => _displayName;

        /// <summary>Icon shown in the shop and hot wheel.</summary>
        public Sprite Icon => _icon;

        /// <summary>Currency cost to hire one of this type.</summary>
        public int HireCost => _hireCost;

        /// <summary>How quickly this lifeguard reaches a drowning swimmer. 1-10.</summary>
        public int Speed => _speed;

        /// <summary>Saves before exhaustion. 1-10.</summary>
        public int Stamina => _stamina;

        /// <summary>Success chance on tough rescues. 1-10.</summary>
        public int Skill => _skill;
    }
}