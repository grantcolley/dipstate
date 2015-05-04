using System;
using System.Diagnostics;
using System.Linq;

namespace DevelopmentInProgress.DipState
{
    public class DipStateEngine : IDipStateEngine
    {
        public IDipState Run(IDipState state, DipStateStatus newStatus)
        {
            return Run(state, newStatus, null);
        }

        public IDipState Run(IDipState state, IDipState transitionState)
        {
            return Run(state, DipStateStatus.Completed, transitionState);
        }

        public IDipState Run(IDipState state, DipStateStatus newStatus, IDipState transitionState)
        {
            if (state.Status.Equals(newStatus))
            {
                return state;
            }

            var currentState = (DipState)state;

            if (transitionState != null
                && (newStatus.Equals(DipStateStatus.Completed)
                    || newStatus.Equals(DipStateStatus.Failed)))
            {
                currentState.Transition = transitionState;
            }

            switch (newStatus)
            {
                case DipStateStatus.Completed:
                    return Transition(currentState);
                case DipStateStatus.Failed:
                    currentState.Status = DipStateStatus.Failed;
                    return Transition(currentState);
                case DipStateStatus.Initialised:
                    return Initialise(currentState);
                case DipStateStatus.Uninitialised:
                    currentState.Reset();
                    return currentState;
                default:
                    return ChangeStatus(currentState, newStatus);
            }
        }

        private IDipState Initialise(IDipState state)
        {
            var dependencies = state.Dependencies.Where(d => !d.Status.Equals(DipStateStatus.Completed)).ToList();
            if (dependencies.Any())
            {
                var dependencyStates = from d in dependencies select String.Format("{0} - {1}", d.Name, d.Status);
                var dependentStateList = String.Join(",", dependencyStates.ToArray());
                WriteLogEntry(state, String.Format("{0} is dependent on {1}", state.Name, dependentStateList));
                return state;
            }

            RunActions(state, DipStateActionType.Entry);
            
            ((DipState)state).Status = DipStateStatus.Initialised;

            if (state.Type.Equals(DipStateType.Auto))
            {
                return Transition(state);
            }

            if (state.SubStates.Any())
            {
                state.SubStates
                    .Where(s => s.InitialiseWithParent)
                    .ToList()
                    .ForEach(s => Initialise(s));
            }

            return state;
        }

        private IDipState Transition(IDipState state)
        {
            if (state.Transition != null
                && !state.Transitions.Exists(t => t.Id.Equals(state.Transition.Id)))
            {
                throw new DipStateException(
                    String.Format("{0} cannot transition to {1} as it is not registered in the transition list.",
                        state.Name, state.Transition.Name));
            }

            if (state.Status.Equals(DipStateStatus.Failed))
            {
                var stateFailedTo = FailToTransitionState(state, state.Transition);
                if (stateFailedTo != null)
                {
                    return Initialise(stateFailedTo);
                }

                WriteLogEntry(state, String.Format("{0} has failed but is unable to transition", state.Name));
                return state;
            }

            // Run exit actions and set the state's status to complete.
            if (TryCompleteState(state))
            {
                // If we have a transition state then initialise and return it.
                if (state.Transition != null)
                {
                    ((DipState)state.Transition).Antecedent = state;
                    return Initialise(state.Transition);
                }

                // If we don't have a transition state then assume we are just completing the state so check
                // if all the parents sub states are complete, and if they are then transition the parent.
                if (state.Parent != null)
                {
                    if (state.Parent.SubStates.Count(s => s.Status.Equals(DipStateStatus.Completed))
                        .Equals(state.Parent.SubStates.Count()))
                    {
                        ((DipState) state.Parent).Transition = state.Parent.Transitions.FirstOrDefault();
                        return Transition(state.Parent);
                    }
                }
            }

            return state;
        }

        private IDipState FailToTransitionState(IDipState current, IDipState failTransitionState)
        {
            if (current != null)
            {
                if (current.Parent != null
                    &&
                    current.Parent.SubStates.Count(
                        s =>
                            s.Status.Equals(DipStateStatus.Uninitialised) ||
                            s.Status.Equals(DipStateStatus.Failed)).Equals(current.Parent.SubStates.Count))
                {
                    current.Parent.Reset();
                }

                if (failTransitionState != null)
                {
                    if (current.Id.Equals(failTransitionState.Id))
                    {
                        return current;
                    }
                    
                    var tranitioState = FailToTransitionState(current.Antecedent, failTransitionState);
                    current.Reset();
                    tranitioState.Reset();
                    return tranitioState;
                }

                if (current.Antecedent != null)
                {
                    var antecedent = current.Antecedent;                    
                    current.Reset();
                    antecedent.Reset();
                    return antecedent;
                }

                current.Reset();
            }

            return null;
        }

        private IDipState ChangeStatus(IDipState state, DipStateStatus newStatus)
        {
            // If newStatus is Completed then just transition the
            // state with a null transition state to complete it.
            if (newStatus.Equals(DipStateStatus.Completed))
            {
                return Transition(state);
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

            var aggregate = (DipState)state.Parent;
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
                aggregate.Status = DipStateStatus.InProgress;
                UpdateParentStatusToInProgress(aggregate);
            }
        }
        
        private bool TryCompleteState(IDipState state)
        {
            if (state.CanComplete())
            {
                RunActions(state, DipStateActionType.Exit);
                ((DipState)state).Status = DipStateStatus.Completed;
                UpdateParentStatusToInProgress(state);
                return true;
            }

            var message = String.Format("{0} is unable to complete", state.Name);
            WriteLogEntry(state, message);
            throw new DipStateException(message);
        }

        private void RunActions(IDipState state, DipStateActionType actionType)
        {
            var actions = state.Actions.Where(a => a.ActionType.Equals(actionType)).ToList();
            actions.ForEach(a => a.Action(state));
        }

        private void WriteLogEntry(IDipState state, string message)
        {
            var logEntry = new LogEntry(message);
            state.Log.Add(logEntry);

            #if DEBUG
            
            Debug.WriteLine(logEntry);

            #endif
        }
    }
}
