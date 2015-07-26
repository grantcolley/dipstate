using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DevelopmentInProgress.DipState
{
    /// <summary>
    /// Extension methods for <see cref="DipState"/>.
    /// </summary>
    public static class DipStateExtensions
    {
        /// <summary>
        /// Reset the state synchronously to uninitialised. This will also reset any substates.
        /// </summary>
        /// <param name="state">The state to reset.</param>
        /// <param name="clearLogs">A flag indicating whether the logs must be cleared with the reset.</param>
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

        /// <summary>
        /// Reset the state asynchronously to uninitialised. This will also reset any substates.
        /// </summary>
        /// <param name="state">The state to reset.</param>
        /// <param name="clearLogs">A flag indicating whether the logs must be cleared with the reset.</param>
        /// <returns>An awaitable task.</returns>
        public static async Task ResetAsync(this DipState state, bool clearLogs = false)
        {
            foreach (var subState in state.SubStates)
            {
                await subState.ResetAsync(clearLogs).ConfigureAwait(false);
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

        /// <summary>
        /// Runs the actions of the specified action type synchronously.
        /// </summary>
        /// <param name="state">The state for which the actions are run.</param>
        /// <param name="actionType">The specified action type.</param>
        public static void RunActions(this DipState state, DipStateActionType actionType)
        {
            var actions = state.Actions.Where(a => a.ActionType.Equals(actionType)).ToList();
            actions.ForEach(a => a.Action(state));
        }

        /// <summary>
        /// Runs the actions of the specified action type asynchronously.
        /// </summary>
        /// <param name="state">The state for which the actions are run.</param>
        /// <param name="actionType">The specified action type.</param>
        /// <returns>An awaitable task.</returns>
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

        /// <summary>
        /// Add a substate.
        /// </summary>
        /// <param name="state">The state for which a substate is added.</param>
        /// <param name="subState">The substate to add.</param>
        /// <returns></returns>
        public static DipState AddSubState(this DipState state, DipState subState)
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
        public static DipState AddTransition(this DipState state, DipState transition)
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
        public static DipState AddAction(this DipState state, DipStateActionType actionType, Action<DipState> action)
        {
            state.Actions.Add(new DipStateAction() { ActionType = actionType, Action = action });
            return state;
        }

        /// <summary>
        /// Add an action to be executed asynchronously.
        /// </summary>
        /// <param name="state">The state for which an action is added.</param>
        /// <param name="actionType">The type of action.</param>
        /// <param name="action">The action to be executed according to the type of action.</param>
        /// <returns>The state for which an action is added.</returns>
        public static DipState AddActionAsync(this DipState state, DipStateActionType actionType, Func<DipState, Task> action)
        {
            state.Actions.Add(new DipStateAction() { ActionType = actionType, ActionAsync = action });
            return state;
        }

        /// <summary>
        /// Add a dependant state to the state. The dependant state cannot be initialised until the state to which it is added has completed.
        /// </summary>
        /// <param name="state">The state to which the dependant is added.</param>
        /// <param name="dependant">The dependant state.</param>
        /// <param name="initialiseDependantWhenComplete">A flag indicating whether the dependant state is initialised when the state to which it is added is completed.</param>
        /// <returns>The state to which the dependant is added.</returns>
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

        /// <summary>
        /// Add a dependency state to the state. The dependancy state must be completed before the state to which it is added can be initialised.
        /// </summary>
        /// <param name="state">The state to which the dependancy state is added.</param>
        /// <param name="dependency">The dependency state.</param>
        /// <param name="initialiseWhenDependencyCompleted">A flag indicating whether the state will be initialised when the dependency state is completed.</param>
        /// <returns>The state to which the dependancy state is added.</returns>
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

        /// <summary>
        /// Initialises the substates of a parent along with the parent. The substates to be initialised must have its InitialiseWithParent field set to true.
        /// </summary>
        /// <param name="state">The state for which the substates are initialised.</param>
        /// <returns></returns>
        public static List<DipState> SubStatesToInitialiseWithParent(this DipState state)
        {
            return state.SubStates.Where(s => s.InitialiseWithParent).ToList();
        }

        /// <summary>
        /// Determines whether the state can be executed.
        /// </summary>
        /// <param name="state">The state to be executed.</param>
        /// <param name="newStatus">The new status of the state to be executed.</param>
        /// <returns>True if the state can be executed, else returns false.</returns>
        public static bool CanExecute(this DipState state, DipStateStatus newStatus)
        {
            return !state.Status.Equals(newStatus);
        }

        /// <summary>
        /// Pre-exection state setup.
        /// </summary>
        /// <param name="state">The state to be executed.</param>
        /// <param name="newStatus">The new status.</param>
        /// <param name="transitionState">The state to be transitioned to.</param>
        public static void PreExecuteSetup(this DipState state, DipStateStatus newStatus, DipState transitionState)
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

        /// <summary>
        /// If the state only has one other state in its transition list, then set that as the transition state.
        /// </summary>
        /// <param name="state">The state to be transitioned.</param>
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
                aggregate.UpdateParentStatusToInProgress();
            }
        }

        /// <summary>
        /// Determines whether the state has any dependency states that have not yet been completed. This will determine whether the state can be initialised or not.
        /// </summary>
        /// <param name="state">The state for which to check if it has any dependencies that are not yet complete.</param>
        /// <returns>True if the state has any dependencies that are not yet complete, else returns false.</returns>
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

        /// <summary>
        /// Determines whether the state can transition to another state.
        /// </summary>
        /// <param name="state">The state to check if it can be transitioned.</param>
        /// <returns>True if the state has another state it can be transitioned to, else returns false.</returns>
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

        /// <summary>
        /// Write a log entry to the state.
        /// </summary>
        /// <param name="state">The state for which to write a log entry.</param>
        /// <param name="message">The message to log.</param>
        public static void WriteLogEntry(this DipState state, string message)
        {
            var logEntry = new LogEntry(message);
            state.Log.Add(logEntry);

#if DEBUG

            Debug.WriteLine(logEntry);

#endif
        }

        /// <summary>
        /// Gets a flattened list of the workflow graph from the state. It does this by seeking the root state and then
        /// populating a list by traversing the graph from the root.
        /// </summary>
        /// <param name="state">The state for which to return a flattened graph.</param>
        /// <returns>A flattened list of states in the graph.</returns>
        public static List<DipState> Flatten(this DipState state)
        {
            var rootState = state.GetRoot();
            return rootState.FlattenStates();
        }

        /// <summary>
        /// Gets the root state from in the graph.
        /// </summary>
        /// <param name="state">The state for which to return the root.</param>
        /// <returns>The root state of the graph.</returns>
        public static DipState GetRoot(this DipState state)
        {
            if (state.Parent != null)
            {
                return GetRoot(state.Parent);
            }

            return state;
        }

        /// <summary>
        /// Fails to the specified transition state asynchrnonously. All states touched when traversing the graph to the state that will be failed to will be reset.
        /// </summary>
        /// <param name="state">The state to fail.</param>
        /// <param name="failTransitionState">The transition state to fail to.</param>
        /// <returns>An awaitable task of type <see cref="DipState"/>.</returns>
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

        /// <summary>
        /// Fails to the specified transition state synchrnonously. All states touched when traversing the graph to the state that will be failed to will be reset.
        /// </summary>
        /// <param name="state">The state to fail.</param>
        /// <param name="failTransitionState">The transition state to fail to.</param>
        /// <returns>The state to be failed to.</returns>
        public static DipState FailToTransitionState(this DipState state, DipState failTransitionState)
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

        /// <summary>
        /// Asynchronously executes the state based on the status provided.
        /// </summary>
        /// <param name="state">The state to execute.</param>
        /// <param name="newStatus">The new status of the state.</param>
        /// <returns>An awaitable task of type <see cref="DipState"/>.</returns>
        public static async Task<DipState> RunAsync(this DipState state, DipStateStatus newStatus)
        {
            return await state.RunAsync(newStatus, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously transition the state to the transition state provided.
        /// </summary>
        /// <param name="state">The state to transition from.</param>
        /// <param name="transitionState">The state to transition to.</param>
        /// <returns>An awaitable task of type <see cref="DipState"/>. Typically this is the state that has been transitioned to.</returns>
        public static async Task<DipState> RunAsync(this DipState state, DipState transitionState)
        {
            return await state.RunAsync(DipStateStatus.Completed, transitionState).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously executes the state based on the status or transition state provided.
        /// </summary>
        /// <param name="state">The state to execute.</param>
        /// <param name="newStatus">The new status of the state.</param>
        /// <param name="transitionState">The state to transition to.</param>
        /// <returns>An awaitable task of type <see cref="DipState"/> which is the result of the execution.</returns>
        public static async Task<DipState> RunAsync(this DipState state, DipStateStatus newStatus, DipState transitionState)
        {
            if (!state.CanExecute(newStatus))
            {
                return state;
            }

            state.PreExecuteSetup(newStatus, transitionState);

            switch (newStatus)
            {
                case DipStateStatus.Completed:
                case DipStateStatus.Failed:
                    return await state.TransitionAsync().ConfigureAwait(false);
                case DipStateStatus.Initialised:
                    return await state.InitialiseAsync().ConfigureAwait(false);
                case DipStateStatus.Uninitialised:
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
        /// <returns>A <see cref="DipState"/>.</returns>
        public static DipState Run(this DipState state, DipStateStatus newStatus)
        {
            return state.Run(newStatus, null);
        }

        /// <summary>
        /// Synchronously transition the state to the transition state provided.
        /// </summary>
        /// <param name="state">The state to transition from.</param>
        /// <param name="transitionState">The state to transition to.</param>
        /// <returns>The state that has been transitioned to.</returns>
        public static DipState Run(this DipState state, DipState transitionState)
        {
            return state.Run(DipStateStatus.Completed, transitionState);
        }

        /// <summary>
        /// Synchronously executes the state based on the status or transition state provided.
        /// </summary>
        /// <param name="state">The state to execute.</param>
        /// <param name="newStatus">The new status of the state.</param>
        /// <param name="transitionState">The state to transition to.</param>
        /// <returns>The result of the execution.</returns>
        public static DipState Run(this DipState state, DipStateStatus newStatus, DipState transitionState)
        {
            if (!state.CanExecute(newStatus))
            {
                return state;
            }

            state.PreExecuteSetup(newStatus, transitionState);

            switch (newStatus)
            {
                case DipStateStatus.Completed:
                case DipStateStatus.Failed:
                    return state.Transition();
                case DipStateStatus.Initialised:
                    return state.Initialise();
                case DipStateStatus.Uninitialised:
                    state.Reset();
                    return state;
                default:
                    return state.ChangeStatus(newStatus);
            }
        }

        private static async Task<DipState> InitialiseAsync(this DipState state)
        {
            if (state.HasDependencies())
            {
                return state;
            }

            await state.RunActionsAsync(DipStateActionType.Entry).ConfigureAwait(false);

            state.Status = DipStateStatus.Initialised;

            await state.RunActionsAsync(DipStateActionType.Status).ConfigureAwait(false);

            if (state.Type.Equals(DipStateType.Auto))
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

        private static DipState Initialise(this DipState state)
        {
            if (state.HasDependencies())
            {
                return state;
            }

            state.RunActions(DipStateActionType.Entry);

            state.Status = DipStateStatus.Initialised;

            state.RunActions(DipStateActionType.Status);

            if (state.Type.Equals(DipStateType.Auto))
            {
                return state.Transition();
            }

            state.SubStatesToInitialiseWithParent().ForEach(s => s.Initialise());

            return state;
        }

        private static async Task<DipState> TransitionAsync(this DipState state)
        {
            if (!state.CanTransition())
            {
                throw new DipStateException(state, String.Format("{0} failed to transition.", state.Name));
            }

            if (state.Status.Equals(DipStateStatus.Failed))
            {
                var stateFailedTo = await state.FailToTransitionStateAsync(state.Transition).ConfigureAwait(false);
                if (stateFailedTo != null)
                {
                    return await stateFailedTo.InitialiseAsync().ConfigureAwait(false);
                }

                state.WriteLogEntry(String.Format("{0} has failed but is unable to transition", state.Name));
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
                if (state.Parent.SubStates.Count(s => s.Status.Equals(DipStateStatus.Completed))
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
                                s => s.CanCompleteParent && s.Status.Equals(DipStateStatus.Completed))))
                {
                    state.Parent.Transition = state.Parent.Transitions.FirstOrDefault();
                    return await state.Parent.TransitionAsync().ConfigureAwait(false);
                }
            }

            return state;
        }

        private static DipState Transition(this DipState state)
        {
            if (!state.CanTransition())
            {
                throw new DipStateException(state, String.Format("{0} failed to transition.", state.Name));
            }

            if (state.Status.Equals(DipStateStatus.Failed))
            {
                var stateFailedTo = state.FailToTransitionState(state.Transition);
                if (stateFailedTo != null)
                {
                    return stateFailedTo.Initialise();
                }

                state.WriteLogEntry(String.Format("{0} has failed but is unable to transition", state.Name));
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
                if (state.Parent.SubStates.Count(s => s.Status.Equals(DipStateStatus.Completed))
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
                                s => s.CanCompleteParent && s.Status.Equals(DipStateStatus.Completed))))
                {
                    state.Parent.Transition = state.Parent.Transitions.FirstOrDefault();
                    return state.Parent.Transition();
                }
            }

            return state;
        }

        private static async Task<DipState> ChangeStatusAsync(this DipState state, DipStateStatus newStatus)
        {
            // If newStatus is Completed then just transition the
            // state with a null transition state to complete it.
            if (newStatus.Equals(DipStateStatus.Completed))
            {
                return await state.TransitionAsync().ConfigureAwait(false);
            }

            state.Status = newStatus;

            await state.RunActionsAsync(DipStateActionType.Status).ConfigureAwait(false);

            state.UpdateParentStatusToInProgress();

            return state;
        }

        private static DipState ChangeStatus(this DipState state, DipStateStatus newStatus)
        {
            // If newStatus is Completed then just transition the
            // state with a null transition state to complete it.
            if (newStatus.Equals(DipStateStatus.Completed))
            {
                return state.Transition();
            }

            state.Status = newStatus;

            state.RunActions(DipStateActionType.Status);

            state.UpdateParentStatusToInProgress();

            return state;
        }

        private static async Task<bool> TryCompleteStateAsync(this DipState state)
        {
            var canComplete = await state.CanCompleteAsync().ConfigureAwait(false);
            if (!canComplete)
            {
                var message = String.Format("{0} is unable to complete", state.Name);

                state.WriteLogEntry(message);

                throw new DipStateException(state, state.Log.Last().Message);
            }

            await state.RunActionsAsync(DipStateActionType.Exit).ConfigureAwait(false);

            state.Status = DipStateStatus.Completed;

            await state.RunActionsAsync(DipStateActionType.Status).ConfigureAwait(false);

            state.SetDefaultTransition();

            state.UpdateParentStatusToInProgress();

            return true;
        }

        private static bool TryCompleteState(this DipState state)
        {
            var canComplete = state.CanComplete();
            if (!canComplete)
            {
                var message = String.Format("{0} is unable to complete", state.Name);

                state.WriteLogEntry(message);

                throw new DipStateException(state, state.Log.Last().Message);
            }

            state.RunActions(DipStateActionType.Exit);

            state.Status = DipStateStatus.Completed;

            state.RunActions(DipStateActionType.Status);

            state.SetDefaultTransition();

            state.UpdateParentStatusToInProgress();

            return true;
        }

        private static List<DipState> FlattenStates(this DipState state, List<DipState> states = null)
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
                state.SubStates.ForEach(s => s.FlattenStates(states));
            }

            return states;
        }
    }
}
