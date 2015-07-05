using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public static DipState AddActionAsync(this DipState state, DipStateActionType actionType, Func<DipState, Task<DipState>> action)
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
