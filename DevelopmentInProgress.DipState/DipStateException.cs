using System;
using System.Collections.Generic;
using System.Linq;

namespace DevelopmentInProgress.DipState
{
    public class DipStateException : Exception
    {
        public DipStateException(DipState state, string message = "", Exception innerException = null)
            : base(message, innerException)
        {
            State = state;
        }

        public DipState State { get; private set; }

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
