using System;
using System.Collections.Generic;

namespace DevelopmentInProgress.DipState
{
    public class DipState : IDipState
    {
        private readonly Predicate<IDipState> canComplete;
        private DipStateStatus status;

        public DipState(int id = 0, string name = "", DipStateType type = DipStateType.Standard, DipStateStatus status = DipStateStatus.Uninitialised, Predicate<IDipState> canComplete = null)
        {
            Id = id;
            Name = name;
            Type = type;
            this.status = status;
            this.canComplete = canComplete;
            Transitions = new List<IDipState>();
            Dependencies = new List<IDipState>();
            SubStates = new List<IDipState>();
            Actions = new List<StateAction>();
            Log = new List<LogEntry>();
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool IsDirty { get; private set; }
        public DipStateType Type { get; private set; }        
        public IDipState Parent { get; private set; }
        public IDipState Antecedent { get; internal set; }
        public IDipState Transition { get; internal set; }
        public List<IDipState> Transitions { get; private set; }
        public List<IDipState> Dependencies { get; private set; }
        public List<IDipState> SubStates { get; private set; }
        public List<StateAction> Actions { get; private set; }
        public List<LogEntry> Log { get; private set; }

        public DipStateStatus Status
        {
            get { return status; }
            internal set
            {
                if (status != value)
                {
                    status = value;
                    IsDirty = true;
                }
            }
        }

        public bool CanComplete()
        {
            return canComplete == null || canComplete(this);
        }

        public void Reset()
        {
            IsDirty = false;
            Transition = null;
            Antecedent = null;
            Status = DipStateStatus.Uninitialised;
        }

        public IDipState AddTransition(IDipState transition)
        {
            Transitions.Add(transition);
            return this;
        }

        public IDipState AddDependency(IDipState dependency)
        {
            Dependencies.Add(dependency);
            return this;
        }

        public IDipState AddSubState(IDipState subState)
        {
            ((DipState)subState).Parent = this;
            SubStates.Add(subState);
            return this;
        }

        public IDipState AddAction(DipStateActionType actionType, Action<IDipState> action)
        {
            Actions.Add(new StateAction() {ActionType = actionType, Action = action});
            return this;
        }
    }
}