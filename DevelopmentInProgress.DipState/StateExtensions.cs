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
        /// Asynchronously executes the state according to type of execution provided.
        /// </summary>
        /// <param name="state">The state to execute.</param>
        /// <param name="executionType">The type of action the execution must perform on the state.</param>
        /// <returns>An awaitable task of type <see cref="State"/> which is the result of the execution.</returns>
        public static async Task<State> ExecuteAsync(this State state, StateExecutionType executionType)
        {
            return await state.ExecuteAsync(executionType, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously transition the state to the transition state provided.
        /// </summary>
        /// <param name="state">The state to transition from.</param>
        /// <param name="transitionToState">The state to transition to.</param>
        /// <param name="transitionWithoutComplete">Indicates whether the state must transition without completing. Set to false by default.</param>
        /// <returns>An awaitable task of type <see cref="State"/>. Typically this is the state that has been transitioned to.</returns>
        public static async Task<State> ExecuteAsync(this State state, State transitionToState, bool transitionWithoutComplete = false)
        {
            state.Transition = transitionToState;
            return await state.ExecuteAsync(StateExecutionType.Complete, transitionWithoutComplete).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously executes the state based on the status or transition state provided.
        /// </summary>
        /// <param name="state">The state to execute.</param>
        /// <param name="executionType">The type of action the execution must perform on the state.</param>
        /// <param name="transitionWithoutComplete">Indicates whether the state must transition without completing. Set to false by default.</param>
        /// <returns>An awaitable task of type <see cref="State"/> which is the result of the execution.</returns>
        private static async Task<State> ExecuteAsync(this State state, StateExecutionType executionType, bool transitionWithoutComplete)
        {
            var newStatus = ExecutionTypeToStatusConverter(executionType);
            if (!state.CanExecute(newStatus))
            {
                return state;
            }

            switch (executionType)
            {
                case StateExecutionType.Complete:
                    return await state.TransitionAsync(transitionWithoutComplete).ConfigureAwait(false);
                case StateExecutionType.Initialise:
                    return await state.InitialiseAsync().ConfigureAwait(false);
                case StateExecutionType.Reset:
                    await state.ResetAsync().ConfigureAwait(false);
                    return state;
                default:
                    return await state.ChangeStatusAsync(newStatus).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Synchronously executes the state according to type of execution provided.
        /// </summary>
        /// <param name="state">The state to execute.</param>
        /// <param name="executionType">The type of action the execution must perform on the state.</param>
        /// <returns>The result of the execution.</returns>
        public static State Execute(this State state, StateExecutionType executionType)
        {
            return state.Execute(executionType, false);
        }

        /// <summary>
        /// Synchronously transition the state to the transition state provided.
        /// </summary>
        /// <param name="state">The state to transition from.</param>
        /// <param name="transitionToState">The state to transition to.</param>
        /// <param name="transitionWithoutComplete">Indicates whether the state must transition without completing. Set to false by default.</param>
        /// <returns>The state that has been transitioned to.</returns>
        public static State Execute(this State state, State transitionToState, bool transitionWithoutComplete = false)
        {
            state.Transition = transitionToState;
            return state.Execute(StateExecutionType.Complete, transitionWithoutComplete);
        }

        /// <summary>
        /// Synchronously executes the state based on the status or transition state provided.
        /// </summary>
        /// <param name="state">The state to execute.</param>
        /// <param name="executionType">The type of action the execution must perform on the state.</param>
        /// <param name="transitionWithoutComplete">Indicates whether the state must transition without completing. Set to false by default.</param>
        /// <returns>The result of the execution.</returns>
        private static State Execute(this State state, StateExecutionType executionType, bool transitionWithoutComplete)
        {
            var newStatus = ExecutionTypeToStatusConverter(executionType);
            if (!state.CanExecute(newStatus))
            {
                return state;
            }

            switch (executionType)
            {
                case StateExecutionType.Complete:
                    return state.Transition(transitionWithoutComplete);
                case StateExecutionType.Initialise:
                    return state.Initialise();
                case StateExecutionType.Reset:
                    state.Reset();
                    return state;
                default:
                    return state.ChangeStatus(newStatus);
            }
        }

        /// <summary>
        /// Add a substate.
        /// </summary>
        /// <param name="state">The state for which a substate is added.</param>
        /// <param name="subState">The substate to add.</param>
        /// <param name="initialiseWithParent">Indicates whether the state is initialised when its parent gets initialised.</param>
        /// <param name="completionRequired">Indicates whether completion is required in order for its parent to complete.</param>
        /// <returns></returns>
        public static State AddSubState(this State state, State subState, bool initialiseWithParent = false, bool completionRequired = true)
        {
            subState.Parent = state;
            subState.InitialiseWithParent = initialiseWithParent;
            subState.CompletionRequired = completionRequired;
            state.SubStates.Add(subState);
            return state;
        }

        /// <summary>
        /// Add a transition state that the state can be transitioned to.
        /// </summary>
        /// <param name="state">The state for which a transition state is added.</param>
        /// <param name="transition">The transition state to add.</param>
        /// <param name="isDefaultTransition">Indicates whether it is the default state to transition to when the state is completed. 
        /// There can only be one default state to transition to. The default can be overriden by explicitly transitioning to another state.</param>
        /// <returns>The state for which a transition state is added.</returns>
        public static State AddTransition(this State state, State transition, bool isDefaultTransition = false)
        {
            if (isDefaultTransition)
            {
                state.Transition = transition;
            }

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

            if (!state.Dependants.Any(d => d.Dependant.Equals(dependant)))
            {
                state.Dependants.Add(new StateDependant()
                {
                    Dependant = dependant,
                    InitialiseDependantWhenComplete = initialiseDependantWhenComplete
                });
            }

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

            if (!state.Dependencies.Any(d => d.Equals(dependency)))
            {
                state.Dependencies.Add(dependency);
            }

            return state;
        }

        /// <summary>
        /// Add a predicate which is executed synchronously to determine whether the state status can be changed i.e. passes validation.
        /// </summary>
        /// <param name="state">The state for which the predicate is added.</param>
        /// <param name="predicate">The predicate to add.</param>
        /// <returns>Returns the state.</returns>
        public static State AddCanChangeStatusPredicate(this State state, Predicate<State> predicate)
        {
            state.CanChangeStateStatus = predicate;
            return state;
        }

        /// <summary>
        /// Add a predicate which is executed synchronously to determine whether the state can be initialised.
        /// </summary>
        /// <param name="state">The state for which the predicate is added.</param>
        /// <param name="predicate">The predicate to add.</param>
        /// <returns>Returns the state.</returns>
        public static State AddCanInitialisePredicate(this State state, Predicate<State> predicate)
        {
            state.CanInitialiseState = predicate;
            return state;
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
        /// Add a predicate which is executed synchronously to determine whether the state can be reset.
        /// </summary>
        /// <param name="state">The state for which the predicate is added.</param>
        /// <param name="predicate">The predicate to add.</param>
        /// <returns>Returns the state.</returns>
        public static State AddCanResetPredicate(this State state, Predicate<State> predicate)
        {
            state.CanResetState = predicate;
            return state;
        }

        /// <summary>
        /// Add a predicate which is executed asynchronously to determine whether the state can be initialised.
        /// </summary>
        /// <param name="state">The state for which the predicate is added.</param>
        /// <param name="asyncPredicate">The async predicate to add.</param>
        /// <returns>Returns the state.</returns>
        public static State AddCanInitialisePredicateAsync(this State state, Func<State, Task<bool>> asyncPredicate)
        {
            state.CanInitialiseStateAsync = asyncPredicate;
            return state;
        }

        /// <summary>
        /// Add a predicate which is executed asynchronously to determine whether the state can be changed i.e. passes validation.
        /// </summary>
        /// <param name="state">The state for which the predicate is added.</param>
        /// <param name="asyncPredicate">The async predicate to add.</param>
        /// <returns>Returns the state.</returns>
        public static State AddCanChangeStatusPredicateAsync(this State state, Func<State, Task<bool>> asyncPredicate)
        {
            state.CanChangeStateStatusAsync = asyncPredicate;
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
        /// Add a predicate which is executed asynchronously to determine whether the state can be reset.
        /// </summary>
        /// <param name="state">The state for which the predicate is added.</param>
        /// <param name="asyncPredicate">The async predicate to add.</param>
        /// <returns>Returns the state.</returns>
        public static State AddCanResetPredicateAsync(this State state, Func<State, Task<bool>> asyncPredicate)
        {
            state.CanResetStateAsync = asyncPredicate;
            return state;
        }

        /// <summary>
        /// Executes a predicate synchronously to determine whether the state can initialise.
        /// </summary>
        /// <returns>Returns true if the state can be initialised, else returns false. If no predicate is provided returns true.</returns>
        public static bool CanInitialise(this State state)
        {
            return state.CanInitialiseState == null || state.CanInitialiseState(state);
        }

        /// <summary>
        /// Executes a predicate synchronously to determine whether the state can change status.
        /// </summary>
        /// <returns>Returns true if the state can change status, else returns false. If no predicate is provided returns true.</returns>
        public static bool CanChangeStatus(this State state)
        {
            return state.CanChangeStateStatus == null || state.CanChangeStateStatus(state);
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
        /// Executes a predicate synchronously to determine whether the state can reset.
        /// </summary>
        /// <returns>Returns true if the state can be reset, else returns false. If no predicate is provided returns true.</returns>
        public static bool CanReset(this State state)
        {
            return state.CanResetState == null || state.CanResetState(state);
        }

        /// <summary>
        /// Executes a predicate asynchronously to determine whether the state can initialise.
        /// </summary>
        /// <returns>Returns true if the state can be initialised, else returns false. If no predicate is provided returns true.</returns>
        public static async Task<bool> CanInitialiseAsync(this State state)
        {
            if (state.CanInitialiseStateAsync != null)
            {
                return await state.CanInitialiseStateAsync(state);
            }

            return true;
        }

        /// <summary>
        /// Executes a predicate asynchronously to determine whether the state can change status.
        /// </summary>
        /// <returns>Returns true if the state can change status, else returns false. If no predicate is provided returns true.</returns>
        public static async Task<bool> CanChangeStatusAsync(this State state)
        {
            if (state.CanChangeStateStatusAsync != null)
            {
                return await state.CanChangeStateStatusAsync(state);
            }

            return true;
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
        /// Executes a predicate asynchronously to determine whether the state can reset.
        /// </summary>
        /// <returns>Returns true if the state can be reset, else returns false. If no predicate is provided returns true.</returns>
        public static async Task<bool> CanResetAsync(this State state)
        {
            if (state.CanResetStateAsync != null)
            {
                return await state.CanResetStateAsync(state);
            }

            return true;
        }

        /// <summary>
        /// Determines whether the state has any dependency states that have not yet been completed. This will determine whether the state can be initialised or not.
        /// </summary>
        /// <param name="state">The state for which to check if it has any dependencies that are not yet complete.</param>
        /// <returns>True if the state has any dependencies that are not yet complete, else returns false.</returns>
        public static bool HasDependencies(this State state)
        {
            var dependencies = state.Dependencies.Where(d => !d.Status.Equals(StateStatus.Completed)).ToList();
            if (dependencies.Any(d => !d.Status.Equals(StateStatus.Completed)))
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

        /// <summary>
        /// Reset the state synchronously to uninitialised. This will also reset any substates, dependents and transitions.
        /// </summary>
        /// <param name="state">The state to reset.</param>
        /// <param name="hardReset">If true will also set the antecedent to null. The default is true.
        /// This is set to false when the reset is initiated from a transition, in which case we maintain a reference to the antecedent.</param>
        private static void Reset(this State state, bool hardReset = true)
        {
            if (state.Status.Equals(StateStatus.Uninitialised))
            {
                return;
            }

            if (!state.CanReset())
            {
                state.WriteLogEntry(String.Format("{0} is unable to reset", state.Name));
                return;
            }

            state.SubStates.ForEach(s => s.Reset());

            if (state.SubStates.Any(s => !s.Status.Equals(StateStatus.Uninitialised)))
            {
                state.WriteLogEntry(String.Format("{0} has one or more sub states that are unable to reset", state.Name));
                return;
            }

            state.Status = StateStatus.Uninitialised;
            state.ExecuteActions(StateActionType.Reset);

            if (hardReset)
            {
                state.Antecedent = null;
            }

            state.Log.Clear();
            state.IsDirty = false;

            state.Dependants.ForEach(d => d.Dependant.Reset());

            if (state.Transition != null)
            {
                state.Transition.Reset();
                state.Transition = null;
            }

            state.UpdateParentStatus();
        }

        /// <summary>
        /// Reset the state asynchronously to uninitialised. This will also reset any substates, dependents and transitions.
        /// </summary>
        /// <param name="state">The state to reset.</param>
        /// <param name="hardReset">If true will also set the antecedent to null. The default is true.
        /// This is set to false when the reset is initiated from a transition, in which case we maintain a reference to the antecedent.</param>
        /// <returns>An awaitable task.</returns>
        private static async Task ResetAsync(this State state, bool hardReset = true)
        {
            if (state.Status.Equals(StateStatus.Uninitialised))
            {
                return;
            }

            var canReset = await state.CanResetAsync().ConfigureAwait(false);
            if (!canReset)
            {
                state.WriteLogEntry(String.Format("{0} is unable to reset", state.Name));
                return;
            }

            foreach (var subState in state.SubStates)
            {
                await subState.ResetAsync().ConfigureAwait(false);
            }

            if (state.SubStates.Any(s => !s.Status.Equals(StateStatus.Uninitialised)))
            {
                state.WriteLogEntry(String.Format("{0} has one or more sub states that are unable to reset", state.Name));
                return;
            }

            state.Status = StateStatus.Uninitialised;
            await state.ExecuteActionsAsync(StateActionType.Reset).ConfigureAwait(false);

            if (hardReset)
            {
                state.Antecedent = null;
            }

            state.Log.Clear();
            state.IsDirty = false;

            foreach (var dependant in state.Dependants)
            {
                await dependant.Dependant.ResetAsync();
            }

            if (state.Transition != null)
            {
                await state.Transition.ResetAsync();
                state.Transition = null;
            }

            state.UpdateParentStatus();
        }

        private static async Task<State> InitialiseAsync(this State state)
        {
            var canInitialise = await state.CanInitialiseAsync().ConfigureAwait(false);
            if (!canInitialise)
            {
                state.WriteLogEntry(String.Format("{0} is unable to initialise", state.Name));
                return state;
            }

            if (state.HasDependencies())
            {
                return state;
            }

            await state.ExecuteActionsAsync(StateActionType.Entry).ConfigureAwait(false);

            state.Status = StateStatus.Initialised;

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

            state.UpdateParentStatus();

            return state;
        }

        private static State Initialise(this State state)
        {
            if (!state.CanInitialise())
            {
                state.WriteLogEntry(String.Format("{0} is unable to initialise", state.Name));
                return state;
            }

            if (state.HasDependencies())
            {
                return state;
            }

            state.ExecuteActions(StateActionType.Entry);

            state.Status = StateStatus.Initialised;

            state.ExecuteActions(StateActionType.Status);

            if (state.Type.Equals(StateType.Auto))
            {
                return state.Transition();
            }

            state.SubStatesToInitialiseWithParent().ForEach(s => s.Initialise());

            state.UpdateParentStatus();

            return state;
        }

        private static async Task<State> TransitionAsync(this State state, bool transitionWithoutComplete = false)
        {
            if (!state.CanTransition())
            {
                state.Transition = null;
                throw new StateException(state, String.Format("{0} failed to transition. Check logs.", state.Name));
            }

            if (transitionWithoutComplete
                && state.Transition != null)
            {
                var transitionedState = state.Transition;

                if (!state.Transition.Status.Equals(StateStatus.Uninitialised))
                {
                    await state.Transition.ResetAsync(false);
                }

                return await transitionedState.InitialiseAsync().ConfigureAwait(false);
            }

            var completeState = await state.TryCompleteStateAsync().ConfigureAwait(false);
            if (!completeState)
            {
                state.Transition = null;
                return state;
            }

            var dependants = state.Dependants.Where(d => d.InitialiseDependantWhenComplete).ToList();
            foreach (var dependant in dependants)
            {
                await dependant.Dependant.InitialiseAsync().ConfigureAwait(false);
            }

            if (state.Transition != null)
            {
                state.Transition.Antecedent = state;
                return await state.Transition.InitialiseAsync().ConfigureAwait(false);
            }
             
            if (state.Parent != null)
            {
                if (state.Parent.SubStates.Count(s => s.Status.Equals(StateStatus.Completed))
                    .Equals(state.Parent.SubStates.Count()))
                {
                    return await state.Parent.TransitionAsync().ConfigureAwait(false);
                }

                if (state.CompletionRequired
                    && state.Parent.SubStates.Count(s => s.CompletionRequired)
                        .Equals(
                            state.Parent.SubStates.Count(
                                s => s.CompletionRequired && s.Status.Equals(StateStatus.Completed))))
                {
                    return await state.Parent.TransitionAsync().ConfigureAwait(false);
                }
            }

            return state;
        }

        private static State Transition(this State state, bool transitionWithoutComplete = false)
        {
            if (!state.CanTransition())
            {
                state.Transition = null;
                throw new StateException(state, String.Format("{0} failed to transition. Check logs.", state.Name));
            }

            if (transitionWithoutComplete
                && state.Transition != null)
            {
                var transitionedState = state.Transition;

                if (!state.Transition.Status.Equals(StateStatus.Uninitialised))
                {
                    transitionedState.Reset(false);
                }

                return transitionedState.Initialise();
            }

            if (!state.TryCompleteState())
            {
                state.Transition = null;
                return state;
            }

            var initialiseDependants = state.Dependants.Where(d => d.InitialiseDependantWhenComplete).ToList();
            initialiseDependants.ForEach(d => d.Dependant.Initialise());

            if (state.Transition != null)
            {
                state.Transition.Antecedent = state;
                return state.Transition.Initialise();
            }
           
            if (state.Parent != null)
            {
                if (state.Parent.SubStates.Count(s => s.Status.Equals(StateStatus.Completed))
                    .Equals(state.Parent.SubStates.Count()))
                {
                    return state.Parent.Transition();
                }

                if (state.CompletionRequired
                    && state.Parent.SubStates.Count(s => s.CompletionRequired)
                        .Equals(
                            state.Parent.SubStates.Count(
                                s => s.CompletionRequired && s.Status.Equals(StateStatus.Completed))))
                {
                    return state.Parent.Transition();
                }
            }

            return state;
        }

        private static async Task<State> ChangeStatusAsync(this State state, StateStatus newStatus)
        {
            var canChangeStatus = await state.CanChangeStatusAsync().ConfigureAwait(false);
            if (!canChangeStatus)
            {
                state.WriteLogEntry(String.Format("{0} is unable to change status", state.Name));
                return state;
            }

            state.Status = newStatus;

            await state.ExecuteActionsAsync(StateActionType.Status).ConfigureAwait(false);

            state.UpdateParentStatus();

            return state;
        }

        private static State ChangeStatus(this State state, StateStatus newStatus)
        {
            if (!state.CanChangeStatus())
            {
                state.WriteLogEntry(String.Format("{0} is unable to change status", state.Name));
                return state;
            }

            state.Status = newStatus;

            state.ExecuteActions(StateActionType.Status);

            state.UpdateParentStatus();

            return state;
        }

        private static async Task<bool> TryCompleteStateAsync(this State state)
        {
            var canComplete = await state.CanCompleteAsync().ConfigureAwait(false);
            if (!canComplete)
            {
                state.WriteLogEntry(String.Format("{0} is unable to complete", state.Name));
                return false;
            }

            await state.ExecuteActionsAsync(StateActionType.Exit).ConfigureAwait(false);

            state.Status = StateStatus.Completed;

            await state.ExecuteActionsAsync(StateActionType.Status).ConfigureAwait(false);

            state.UpdateParentStatus();

            return true;
        }

        private static bool TryCompleteState(this State state)
        {
            if (!state.CanComplete())
            {
                state.WriteLogEntry(String.Format("{0} is unable to complete", state.Name));
                return false;
            }

            state.ExecuteActions(StateActionType.Exit);

            state.Status = StateStatus.Completed;

            state.ExecuteActions(StateActionType.Status);

            state.UpdateParentStatus();

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

        private static List<State> SubStatesToInitialiseWithParent(this State state)
        {
            return state.SubStates.Where(s => s.InitialiseWithParent).ToList();
        }

        private static bool CanExecute(this State state, StateStatus newStatus)
        {
            return !state.Status.Equals(newStatus);
        }

        private static void UpdateParentStatus(this State state)
        {
            if (state.Parent == null)
            {
                return;
            }

            var aggregate = state.Parent;

            if (aggregate.SubStates.Any(s => s.Status.Equals(StateStatus.InProgress))
                || (aggregate.SubStates.Any(s => s.Status.Equals(StateStatus.Completed))
                    && !aggregate.SubStates.All(s => s.Status.Equals(StateStatus.Completed))))
            {
                aggregate.Status = StateStatus.InProgress;
                aggregate.UpdateParentStatus();
                return;
            }

            if (aggregate.SubStates.Any(s => s.Status.Equals(StateStatus.Initialised)))
            {
                aggregate.Status = StateStatus.Initialised;
                aggregate.UpdateParentStatus();
                return;
            }

            if (aggregate.SubStates.All(s => s.Status.Equals(StateStatus.Uninitialised)))
            {
                aggregate.Status = StateStatus.Uninitialised;
                aggregate.UpdateParentStatus();
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

        private static StateStatus ExecutionTypeToStatusConverter(StateExecutionType executionType)
        {
            switch (executionType)
            {
                case StateExecutionType.Initialise:
                    return StateStatus.Initialised;
                case StateExecutionType.Complete:
                    return StateStatus.Completed;
                case StateExecutionType.InProgress:
                    return StateStatus.InProgress;
                case StateExecutionType.Reset:
                    return StateStatus.Uninitialised;
                default:
                    throw new InvalidCastException(String.Format("ExecutionTypeToStatusConverter : {0}", executionType));
            }
        }
    }
}
