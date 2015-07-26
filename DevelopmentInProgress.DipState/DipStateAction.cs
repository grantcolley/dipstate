using System;
using System.Threading.Tasks;

namespace DevelopmentInProgress.DipState
{
    /// <summary>
    /// DipStateAction represents an action that is performed at a point in the states lifecyle e.g. on entry, when status changes, or on exit.
    /// Both asynchronous and synchronous actions are supported, however, the are mutually exclusive to a DipStateAction object 
    /// i.e. an instance of DipStateAction can either be executed synchronously or asynchronously but not both.
    /// </summary>
    public class DipStateAction
    {
        /// <summary>
        /// Gets or sets the type of action which determines when the action is executed. 
        /// </summary>
        public DipStateActionType ActionType { get; set; }

        /// <summary>
        /// Gets or sets a action to be executed synchronously.
        /// </summary>
        public Action<DipState> Action { get; set; }

        /// <summary>
        /// Gets or sets a action to be executed asynchronously.
        /// </summary>
        public Func<DipState, Task> ActionAsync { get; set; }

        /// <summary>
        /// Gets a flag indicating whether the instance of DipStateAction is asynchronous or synchronous. 
        /// </summary>
        public bool IsActionAsync
        {
            get { return (ActionAsync != null); }
        }
    }
}