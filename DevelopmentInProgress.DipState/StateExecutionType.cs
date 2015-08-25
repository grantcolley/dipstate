namespace DevelopmentInProgress.DipState
{
    /// <summary>
    /// The execution action of the state.
    /// </summary>
    public enum StateExecutionType
    {
        /// <summary>
        /// Resets the state.
        /// </summary>
        Reset = 1,

        /// <summary>
        /// Initialise the state.
        /// </summary>
        Initialise = 2,

        /// <summary>
        /// Sets the state to in progress.
        /// </summary>
        InProgress = 3,

        /// <summary>
        /// Completes the state.
        /// </summary>
        Complete = 4
    }
}