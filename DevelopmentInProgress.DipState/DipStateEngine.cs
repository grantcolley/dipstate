using System;
using System.Linq;
using System.Threading.Tasks;

namespace DevelopmentInProgress.DipState
{
    public class DipStateEngine : IDipStateEngine
    {
        public async Task<DipState> RunAsync(DipState state, DipStateStatus newStatus)
        {
            return await RunAsync(state, newStatus, null).ConfigureAwait(false);
        }

        public async Task<DipState> RunAsync(DipState state, DipState transitionState)
        {
            return await RunAsync(state, DipStateStatus.Completed, transitionState).ConfigureAwait(false);
        }

        public async Task<DipState> RunAsync(DipState state, DipStateStatus newStatus, DipState transitionState)
        {
            if (!state.CanRun(newStatus))
            {
                return state;
            }

            state.PreRunSetup(newStatus, transitionState);

            switch (newStatus)
            {
                case DipStateStatus.Completed:
                case DipStateStatus.Failed:
                    return await TransitionAsync(state).ConfigureAwait(false);
                case DipStateStatus.Initialised:
                    return await InitialiseAsync(state).ConfigureAwait(false);
                case DipStateStatus.Uninitialised:
                    await state.ResetAsync();
                    return state;
                default:
                    return await ChangeStatusAsync(state, newStatus).ConfigureAwait(false);
            }
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
            if (!state.CanRun(newStatus))
            {
                return state;
            }

            state.PreRunSetup(newStatus, transitionState);

            switch (newStatus)
            {
                case DipStateStatus.Completed:
                case DipStateStatus.Failed:
                    return Transition(state);
                case DipStateStatus.Initialised:
                    return Initialise(state);
                case DipStateStatus.Uninitialised:
                    state.Reset();
                    return state;
                default:
                    return ChangeStatus(state, newStatus);
            }
        }

        private async Task<DipState> InitialiseAsync(DipState state)
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
                return await TransitionAsync(state).ConfigureAwait(false);
            }

            var subStates = state.SubStatesToInitialiseWithParent();
            foreach (var subState in subStates)
            {
                await InitialiseAsync(subState).ConfigureAwait(false);
            }

            return state;
        }

        private DipState Initialise(DipState state)
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
                return Transition(state);
            }

            state.SubStatesToInitialiseWithParent().ForEach(s => Initialise(s));

            return state;
        }

        private async Task<DipState> TransitionAsync(DipState state)
        {
            if (!state.CanTransition())
            {
                throw new Exception(String.Format("{0} failed to transition.", state.Name));
            }

            if (state.Status.Equals(DipStateStatus.Failed))
            {
                var stateFailedTo = await state.FailToTransitionStateAsync(state.Transition);
                if (stateFailedTo == null)
                {
                    return await InitialiseAsync(stateFailedTo).ConfigureAwait(false);
                }

                state.WriteLogEntry(String.Format("{0} has failed but is unable to transition", state.Name));
                return state;
            }

            // Run exit actions and set the state's status to complete.
            var completeState = await TryCompleteStateAsync(state).ConfigureAwait(false);
            if (!completeState)
            {
                return state;
            }

            var dependants = state.Dependants.Where(d => d.InitialiseDependantWhenComplete).ToList();
            foreach (var dependant in dependants)
            {
                await InitialiseAsync(dependant.Dependant).ConfigureAwait(false);
            }

            // If we have a transition state then initialise and return it.
            if (state.Transition != null)
            {
                state.Transition.Antecedent = state;
                return await InitialiseAsync(state.Transition).ConfigureAwait(false);
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
                    return await TransitionAsync(state.Parent).ConfigureAwait(false);
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
                    return await TransitionAsync(state.Parent).ConfigureAwait(false);
                }
            }

            return state;
        }

        private DipState Transition(DipState state)
        {
            if (!state.CanTransition())
            {
                throw new Exception(String.Format("{0} failed to transition.", state.Name));
            }
            
            if (state.Status.Equals(DipStateStatus.Failed))
            {
                var stateFailedTo = state.FailToTransitionState(state.Transition);
                if (stateFailedTo != null)
                {
                    return Initialise(stateFailedTo);
                }

                state.WriteLogEntry(String.Format("{0} has failed but is unable to transition", state.Name));
                return state;
            }

            // Run exit actions and set the state's status to complete.
            if (!TryCompleteState(state))
            {
                return state;
            }

            var initialiseDependants = state.Dependants.Where(d => d.InitialiseDependantWhenComplete).ToList();
            initialiseDependants.ForEach(d => Initialise(d.Dependant));

            // If we have a transition state then initialise and return it.
            if (state.Transition != null)
            {
                state.Transition.Antecedent = state;
                return Initialise(state.Transition);
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
                    return Transition(state.Parent);
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
                    return Transition(state.Parent);
                }
            }

            return state;
        }

        private async Task<DipState> ChangeStatusAsync(DipState state, DipStateStatus newStatus)
        {
            // If newStatus is Completed then just transition the
            // state with a null transition state to complete it.
            if (newStatus.Equals(DipStateStatus.Completed))
            {
                return await TransitionAsync(state).ConfigureAwait(false);
            }

            state.Status = newStatus;

            await state.RunActionsAsync(DipStateActionType.Status).ConfigureAwait(false);

            state.UpdateParentStatusToInProgress();

            return state;
        }

        private DipState ChangeStatus(DipState state, DipStateStatus newStatus)
        {
            // If newStatus is Completed then just transition the
            // state with a null transition state to complete it.
            if (newStatus.Equals(DipStateStatus.Completed))
            {
                return Transition(state);
            }

            state.Status = newStatus;

            state.RunActions(DipStateActionType.Status);

            state.UpdateParentStatusToInProgress();

            return state;
        }

        private async Task<bool> TryCompleteStateAsync(DipState state)
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

            await state.RunActionsAsync(DipStateActionType.Status);

            state.SetDefaultTransition();

            state.UpdateParentStatusToInProgress();

            return true;
        }

        private bool TryCompleteState(DipState state)
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
    }
}
