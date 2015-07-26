namespace DevelopmentInProgress.DipState
{
    /// <summary>
    /// The type of the state.
    /// </summary>
    public enum DipStateType
    {
        /// <summary>
        /// A standard state in a workflow.
        /// </summary>
        Standard = 1,

        /// <summary>
        /// Auto states are automatically transitioned after being initialised.
        /// The entry actions are executed on initialising the state providing
        /// an opportunity to process data and or make a decision regarding which 
        /// state the auto state must transition to. The auto state provides an 
        /// opportunity to make runtime decisions about the path along the workflow 
        /// that needs to be followed etc. 
        /// </summary>
        Auto = 2,

        /// <summary>
        /// The root state is the state the owns the workflow. The root state can have 
        /// one or more sub states, each of which can have their own substates (or mini workflows).
        /// When the root state is initialised it can optionally initialise one or more sub states.
        /// When all substates are complete the root state is completed, completing the workflow.
        /// </summary>
        Root = 3
    }
}