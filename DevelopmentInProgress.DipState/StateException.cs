using System;
using System.Collections.Generic;
using System.Linq;

namespace DevelopmentInProgress.DipState
{
    /// <summary>
    /// A class representing a <see cref="State"/> exception.
    /// </summary>
    public class StateException : Exception
    {
        /// <summary>
        /// Intilialises a new instance of the <see cref="StateException"/> class.
        /// </summary>
        /// <param name="state">The state to which the exception applies.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public StateException(State state, string message = "", Exception innerException = null)
            : base(message, innerException)
        {
            State = state;
        }

        /// <summary>
        /// Gets the state to which the exception applies.
        /// </summary>
        public State State { get; private set; }
    }
}
