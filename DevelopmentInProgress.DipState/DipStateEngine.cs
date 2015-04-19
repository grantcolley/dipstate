using System;
using System.Linq;

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
            if (state.Status.Equals(DipStateStatus.Completed))
            {
                throw new DipStateException(String.Format(
                    "Cannot transition state {0} {1} has already been completed.", state.Id, state.Name));
            }

            return Transition(state, transitionState);
        }

        private IDipState InitialiseState(IDipState state)
        {
            RunActions(state, DipStateAction.Entry);

            if (state.Type.Equals(DipStateType.Auto))
            {
                return Transition(state, state.Transition);
            }

            if (state.SubStates.Any())
            {
                state.SubStates.ToList().ForEach(s => InitialiseState(s));
            }

            return state;
        }

        private IDipState Transition(IDipState state, IDipState transitionState)
        {
            if (state.Status.Equals(DipStateStatus.Failed)
                && transitionState == null)
            {
                throw new DipStateException(String.Format("A failed state needs a state that it can transition to."));
            }

            if (TryCompleteState(state))
            {
                if (transitionState != null)
                {
                    return InitialiseState(transitionState);
                }

                if (state.Parent != null)
                {
                    if (state.Parent.SubStates.Count(s => s.Status.Equals(DipStateStatus.Completed))
                        .Equals(state.Parent.SubStates.Count()))
                    {
                        return Transition(state.Parent, state.Parent.Transitions.FirstOrDefault());
                    }
                }
            }

            return state;
        }

        private IDipState ChangeStatus(IDipState state, DipStateStatus newStatus)
        {
            // If newStatus is Completed then just transition the
            // state with a null transition state to complete it.
            if (newStatus.Equals(DipStateStatus.Completed))
            {
                return Transition(state, null);
            }

            ((DipState) state).Status = newStatus;
            UpdateParentStatusToInProgress(state);
            return state;
        }

        /// <summary>
        /// Set the parent (aggregate) state to InProgress if any of its sub states are
        /// In Progress or if at least one, but not all, of its sub states are Complete.
        /// </summary>
        /// <param name="state">The state whose parent status will be set to InProgress.</param>
        private void UpdateParentStatusToInProgress(IDipState state)
        {
            if (state.Parent == null
                || state.Parent.Status.Equals(DipStateStatus.InProgress))
            {
                return;
            }

            var aggregate = state.Parent;
            if (aggregate.Status.Equals(DipStateStatus.Completed))
            {
                throw new DipStateException(
                    String.Format("{0} {1} cannot be set to InProgress because it has already been set to Completed.",
                        aggregate.Id, aggregate.Name));
            }

            if (aggregate.SubStates.Any(s => s.Status.Equals(DipStateStatus.InProgress))
                || (aggregate.SubStates.Any(s => s.Status.Equals(DipStateStatus.Completed))
                    &&
                    !aggregate.SubStates.Count()
                        .Equals(aggregate.SubStates.Count(s => s.Status.Equals(DipStateStatus.Completed)))))
            {
                ((DipState) aggregate).Status = DipStateStatus.InProgress;
                UpdateParentStatusToInProgress(aggregate);
            }
        }
        
        private bool TryCompleteState(IDipState state)
        {
            if (state.CanComplete())
            {
                RunActions(state, DipStateAction.Exit);
                ChangeStatus(state, DipStateStatus.Completed);
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
