using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DevelopmentInProgress.DipState
{
    /// <summary>
    /// Extension methods for <see cref="State"/>.
    /// </summary>
    public static class StateExtensions
    {
        /// <summary>
        /// Asynchronously executes the state based on the status provided.
        /// </summary>
        /// <param name="state">The state to execute.</param>
        /// <param name="newStatus">The new status of the state.</param>
        /// <returns>An awaitable task of type <see cref="State"/>.</returns>
        public static async Task<State> ExecuteAsync(this State state, StateStatus newStatus)
        {
            return await state.ExecuteAsync(newStatus, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously transition the state to the transition state provided.
        /// </summary>
        /// <param name="state">The state to transition from.</param>
        /// <param name="transitionState">The state to transition to.</param>
        /// <returns>An awaitable task of type <see cref="State"/>. Typically this is the state that has been transitioned to.</returns>
        public static async Task<State> ExecuteAsync(this State state, State transitionState)
        {
            return await state.ExecuteAsync(StateStatus.Complete, transitionState).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously executes the state based on the status or transition state provided.
        /// </summary>
        /// <param name="state">The state to execute.</param>
        /// <param name="newStatus">The new status of the state.</param>
        /// <param name="transitionState">The state to transition to.</param>
        /// <returns>An awaitable task of type <see cref="State"/> which is the result of the execution.</returns>
        public static async Task<State> ExecuteAsync(this State state, StateStatus newStatus, State transitionState)
        {
            if (!state.CanExecute(newStatus))
            {
                return state;
            }

            state.PreExecuteSetup(newStatus, transitionState);

            switch (newStatus)
            {
                case StateStatus.Complete:
                case StateStatus.Fail:
                    return await state.TransitionAsync().ConfigureAwait(false);
                case StateStatus.Initialise:
                    return await state.InitialiseAsync().ConfigureAwait(false);
                case StateStatus.Uninitialise:
                    await state.ResetAsync().ConfigureAwait(false);
                    return state;
                default:
                    return await state.ChangeStatusAsync(newStatus).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Synchronously executes the state based on the status provided.
        /// </summary>
        /// <param name="state">The state to execute.</param>
        /// <param name="newStatus">The new status of the state.</param>
        /// <returns>A <see cref="State"/>.</returns>
        public static State Execute(this State state, StateStatus newStatus)
        {
            return state.Execute(newStatus, null);
        }

        /// <summary>
        /// Synchronously transition the state to the transition state provided.
        /// </summary>
        /// <param name="state">The state to transition from.</param>
        /// <param name="transitionState">The state to transition to.</param>
        /// <returns>The state that has been transitioned to.</returns>
        public static State Execute(this State state, State transitionState)
        {
            return state.Execute(StateStatus.Complete, transitionState);
        }

        /// <summary>
        /// Synchronously executes the state based on the status or transition state provided.
        /// </summary>
        /// <param name="state">The state to execute.</param>
        /// <param name="newStatus">The new status of the state.</param>
        /// <param name="transitionState">The state to transition to.</param>
        /// <returns>The result of the execution.</returns>
        public static State Execute(this State state, StateStatus newStatus, State transitionState)
        {
            if (!state.CanExecute(newStatus))
            {
                return state;
            }

            state.PreExecuteSetup(newStatus, transitionState);

            switch (newStatus)
            {
                case StateStatus.Complete:
                case StateStatus.Fail:
                    return state.Transition();
                case StateStatus.Initialise:
                    return state.Initialise();
                case StateStatus.Uninitialise:
                    state.Reset();
                    return state;
                default:
                    return state.ChangeStatus(newStatus);
            }
        }

        /// <summary>
        /// Reset the state synchronously to uninitialised. This will also reset any substates but not the parent.
        /// </summary>
        /// <param name="state">The state to reset.</param>
        /// <param name="hardReset">A flag indicating whether the the logs are also cleared and if Dependants and Transitions also get reset.</param>
        public static void Reset(this State state, bool hardReset = false)
        {
            if (state.Status.Equals(StateStatus.Uninitialise))
            {
                return;
            }

            state.SubStates.ForEach(s => s.Reset(hardReset));

            if (hardReset)
            {
                state.Dependants.ForEach(d => d.Dependant.Reset(true));
                if (state.Transition != null)
                {
                    state.Transition.Reset(true);
                }
            }

            state.Transition = null;
            state.Antecedent = null;

            state.Status = StateStatus.Uninitialise;
            state.ExecuteActions(StateActionType.Reset);

            state.IsDirty = false;

            if (hardReset)
            {
                // Clear logs last after Reset actions run.
                state.Log.Clear();
            }
        }

        /// <summary>
        /// Reset the state asynchronously to uninitialised. This will also reset any substates but not the parent.
        /// </summary>
        /// <param name="state">The state to reset.</param>
        /// <param name="hardReset">A flag indicating whether the the logs are also cleared and if Dependants and Transitions also get reset.</param>
        /// <returns>An awaitable task.</returns>
        public static async Task ResetAsync(this State state, bool hardReset = false)
        {
            if (state.Status.Equals(StateStatus.Uninitialise))
            {
                return;
            }

            foreach (var subState in state.SubStates)
            {
                await subState.ResetAsync(hardReset).ConfigureAwait(false);
            }

            if (hardReset)
            {
                foreach (var dependant in state.Dependants)
                {
                    await dependant.Dependant.ResetAsync(true);
                }

                if (state.Transition != null)
                {
                    await state.Transition.ResetAsync(true);
                }
            }

            state.Transition = null;
            state.Antecedent = null;

            state.Status = StateStatus.Uninitialise;
            await state.ExecuteActionsAsync(StateActionType.Reset).ConfigureAwait(false);

            state.IsDirty = false;

            if (hardReset)
            {
                // Clear logs last after Reset actions run.
                state.Log.Clear();
            }
        }

        /// <summary>
        /// Add a substate.
        /// </summary>
        /// <param name="state">The state for which a substate is added.</param>
        /// <param name="subState">The substate to add.</param>
        /// <returns></returns>
        public static State AddSubState(this State state, State subState)
        {
            subState.Parent = state;
            state.SubStates.Add(subState);
            return state;
        }

        /// <summary>
        /// Add a transition state that the state can be transitioned to.
        /// </summary>
        /// <param name="state">The state for which a transition state is added.</param>
        /// <param name="transition">The transition state to add.</param>
        /// <returns>The state for which a transition state is added.</returns>
        public static State AddTransition(this State state, State transition)
        {
            state.Transitions.Add(transition);
            return state;
        }

        /// <summary>
        /// Add an action to be executed synchronously.
        /// </summary>
        /// <param name="state">The state for which an action is added.</param>
        /// <param name="actionType">The type of action.</param>
        /// <param name="action">The action to be executed according to the type of action.</param>
        /// <returns>The state for which an action is added.</returns>
        public static State AddAction(this State state, StateActionType actionType, Action<State> action)
        {
            state.Actions.Add(new StateAction() { ActionType = actionType, Action = action });
            return state;
        }

        /// <summary>
        /// Add an action to be executed asynchronously.
        /// </summary>
        /// <param name="state">The state for which an action is added.</param>
        /// <param name="actionType">The type of action.</param>
        /// <param name="action">The action to be executed according to the type of action.</param>
        /// <returns>The state for which an action is added.</returns>
        public static State AddActionAsync(this State state, StateActionType actionType, Func<State, Task> action)
        {
            state.Actions.Add(new StateAction() { ActionType = actionType, ActionAsync = action });
            return state;
        }

        /// <summary>
        /// Add a dependant state to the state. The dependant state cannot be initialised until the state to which it is added has completed.
        /// </summary>
        /// <param name="state">The state to which the dependant is added.</param>
        /// <param name="dependant">The dependant state.</param>
        /// <param name="initialiseDependantWhenComplete">A flag indicating whether the dependant state is initialised when the state to which it is added is completed.</param>
        /// <returns>The state to which the dependant is added.</returns>
        public static State AddDependant(this State state, State dependant, bool initialiseDependantWhenComplete = false)
        {
            if (!dependant.Dependencies.Any(d => d.Equals(state)))
            {
                dependant.Dependencies.Add(state);
            }

            state.Dependants.Add(new StateDependant()
            {
                Dependant = dependant,
                InitialiseDependantWhenComplete = initialiseDependantWhenComplete
            });

            return state;
        }

        /// <summary>
        /// Add a dependency state to the state. The dependancy state must be completed before the state to which it is added can be initialised.
        /// </summary>
        /// <param name="state">The state to which the dependancy state is added.</param>
        /// <param name="dependency">The dependency state.</param>
        /// <param name="initialiseWhenDependencyCompleted">A flag indicating whether the state will be initialised when the dependency state is completed.</param>
        /// <returns>The state to which the dependancy state is added.</returns>
        public static State AddDependency(this State state, State dependency, bool initialiseWhenDependencyCompleted = false)
        {
            if (!dependency.Dependants.Any(d => d.Dependant.Equals(state)))
            {
                dependency.Dependants.Add(new StateDependant()
                {
                    Dependant = state,
                    InitialiseDependantWhenComplete = initialiseWhenDependencyCompleted
                });
            }

            state.Dependencies.Add(dependency);
            return state;
        }

        /// <summary>
        /// Executes a predicate synchronously to determine whether the state can complete.
        /// </summary>
        /// <returns>Returns true if the state can be completed, else returns false. If no predicate is provided returns true.</returns>
        public static bool CanComplete(this State state)
        {
            return state.CanCompleteState == null || state.CanCompleteState(state);
        }

        /// <summary>
        /// Executes a predicate asynchronously to determine whether the state can complete.
        /// </summary>
        /// <returns>Returns true if the state can be completed, else returns false. If no predicate is provided returns true.</returns>
        public static async Task<bool> CanCompleteAsync(this State state)
        {
            if (state.CanCompleteStateAsync != null)
            {
                return await state.CanCompleteStateAsync(state);
            }

            return true;
        }

        /// <summary>
        /// Add a predicate which is executed synchronously to determine whether the state can be completed i.e. passes validation.
        /// </summary>
        /// <param name="state">The state for which the predicate is added.</param>
        /// <param name="predicate">The predicate to add.</param>
        /// <returns>Returns the state.</returns>
        public static State AddCanCompletePredicate(this State state, Predicate<State> predicate)
        {
            state.CanCompleteState = predicate;
            return state;
        }

        /// <summary>
        /// Add a predicate which is executed asynchronously to determine whether the state can be completed i.e. passes validation.
        /// </summary>
        /// <param name="state">The state for which the predicate is added.</param>
        /// <param name="asyncPredicate">The async predicate to add.</param>
        /// <returns>Returns the state.</returns>
        public static State AddCanCompletePredicateAsync(this State state, Func<State, Task<bool>> asyncPredicate)
        {
            state.CanCompleteStateAsync = asyncPredicate;
            return state;
        }

        /// <summary>
        /// Determines whether the state has any dependency states that have not yet been completed. This will determine whether the state can be initialised or not.
        /// </summary>
        /// <param name="state">The state for which to check if it has any dependencies that are not yet complete.</param>
        /// <returns>True if the state has any dependencies that are not yet complete, else returns false.</returns>
        public static bool HasDependencies(this State state)
        {
            var dependencies = state.Dependencies.Where(d => !d.Status.Equals(StateStatus.Complete)).ToList();
            if (dependencies.Any())
            {
                var dependencyStates = from d in dependencies select String.Format("{0} - {1}", d.Name, d.Status);
                var dependentStateList = String.Join(",", dependencyStates.ToArray());
                state.WriteLogEntry(String.Format("{0} is dependent on {1}", state.Name, dependentStateList));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a flattened list of the workflow graph from the state. It does this by seeking the root state and then
        /// populating a list by traversing the graph from the root.
        /// </summary>
        /// <param name="state">The state for which to return a flattened graph.</param>
        /// <returns>A flattened list of states in the graph.</returns>
        public static List<State> Flatten(this State state)
        {
            var rootState = state.GetRoot();
            return rootState.FlattenStates();
        }

        /// <summary>
        /// Gets the root state from in the graph.
        /// </summary>
        /// <param name="state">The state for which to return the root.</param>
        /// <returns>The root state of the graph.</returns>
        public static State GetRoot(this State state)
        {
            if (state.Parent != null)
            {
                return GetRoot(state.Parent);
            }

            return state;
        }

        /// <summary>
        /// Write a log entry to the state.
        /// </summary>
        /// <param name="state">The state for which to write a log entry.</param>
        /// <param name="message">The message to log.</param>
        public static void WriteLogEntry(this State state, string message)
        {
            var logEntry = new LogEntry(message);
            state.Log.Add(logEntry);

            #if DEBUG

            Debug.WriteLine(logEntry);

            #endif
        }

        private static async Task<State> InitialiseAsync(this State state)
        {
            if (state.HasDependencies())
            {
                return state;
            }

            await state.ExecuteActionsAsync(StateActionType.Entry).ConfigureAwait(false);

            state.Status = StateStatus.Initialise;

            await state.ExecuteActionsAsync(StateActionType.Status).ConfigureAwait(false);

            if (state.Type.Equals(StateType.Auto))
            {
                return await state.TransitionAsync().ConfigureAwait(false);
            }

            var subStates = state.SubStatesToInitialiseWithParent();
            foreach (var subState in subStates)
            {
                await subState.InitialiseAsync().ConfigureAwait(false);
            }

            return state;
        }

        private static State Initialise(this State state)
        {
            if (state.HasDependencies())
            {
                return state;
            }

            state.ExecuteActions(StateActionType.Entry);

            state.Status = StateStatus.Initialise;

            state.ExecuteActions(StateActionType.Status);

            if (state.Type.Equals(StateType.Auto))
            {
                return state.Transition();
            }

            state.SubStatesToInitialiseWithParent().ForEach(s => s.Initialise());

            return state;
        }

        private static async Task<State> TransitionAsync(this State state)
        {
            if (!state.CanTransition())
            {
                throw new StateException(state, String.Format("{0} failed to transition.", state.Name));
            }

            if (state.Status.Equals(StateStatus.Fail))
            {
                if (state.Transition != null)
                {
                    var stateFailedTo = state.Transition;
                    state.Transition = null;
                    await stateFailedTo.ResetAsync(true);
                    return await stateFailedTo.InitialiseAsync().ConfigureAwait(false);
                }

                await state.ResetAsync(true);
                return state;
            }

            // Run exit actions and set the state's status to complete.
            var completeState = await state.TryCompleteStateAsync().ConfigureAwait(false);
            if (!completeState)
            {
                return state;
            }

            var dependants = state.Dependants.Where(d => d.InitialiseDependantWhenComplete).ToList();
            foreach (var dependant in dependants)
            {
                await dependant.Dependant.InitialiseAsync().ConfigureAwait(false);
            }

            // If we have a transition state then initialise and return it.
            if (state.Transition != null)
            {
                state.Transition.Antecedent = state;
                return await state.Transition.InitialiseAsync().ConfigureAwait(false);
            }

            // If we don't have a transition state then assume we are just completing 
            // the state so check if the parents needs to be completed too.               
            if (state.Parent != null)
            {
                // If all the parents sub states are complete then transition the parent.
                if (state.Parent.SubStates.Count(s => s.Status.Equals(StateStatus.Complete))
                    .Equals(state.Parent.SubStates.Count()))
                {
                    state.Parent.Transition = state.Parent.Transitions.FirstOrDefault();
                    return await state.Parent.TransitionAsync().ConfigureAwait(false);
                }

                // If this state can complete its parent and all the parents substates 
                // having CanCompleteParent == true, then transition the parent.
                if (state.CanCompleteParent
                    && state.Parent.SubStates.Count(s => s.CanCompleteParent)
                        .Equals(
                            state.Parent.SubStates.Count(
                                s => s.CanCompleteParent && s.Status.Equals(StateStatus.Complete))))
                {
                    state.Parent.Transition = state.Parent.Transitions.FirstOrDefault();
                    return await state.Parent.TransitionAsync().ConfigureAwait(false);
                }
            }

            return state;
        }

        private static State Transition(this State state)
        {
            if (!state.CanTransition())
            {
                throw new StateException(state, String.Format("{0} failed to transition.", state.Name));
            }

            if (state.Status.Equals(StateStatus.Fail))
            {
                if (state.Transition != null)
                {
                    var stateFailedTo = state.Transition;
                    state.Transition = null;
                    stateFailedTo.Reset(true);
                    return stateFailedTo.Initialise();
                }

                state.Reset(true);
                return state;
            }

            // Run exit actions and set the state's status to complete.
            if (!state.TryCompleteState())
            {
                return state;
            }

            var initialiseDependants = state.Dependants.Where(d => d.InitialiseDependantWhenComplete).ToList();
            initialiseDependants.ForEach(d => d.Dependant.Initialise());

            // If we have a transition state then initialise and return it.
            if (state.Transition != null)
            {
                state.Transition.Antecedent = state;
                return state.Transition.Initialise();
            }

            // If we don't have a transition state then assume we are just completing 
            // the state so check if the parents needs to be completed too.               
            if (state.Parent != null)
            {
                // If all the parents sub states are complete then transition the parent.
                if (state.Parent.SubStates.Count(s => s.Status.Equals(StateStatus.Complete))
                    .Equals(state.Parent.SubStates.Count()))
                {
                    state.Parent.Transition = state.Parent.Transitions.FirstOrDefault();
                    return state.Parent.Transition();
                }

                // If this state can complete its parent and all the parents substates 
                // having CanCompleteParent == true, then transition the parent.
                if (state.CanCompleteParent
                    && state.Parent.SubStates.Count(s => s.CanCompleteParent)
                        .Equals(
                            state.Parent.SubStates.Count(
                                s => s.CanCompleteParent && s.Status.Equals(StateStatus.Complete))))
                {
                    state.Parent.Transition = state.Parent.Transitions.FirstOrDefault();
                    return state.Parent.Transition();
                }
            }

            return state;
        }

        private static async Task<State> ChangeStatusAsync(this State state, StateStatus newStatus)
        {
            // If newStatus is Complete then just transition the
            // state with a null transition state to complete it.
            if (newStatus.Equals(StateStatus.Complete))
            {
                return await state.TransitionAsync().ConfigureAwait(false);
            }

            state.Status = newStatus;

            await state.ExecuteActionsAsync(StateActionType.Status).ConfigureAwait(false);

            state.UpdateParentStatusToInProgress();

            return state;
        }

        private static State ChangeStatus(this State state, StateStatus newStatus)
        {
            // If newStatus is Complete then just transition the
            // state with a null transition state to complete it.
            if (newStatus.Equals(StateStatus.Complete))
            {
                return state.Transition();
            }

            state.Status = newStatus;

            state.ExecuteActions(StateActionType.Status);

            state.UpdateParentStatusToInProgress();

            return state;
        }

        private static async Task<bool> TryCompleteStateAsync(this State state)
        {
            var canComplete = await state.CanCompleteAsync().ConfigureAwait(false);
            if (!canComplete)
            {
                var message = String.Format("{0} is unable to complete", state.Name);

                state.WriteLogEntry(message);

                throw new StateException(state, state.Log.Last().Message);
            }

            await state.ExecuteActionsAsync(StateActionType.Exit).ConfigureAwait(false);

            state.Status = StateStatus.Complete;

            await state.ExecuteActionsAsync(StateActionType.Status).ConfigureAwait(false);

            state.SetDefaultTransition();

            state.UpdateParentStatusToInProgress();

            return true;
        }

        private static bool TryCompleteState(this State state)
        {
            var canComplete = state.CanComplete();
            if (!canComplete)
            {
                var message = String.Format("{0} is unable to complete", state.Name);

                state.WriteLogEntry(message);

                throw new StateException(state, state.Log.Last().Message);
            }

            state.ExecuteActions(StateActionType.Exit);

            state.Status = StateStatus.Complete;

            state.ExecuteActions(StateActionType.Status);

            state.SetDefaultTransition();

            state.UpdateParentStatusToInProgress();

            return true;
        }

        private static void ExecuteActions(this State state, StateActionType actionType)
        {
            var actions = state.Actions.Where(a => a.ActionType.Equals(actionType)).ToList();
            actions.ForEach(a => a.Action(state));
        }

        private static async Task ExecuteActionsAsync(this State state, StateActionType actionType)
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

        private static List<State> FlattenStates(this State state, List<State> states = null)
        {
            if (states == null)
            {
                states = new List<State>();
            }

            if (!states.Contains(state))
            {
                states.Add(state);
            }

            if (state.SubStates.Any())
            {
                state.SubStates.ForEach(s => s.FlattenStates(states));
            }

            return states;
        }

        private static async Task<State> FailToTransitionStateAsync(this State state, State failTransitionState)
        {
            if (state != null)
            {
                if (state.Parent != null
                    &&
                    state.Parent.SubStates.Count(
                        s =>
                            s.Status.Equals(StateStatus.Uninitialise) ||
                            s.Status.Equals(StateStatus.Fail)).Equals(state.Parent.SubStates.Count))
                {
                    await state.Parent.ResetAsync().ConfigureAwait(false);
                }

                if (failTransitionState != null)
                {
                    if (state.Id.Equals(failTransitionState.Id))
                    {
                        return state;
                    }

                    var tranitionState = await state.Antecedent.FailToTransitionStateAsync(failTransitionState).ConfigureAwait(false);
                    await state.ResetAsync().ConfigureAwait(false);
                    await tranitionState.ResetAsync().ConfigureAwait(false);
                    return tranitionState;
                }

                await state.ResetAsync().ConfigureAwait(false);
            }

            return null;
        }

        private static State FailToTransitionState(this State state, State failTransitionState)
        {
            if (state != null)
            {
                if (state.Parent != null
                    &&
                    state.Parent.SubStates.Count(
                        s =>
                            s.Status.Equals(StateStatus.Uninitialise) ||
                            s.Status.Equals(StateStatus.Fail)).Equals(state.Parent.SubStates.Count))
                {
                    state.Parent.Reset();
                }

                if (failTransitionState != null)
                {
                    if (state.Id.Equals(failTransitionState.Id))
                    {
                        return state;
                    }

                    var tranitionState = state.Antecedent.FailToTransitionState(failTransitionState);
                    state.Reset();
                    tranitionState.Reset();
                    return tranitionState;
                }

                state.Reset();
            }

            return null;
        }

        private static List<State> SubStatesToInitialiseWithParent(this State state)
        {
            return state.SubStates.Where(s => s.InitialiseWithParent).ToList();
        }

        private static bool CanExecute(this State state, StateStatus newStatus)
        {
            return !state.Status.Equals(newStatus);
        }

        private static void PreExecuteSetup(this State state, StateStatus newStatus, State transitionState)
        {
            if (newStatus.Equals(StateStatus.Fail))
            {
                state.Status = StateStatus.Fail;
            }

            if (transitionState != null
                && (newStatus.Equals(StateStatus.Complete)
                    || newStatus.Equals(StateStatus.Fail)))
            {
                state.Transition = transitionState;
            }
        }

        private static void SetDefaultTransition(this State state)
        {
            if (state.Transition == null
                && state.Transitions.Count.Equals(1))
            {
                state.Transition = state.Transitions.First();
            }
        }

        private static void UpdateParentStatusToInProgress(this State state)
        {
            if (state.Parent == null
                || state.Parent.Status.Equals(StateStatus.InProgress))
            {
                return;
            }

            var aggregate = state.Parent;
            if (aggregate.Status.Equals(StateStatus.Complete))
            {
                var message =
                    String.Format("{0} {1} cannot be set to InProgress because it has already been set to Complete.",
                        aggregate.Id, aggregate.Name);
                aggregate.WriteLogEntry(message);
                throw new StateException(aggregate, message);
            }

            if (aggregate.SubStates.Any(s => s.Status.Equals(StateStatus.InProgress))
                || (aggregate.SubStates.Any(s => s.Status.Equals(StateStatus.Complete))
                    &&
                    !aggregate.SubStates.Count()
                        .Equals(aggregate.SubStates.Count(s => s.Status.Equals(StateStatus.Complete)))))
            {
                aggregate.Status = StateStatus.InProgress;
                aggregate.UpdateParentStatusToInProgress();
            }
        }

        private static bool CanTransition(this State state)
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
    }
}
