﻿namespace DevelopmentInProgress.DipState
{
    /// <summary>
    /// The type of action which determines when the action gets executed in the lifecycle of the state.
    /// </summary>
    public enum DipStateActionType
    {
        /// <summary>
        /// Indicates the action is executed when the state's status changes.
        /// The action is executed after the status has changed.
        /// </summary>
        Status,

        /// <summary>
        /// Indicates the action is executed when the state's is initialised.
        /// The action is executed prior to actual initialisation.
        /// </summary>
        Entry,

        /// <summary>
        /// Indicates the action is executed when the state's is completed.
        /// The action is executed after the status has completed.
        /// </summary>
        Exit,

        /// <summary>
        /// Indicates the action is executed when the state is reset.
        /// </summary>
        Reset
    }
}
