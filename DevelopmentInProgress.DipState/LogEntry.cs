using System;

namespace DevelopmentInProgress.DipState
{
    public class LogEntry
    {
        public LogEntry(string message)
        {
            Message = message;
            Time = DateTime.Now;
        }

        public DateTime Time { get; private set; }
        public string Message { get; private set; }

        public override string ToString()
        {
            return String.Format("{0}   {1}", Time.ToString("yyyy-MM-dd HH:mm:ss"), Message);
        }
    }
}
