using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DevelopmentInProgress.DipState.Test
{
    public static class AsyncTestMethods
    {
        public static async Task AsyncAutoEnrtyActionTransitionToFinalReview(State state)
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

        public static async Task AsyncAutoEnrtyActionTransitionToOverride(State state)
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

        public static async Task AsyncAutoEnrtyActionTransitionToAdjustments(State state)
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

        public static async Task AsyncGenericEntryAction(State state)
        {
            await Task.Run(async delegate
            {
                var random = new Random();
                int milliseconds = random.Next(3)*1000;
                await Task.Delay(milliseconds);
            });

            var contextClass = state as State<ContextText>;
            contextClass.Context.Text = "Entry Action";
        }
    }
}
