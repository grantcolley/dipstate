using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DevelopmentInProgress.DipState
{
    public static class DipStateExtensions
    {
        public static void Reset(this DipState state, bool clearLogs = false)
        {
            state.SubStates.ForEach(s => s.Reset(clearLogs));
            state.Transition = null;
            state.Antecedent = null;

            state.Status = DipStateStatus.Uninitialised;
            state.RunActions(DipStateActionType.Status);

            state.IsDirty = false;

            if (clearLogs)
            {
                state.Log.Clear();
            }
        }

        public static async Task ResetAsync(this DipState state, bool clearLogs = false)
        {
            foreach (var subState in state.SubStates)
            {
                await subState.ResetAsync(clearLogs);
            }

            state.Transition = null;
            state.Antecedent = null;

            state.Status = DipStateStatus.Uninitialised;
            await state.RunActionsAsync(DipStateActionType.Status).ConfigureAwait(false);

            state.IsDirty = false;

            if (clearLogs)
            {
                state.Log.Clear();
            }
        }

        public static void RunActions(this DipState state, DipStateActionType actionType)
        {
            var actions = state.Actions.Where(a => a.ActionType.Equals(actionType)).ToList();
            actions.ForEach(a => a.Action(state));
        }

        public static async Task RunActionsAsync(this DipState state, DipStateActionType actionType)
        {
            var actions = state.Actions.Where(a => a.ActionType.Equals(actionType)).ToList();
            foreach (var action in actions)
            {
                if (action.IsActionAsync)
                {
                    await action.ActionAsync(state).ConfigureAwait(false);
                }
                else
                {
                    action.Action(state);
                }
            }
        }

        public static DipState AddSubState(this DipState state, DipState subState)
        {
            subState.Parent = state;
            state.SubStates.Add(subState);
            return state;
        }

        public static DipState AddTransition(this DipState state, DipState transition)
        {
            state.Transitions.Add(transition);
            return state;
        }

        public static DipState AddAction(this DipState state, DipStateActionType actionType, Action<DipState> action)
        {
            state.Actions.Add(new DipStateAction() { ActionType = actionType, Action = action });
            return state;
        }

        public static DipState AddActionAsync(this DipState state, DipStateActionType actionType, Func<DipState, Task> action)
        {
            state.Actions.Add(new DipStateAction() { ActionType = actionType, ActionAsync = action });
            return state;
        }

        public static DipState AddDependant(this DipState state, DipState dependant, bool initialiseDependantWhenComplete = false)
        {
            if (!dependant.Dependencies.Any(d => d.Equals(state)))
            {
                dependant.Dependencies.Add(state);
            }

            state.Dependants.Add(new DipStateDependant()
            {
                Dependant = dependant,
                InitialiseDependantWhenComplete = initialiseDependantWhenComplete
            });

            return state;
        }

        public static DipState AddDependency(this DipState state, DipState dependency, bool initialiseWhenDependencyCompleted = false)
        {
            if (!dependency.Dependants.Any(d => d.Dependant.Equals(state)))
            {
                dependency.Dependants.Add(new DipStateDependant()
                {
                    Dependant = state,
                    InitialiseDependantWhenComplete = initialiseWhenDependencyCompleted
                });
            }

            state.Dependencies.Add(dependency);
            return state;
        }

        public static List<DipState> SubStatesToInitialiseWithParent(this DipState state)
        {
            return state.SubStates.Where(s => s.InitialiseWithParent).ToList();
        }

        public static bool CanRun(this DipState state, DipStateStatus newStatus)
        {
            return !state.Status.Equals(newStatus);
        }

        public static void PreRunSetup(this DipState state, DipStateStatus newStatus, DipState transitionState)
        {
            if (newStatus.Equals(DipStateStatus.Failed))
            {
                state.Status = DipStateStatus.Failed;
            }

            if (transitionState != null
                && (newStatus.Equals(DipStateStatus.Completed)
                    || newStatus.Equals(DipStateStatus.Failed)))
            {
                state.Transition = transitionState;
            }
        }

        public static void SetDefaultTransition(this DipState state)
        {
            if (state.Transition == null
                && state.Transitions.Count.Equals(1))
            {
                state.Transition = state.Transitions.First();
            }
        }

        /// <summary>
        /// Set the parent (aggregate) state to InProgress if any of its sub states are
        /// In Progress or if at least one, but not all, of its sub states are Complete.
        /// </summary>
        /// <param name="state">The state whose parent status will be set to InProgress.</param>
        public static void UpdateParentStatusToInProgress(this DipState state)
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
                aggregate.WriteLogEntry(message);
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

        public static bool HasDependencies(this DipState state)
        {
            var dependencies = state.Dependencies.Where(d => !d.Status.Equals(DipStateStatus.Completed)).ToList();
            if (dependencies.Any())
            {
                var dependencyStates = from d in dependencies select String.Format("{0} - {1}", d.Name, d.Status);
                var dependentStateList = String.Join(",", dependencyStates.ToArray());
                state.WriteLogEntry(String.Format("{0} is dependent on {1}", state.Name, dependentStateList));
                return true;
            }

            return false;
        }

        public static bool CanTransition(this DipState state)
        {
            if (state.Transition != null
                && !state.Transitions.Exists(t => t.Id.Equals(state.Transition.Id)))
            {
                var message =
                    String.Format("{0} cannot transition to {1} as it is not registered in the transition list.",
                        state.Name, state.Transition.Name);
                state.WriteLogEntry(message);
                return false;
            }

            return true;
        }

        public static void WriteLogEntry(this DipState state, string message)
        {
            var logEntry = new LogEntry(message);
            state.Log.Add(logEntry);

#if DEBUG

            Debug.WriteLine(logEntry);

#endif
        }

        public static List<DipState> Flatten(this DipState state)
        {
            var rootState = GetRoot(state);
            return FlattenStates(rootState);
        }

        public static DipState GetRoot(this DipState state)
        {
            if (state.Parent != null)
            {
                return GetRoot(state.Parent);
            }

            return state;
        }

        public static async Task<DipState> FailToTransitionStateAsync(this DipState state, DipState failTransitionState)
        {
            if (state != null)
            {
                if (state.Parent != null
                    &&
                    state.Parent.SubStates.Count(
                        s =>
                            s.Status.Equals(DipStateStatus.Uninitialised) ||
                            s.Status.Equals(DipStateStatus.Failed)).Equals(state.Parent.SubStates.Count))
                {
                    await state.Parent.ResetAsync();
                }

                if (failTransitionState != null)
                {
                    if (state.Id.Equals(failTransitionState.Id))
                    {
                        return state;
                    }

                    var tranitionState = await FailToTransitionStateAsync(state.Antecedent, failTransitionState);
                    await state.ResetAsync();
                    await tranitionState.ResetAsync();
                    return tranitionState;
                }

                await state.ResetAsync();
            }

            return null;
        }

        public static DipState FailToTransitionState(this DipState current, DipState failTransitionState)
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

        private static List<DipState> FlattenStates(DipState state, List<DipState> states = null)
        {
            if (states == null)
            {
                states = new List<DipState>();    
            }

            if (!states.Contains(state))
            {
                states.Add(state);
            }

            if (state.SubStates.Any())
            {
                state.SubStates.ForEach(s => FlattenStates(s, states));
            }

            return states;
        }
    }
}
