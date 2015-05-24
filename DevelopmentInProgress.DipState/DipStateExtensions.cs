using System;
using System.Linq;

namespace DevelopmentInProgress.DipState
{
    public static class DipStateExtensions
    {
        public static void Reset(this DipState state, bool clearLogs = false)
        {
            state.Transition = null;
            state.Antecedent = null;
            state.Status = DipStateStatus.Uninitialised;
            state.SubStates.ForEach(s => s.Reset());
            state.IsDirty = false;

            if (clearLogs)
            {
                state.Log.Clear();
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
    }
}
