namespace ComfyJam.Lifeguards
{
    /// <summary>
    /// A single hired lifeguard at runtime. Identity is the <see cref="Id"/>, so the
    /// gameplay/AI side can hand the same instance back when reporting a return or death.
    /// The roster owns state changes, set <see cref="State"/> through the roster, not directly.
    /// </summary>
    public class LifeguardInstance
    {
        /// <summary>Unique id assigned by the roster when this lifeguard is hired.</summary>
        public int Id { get; }

        /// <summary>Which kind of lifeguard this is.</summary>
        public LifeguardTypeSO Type { get; }

        /// <summary>Current lifecycle state. Managed by the roster.</summary>
        public LifeguardState State { get; set; }

        public LifeguardInstance(int id, LifeguardTypeSO type)
        {
            Id = id;
            Type = type;
            State = LifeguardState.Available;
        }
    }
}