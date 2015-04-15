using System;

namespace DevelopmentInProgress.DipState
{
    public class DipStateException : Exception
    {
        public DipStateException(string message = "", Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
