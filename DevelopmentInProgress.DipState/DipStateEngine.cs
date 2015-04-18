using System;
using System.Linq;
using System.Threading.Tasks;

namespace DevelopmentInProgress.DipState
{
    public class DipStateEngine : IDipStateEngine
    {
        public IDipState Run(IDipState state, IDipState transitionState)
        {
            return Run(state, DipStateStatus.Completed, transitionState);
        }

        public IDipState Run(IDipState state, DipStateStatus newStatus, IDipState transitionState = null)
        {
            throw new NotImplementedException();
        }

        private IDipState InitialiseState(IDipState state)
        {
            RunActions(state, DipStateAction.Entry);

            if (state.Type.Equals(DipStateType.Auto))
            {
                if (state.AutoTransition != null)
                {
                    // return Transition...
                }
                else
                {
                    state.Status = DipStateStatus.Completed;
                    BubbleStatus(state);
                    // return Transition...
                }
            }

            if (state.SubStates.Any())
            {
                state.SubStates.ToList().ForEach(s => InitialiseState(s));
            }

            return state;
        }

        private IDipState Transition(IDipState state, IDipState transitionState)
        {
            
        }

        /// <summary>
        /// If a state is InProgress or Completed set its parent state (the aggregate
        /// state) to InProgress if the parent has not already been set to InProgress. 
        /// </summary>
        /// <param name="state">The state.</param>
        private void BubbleStatus(IDipState state)
        {
            if (state.Parent == null)
            {
                return;
            }

            var aggregate = state.Parent;
            if (aggregate.Status.Equals(DipStateStatus.InProgress))
            {
                return;
            }

            if (aggregate.Status.Equals(DipStateStatus.Completed))
            {
                throw new DipStateException(
                    String.Format("{0} {1} cannot be set to InProgress because it has already been set to Completed.",
                        aggregate.Id, aggregate.Name));
            }

            // Only set the parent (the aggregate state) to InProgress if any of its sub states
            // are In Progress of if at least one, but not all, of its sub states are Complete.
            if (aggregate.SubStates.Any(s => s.Status.Equals(DipStateStatus.InProgress))
                || (aggregate.SubStates.Any(s => s.Status.Equals(DipStateStatus.Completed))
                    &&
                    !aggregate.SubStates.Count()
                        .Equals(aggregate.SubStates.Count(s => s.Status.Equals(DipStateStatus.Completed)))))
            {
                aggregate.Status = DipStateStatus.InProgress;
                BubbleStatus(aggregate);
            }
        }

        private bool TryCompleteState(IDipState state)
        {
            if (state.CanComplete())
            {
                RunActions(state, DipStateAction.Exit);
                state.Status = DipStateStatus.Completed;
                BubbleStatus(state);
                return true;
            }

            throw new DipStateException(String.Format("Cannot complete state {0} {1}", state.Id, state.Name));
        }

        private void RunActions(IDipState state, DipStateAction action)
        {
            var actions = state.Actions.Where(a => a.Action.Equals(action)).ToList();
            actions.ForEach(a => a.Delegate(state));
        }
    }
}
