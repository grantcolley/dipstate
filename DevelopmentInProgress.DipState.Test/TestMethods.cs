using System;
using System.Diagnostics;

namespace DevelopmentInProgress.DipState.Test
{
    public static class TestMethods
    {
        public static void TraceWrite(State state)
        {
            Trace.WriteLine(String.Format("{0} {1} - {2}", state.Id, state.Name, state.Status));
            foreach (var logEntry in state.Log)
            {
                Trace.WriteLine(String.Format("     {0}", logEntry.Message));
            }
        }
    }
}
