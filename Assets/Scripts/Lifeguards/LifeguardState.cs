namespace ComfyJam.Lifeguards
{
    /// <summary>
    /// The lifecycle state of a single lifeguard in the roster.
    /// A lifeguard moves Available -> Deployed -> Dead
    /// and can transition to Dead from Deployed.
    /// </summary>
    public enum LifeguardState
    {
        /// <summary>Hired and waiting in the roster, ready to be deployed.</summary>
        Available,

        /// <summary>Currently out in the sea on a rescue.</summary>
        Deployed,

        /// <summary>Killed while deployed. Permanent and never returns to the roster.</summary>
        Dead
    }
}