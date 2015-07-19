using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DevelopmentInProgress.DipState.Test
{
    public static class AsyncTestMethods
    {
        public static async Task AsyncEntryAction(DipState state)
        {
            var startLogEntry = new LogEntry(String.Format("Start Entry Async Action - {0}", state.Name));
            state.Log.Add(startLogEntry);

            Debug.WriteLine(startLogEntry.ToString());

            await Task.Run(async delegate
            {
                var random = new Random();
                int milliseconds = random.Next(3) * 1000;
                await Task.Delay(milliseconds);
            });

            var endLogEntry = new LogEntry(String.Format("End Entry Async Action - {0}", state.Name));
            state.Log.Add(endLogEntry);

            Debug.WriteLine(endLogEntry.ToString());
        }

        public static async Task AsyncExitAction(DipState state)
        {
            var startLogEntry = new LogEntry(String.Format("Start Exit Async Action - {0}", state.Name));
            state.Log.Add(startLogEntry);

            Debug.WriteLine(startLogEntry.ToString());

            await Task.Run(async delegate
            {
                var random = new Random();
                int milliseconds = random.Next(3) * 1000;
                await Task.Delay(milliseconds);
            });

            var endLogEntry = new LogEntry(String.Format("End Exit Async Action - {0}", state.Name));
            state.Log.Add(endLogEntry);

            Debug.WriteLine(endLogEntry.ToString());
        }

        public static async Task<bool> AsyncCanCompleteTrue(DipState state)
        {
            var startLogEntry = new LogEntry(String.Format("Start Can Complete Async True - {0}", state.Name));
            state.Log.Add(startLogEntry);

            Debug.WriteLine(startLogEntry.ToString());

            await Task.Run(async delegate
            {
                var random = new Random();
                int milliseconds = random.Next(3) * 1000;
                await Task.Delay(milliseconds);
            });

            var endLogEntry = new LogEntry(String.Format("End Can Complete Async True - {0}", state.Name));
            state.Log.Add(endLogEntry);

            Debug.WriteLine(endLogEntry.ToString());

            return true;
        }

        public static async Task<bool> AsyncCanCompleteFalse(DipState state)
        {
            var startLogEntry = new LogEntry(String.Format("Start Can Complete Async False - {0}", state.Name));
            state.Log.Add(startLogEntry);

            Debug.WriteLine(startLogEntry.ToString());

            await Task.Run(async delegate
            {
                var random = new Random();
                int milliseconds = random.Next(3) * 1000;
                await Task.Delay(milliseconds);
            });

            var endLogEntry = new LogEntry(String.Format("End Can Complete Async False - {0}", state.Name));
            state.Log.Add(endLogEntry);

            Debug.WriteLine(endLogEntry.ToString());

            return false;
        }

        public static async Task AsyncAutoEnrtyActionTransitionToFinalReview(DipState state)
        {
            var startLogEntry = new LogEntry(String.Format("Start Auto Entry Async Transition - {0}", state.Name));
            state.Log.Add(startLogEntry);

            Debug.WriteLine(startLogEntry.ToString());

            await Task.Run(async delegate
            {
                var random = new Random();
                int milliseconds = random.Next(3)*1000;
                await Task.Delay(milliseconds);
            });

            state.Transition = state.Transitions.First(t => t.Name.Equals("Final Review"));

            var transitionLogEntry =
                new LogEntry(String.Format("Set {0} Transition = {1}", state.Name, state.Transition.Name));
            state.Log.Add(transitionLogEntry);

            Debug.WriteLine(transitionLogEntry.ToString());

            var endLogEntry = new LogEntry(String.Format("End Auto Entry Async Action Transition - {0}", state.Name));
            state.Log.Add(endLogEntry);

            Debug.WriteLine(endLogEntry.ToString());
        }

        public static async Task AsyncAutoEnrtyActionTransitionToOverride(DipState state)
        {
            var startLogEntry = new LogEntry(String.Format("Start Auto Entry Async Transition - {0}", state.Name));
            state.Log.Add(startLogEntry);

            Debug.WriteLine(startLogEntry.ToString());

            await Task.Run(async delegate
            {
                var random = new Random();
                int milliseconds = random.Next(3) * 1000;
                await Task.Delay(milliseconds);
            });

            state.Transition = state.Transitions.First(t => t.Name.Equals("Override"));
            
            var transitionLogEntry =
                new LogEntry(String.Format("Set {0} Transition = {1}", state.Name, state.Transition.Name));
            state.Log.Add(transitionLogEntry);

            Debug.WriteLine(transitionLogEntry.ToString());

            var endLogEntry = new LogEntry(String.Format("End Auto Entry Async Action Transition - {0}", state.Name));
            state.Log.Add(endLogEntry);

            Debug.WriteLine(endLogEntry.ToString());
        }

        public static async Task AsyncAutoEnrtyActionTransitionToAdjustments(DipState state)
        {
            var startLogEntry = new LogEntry(String.Format("Start Auto Entry Async Transition - {0}", state.Name));
            state.Log.Add(startLogEntry);

            Debug.WriteLine(startLogEntry.ToString());

            await Task.Run(async delegate
            {
                var random = new Random();
                int milliseconds = random.Next(3) * 1000;
                await Task.Delay(milliseconds);
            });

            state.Transition = state.Transitions.First(t => t.Name.Equals("Adjustments"));

            var transitionLogEntry =
                new LogEntry(String.Format("Set {0} Transition = {1}", state.Name, state.Transition.Name));
            state.Log.Add(transitionLogEntry);

            Debug.WriteLine(transitionLogEntry.ToString());

            var endLogEntry = new LogEntry(String.Format("End Auto Entry Async Action Transition - {0}", state.Name));
            state.Log.Add(endLogEntry);

            Debug.WriteLine(endLogEntry.ToString());
        }

        public static async Task TraceWrite(DipState state)
        {
            await Task.Run(async delegate
            {
                var random = new Random();
                int milliseconds = random.Next(3)*1000;
                await Task.Delay(milliseconds);
            });

            Trace.WriteLine(String.Format("{0} {1} - {2}", state.Id, state.Name, state.Status));
            foreach (var logEntry in state.Log)
            {
                Trace.WriteLine(String.Format("     {0}", logEntry.Message));
            }
        }

        public static async Task AsyncGenericEntryAction(DipState state)
        {
            await Task.Run(async delegate
            {
                var random = new Random();
                int milliseconds = random.Next(3)*1000;
                await Task.Delay(milliseconds);
            });

            var contextClass = state as DipState<ContextText>;
            contextClass.Context.Text = "Entry Action";
        }
    }
}
