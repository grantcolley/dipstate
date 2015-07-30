namespace DevelopmentInProgress.DipState
{
    /// <summary>
    /// The status of the state.
    /// </summary>
    public enum StateStatus
    {
        /// <summary>
        /// Indicates the state has not been entered.
        /// </summary>
        Uninitialise = 1,

        /// <summary>
        /// Indicates the state has been entered and is active.
        /// </summary>
        Initialise = 2,

        /// <summary>
        /// Indicates the state is active and in progress.
        /// </summary>
        InProgress = 3,

        /// <summary>
        /// Indiactes the state has been successfully completed.
        /// </summary>
        Complete = 4,

        /// <summary>
        /// Indiactes the state has failed.
        /// </summary>
        Fail = 5
    }
}