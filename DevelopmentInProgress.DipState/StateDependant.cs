namespace DevelopmentInProgress.DipState
{
    /// <summary>
    /// Represents a state that is dependant on the current state.
    /// </summary>
    public class StateDependant
    {
        /// <summary>
        /// Gets or sets a flag that determines whether the dependant
        /// state is initialised when the current state has completed.
        /// </summary>
        public bool InitialiseDependantWhenComplete { get; set; }

        /// <summary>
        /// Gets or sets the dependant state.
        /// </summary>
        public State Dependant { get; set; }
    }
}
