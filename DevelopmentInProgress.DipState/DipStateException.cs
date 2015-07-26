using System;
using System.Collections.Generic;
using System.Linq;

namespace DevelopmentInProgress.DipState
{
    /// <summary>
    /// A class representing a <see cref="DipState"/> exception.
    /// </summary>
    public class DipStateException : Exception
    {
        /// <summary>
        /// Intilialises a new instance of the <see cref="DipStateException"/> class.
        /// </summary>
        /// <param name="state">The state to which the exception applies.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DipStateException(DipState state, string message = "", Exception innerException = null)
            : base(message, innerException)
        {
            State = state;
        }

        /// <summary>
        /// Gets the state to which the exception applies.
        /// </summary>
        public DipState State { get; private set; }

        /// <summary>
        /// Gets the log belonging to the state to which the exception applies.
        /// </summary>
        public List<string> Messages
        {
            get
            {
                if (State != null)
                {
                    return (from l in State.Log select l.Message).ToList();
                }

                return new List<string>();
            }
        }
    }
}
