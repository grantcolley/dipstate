using System;

namespace DevelopmentInProgress.DipState
{
    /// <summary>
    /// A class representing an entry in a <see cref="State"/>'s log.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="LogEntry"/> class.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public LogEntry(string message)
        {
            Message = message;
            Time = DateTime.Now;
        }

        /// <summary>
        /// Gets the time the message was logged.
        /// </summary>
        public DateTime Time { get; private set; }

        /// <summary>
        /// Gets the message that is logged.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Overrides the ToString() method of the <see cref="LogEntry"/> class.
        /// </summary>
        /// <returns>The message that is logged and the time that it was logged.</returns>
        public override string ToString()
        {
            return String.Format("{0}   {1}", Time.ToString("yyyy-MM-dd HH:mm:ss"), Message);
        }
    }
}
