﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DevelopmentInProgress.DipState
{
    public class DipStateEngine : IDipStateEngine
    {
        public Task<DipState> RunAsync(DipState state, DipStateStatus newStatus)
        {
            return RunAsync(state, newStatus, null);
        }

        public Task<DipState> RunAsync(DipState state, DipState transitionState)
        {
            return RunAsync(state, DipStateStatus.Completed, transitionState);
        }

        public Task<DipState> RunAsync(DipState state, DipStateStatus newStatus, DipState transitionState)
        {
            var tcs = new TaskCompletionSource<DipState>();
            try
            {
                tcs.SetResult(Run(state, newStatus, transitionState, true));
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return tcs.Task;
        }

        public DipState Run(DipState state, DipStateStatus newStatus)
        {
            return Run(state, newStatus, null);
        }

        public DipState Run(DipState state, DipState transitionState)
        {
            return Run(state, DipStateStatus.Completed, transitionState);
        }

        public DipState Run(DipState state, DipStateStatus newStatus, DipState transitionState)
        {
            return Run(state, newStatus, transitionState, false);
        }

        private DipState Run(DipState state, DipStateStatus newStatus, DipState transitionState, bool isAsync)
        {
            if (state.Status.Equals(newStatus))
            {
                return state;
            }

            var currentState = state;

            if (transitionState != null
                && (newStatus.Equals(DipStateStatus.Completed)
                    || newStatus.Equals(DipStateStatus.Failed)))
            {
                currentState.Transition = transitionState;
            }

            switch (newStatus)
            {
                case DipStateStatus.Completed:
                    return Transition(currentState, isAsync);
                case DipStateStatus.Failed:
                    currentState.Status = DipStateStatus.Failed;
                    return Transition(currentState, isAsync);
                case DipStateStatus.Initialised:
                    return Initialise(currentState, isAsync);
                case DipStateStatus.Uninitialised:
                    currentState.Reset();
                    return currentState;
                default:
                    return ChangeStatus(currentState, newStatus, isAsync);
            }
        }

        private DipState Initialise(DipState state, bool isAsync)
        {
            var dependencies = state.Dependencies.Where(d => !d.Status.Equals(DipStateStatus.Completed)).ToList();
            if (dependencies.Any())
            {
                var dependencyStates = from d in dependencies select String.Format("{0} - {1}", d.Name, d.Status);
                var dependentStateList = String.Join(",", dependencyStates.ToArray());
                WriteLogEntry(state, String.Format("{0} is dependent on {1}", state.Name, dependentStateList));
                return state;
            }

            RunActions(state, DipStateActionType.Entry, isAsync);
            
            state.Status = DipStateStatus.Initialised;

            if (state.Type.Equals(DipStateType.Auto))
            {
                return Transition(state, isAsync);
            }

            if (state.SubStates.Any())
            {
                state.SubStates
                    .Where(s => s.InitialiseWithParent)
                    .ToList()
                    .ForEach(s => Initialise(s, isAsync));
            }

            return state;
        }

        private DipState Transition(DipState state, bool isAsync)
        {
            if (state.Transition != null
                && !state.Transitions.Exists(t => t.Id.Equals(state.Transition.Id)))
            {
                var message =
                    String.Format("{0} cannot transition to {1} as it is not registered in the transition list.",
                        state.Name, state.Transition.Name);
                WriteLogEntry(state, message);
                throw new DipStateException(state, message);
            }

            if (state.Status.Equals(DipStateStatus.Failed))
            {
                var stateFailedTo = FailToTransitionState(state, state.Transition);
                if (stateFailedTo != null)
                {
                    return Initialise(stateFailedTo, isAsync);
                }

                WriteLogEntry(state, String.Format("{0} has failed but is unable to transition", state.Name));
                return state;
            }

            // Run exit actions and set the state's status to complete.
            if (TryCompleteState(state, isAsync))
            {
                var initialiseDependants = state.Dependants.Where(d => d.InitialiseDependantWhenComplete).ToList();
                initialiseDependants.ForEach(d => Initialise(d.Dependant, isAsync));

                // If we have a transition state then initialise and return it.
                if (state.Transition != null)
                {
                    state.Transition.Antecedent = state;
                    return Initialise(state.Transition, isAsync);
                }

                // If we don't have a transition state then assume we are just completing 
                // the state so check if the parents needs to be completed too.               
                if (state.Parent != null)
                {
                    // If all the parents sub states are complete then transition the parent.
                    if (state.Parent.SubStates.Count(s => s.Status.Equals(DipStateStatus.Completed))
                        .Equals(state.Parent.SubStates.Count()))
                    {
                        state.Parent.Transition = state.Parent.Transitions.FirstOrDefault();
                        return Transition(state.Parent, isAsync);
                    }

                    // If this state can complete its parent and all the parents substates 
                    // having CanCompleteParent == true, then transition the parent.
                    if (state.CanCompleteParent
                        && state.Parent.SubStates.Count(s => s.CanCompleteParent)
                            .Equals(
                                state.Parent.SubStates.Count(
                                    s => s.CanCompleteParent && s.Status.Equals(DipStateStatus.Completed))))
                    {
                        state.Parent.Transition = state.Parent.Transitions.FirstOrDefault();
                        return Transition(state.Parent, isAsync);
                    }
                }
            }

            return state;
        }

        private DipState FailToTransitionState(DipState current, DipState failTransitionState)
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
                    
                    var tranitionState = FailToTransitionState(current.Antecedent, failTransitionState);
                    current.Reset();
                    tranitionState.Reset();
                    return tranitionState;
                }

                current.Reset();
            }

            return null;
        }

        private DipState ChangeStatus(DipState state, DipStateStatus newStatus, bool isAsync)
        {
            // If newStatus is Completed then just transition the
            // state with a null transition state to complete it.
            if (newStatus.Equals(DipStateStatus.Completed))
            {
                return Transition(state, isAsync);
            }

            state.Status = newStatus;
            UpdateParentStatusToInProgress(state);
            return state;
        }

        /// <summary>
        /// Set the parent (aggregate) state to InProgress if any of its sub states are
        /// In Progress or if at least one, but not all, of its sub states are Complete.
        /// </summary>
        /// <param name="state">The state whose parent status will be set to InProgress.</param>
        private void UpdateParentStatusToInProgress(DipState state)
        {
            if (state.Parent == null
                || state.Parent.Status.Equals(DipStateStatus.InProgress))
            {
                return;
            }

            var aggregate = state.Parent;
            if (aggregate.Status.Equals(DipStateStatus.Completed))
            {
                var message =
                    String.Format("{0} {1} cannot be set to InProgress because it has already been set to Completed.",
                        aggregate.Id, aggregate.Name);
                WriteLogEntry(aggregate, message);
                throw new DipStateException(aggregate, message);
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

        private bool TryCompleteState(DipState state, bool isAsync)
        {
            if (state.CanComplete())
            {
                RunActions(state, DipStateActionType.Exit, isAsync);
                
                state.Status = DipStateStatus.Completed;

                if (state.Transition == null
                    && state.Transitions.Count.Equals(1))
                {
                    state.Transition = state.Transitions.First();
                }

                UpdateParentStatusToInProgress(state);
                return true;
            }

            if (state.Log.Count.Equals(0))
            {
                var message = String.Format("{0} is unable to complete", state.Name);
                WriteLogEntry(state, message);
            }

            throw new DipStateException(state, state.Log.Last().Message);
        }

        private void RunActions(DipState state, DipStateActionType actionType, bool isAsync)
        {
            var actions = state.Actions.Where(a => a.ActionType.Equals(actionType)).ToList();
            actions.ForEach(a =>
            {
                if (isAsync && a.IsActionAsync)
                {
                    RunAsyncAction(a.ActionAsync, state);
                }
                else
                {
                    a.Action(state);
                }
            });
        }

        private async void RunAsyncAction(Func<DipState, Task> asyncAction, DipState state)
        {
            await asyncAction(state);
        }

        private void WriteLogEntry(DipState state, string message)
        {
            var logEntry = new LogEntry(message);
            state.Log.Add(logEntry);

            #if DEBUG
            
            Debug.WriteLine(logEntry);

            #endif
        }
    }
}
