using System.Collections.Generic;

namespace DevelopmentInProgress.DipState
{
    public abstract class DipStateDecorator : IDipState
    {
        private readonly IDipState dipState;

        protected DipStateDecorator(IDipState dipState)
        {
            this.dipState = dipState;
        }

        public int Id
        {
            get { return dipState.Id; }
        }

        public string Name
        {
            get { return dipState.Name; }
        }

        public DipStateType Type
        {
            get { return dipState.Type; }
        }

        public bool IsDirty
        {
            get { return dipState.IsDirty; }
        }

        public IDipState Parent
        {
            get { return dipState.Parent; }
        }

        public IDipState Antecedent
        {
            get { return dipState.Antecedent; }
        }

        public IDipState Transition
        {
            get { return dipState.Transition; }
        }

        public List<IDipState> Transitions
        {
            get { return dipState.Transitions; }
        }

        public List<IDipState> Dependencies
        {
            get { return dipState.Dependencies; }
        }

        public List<IDipState> SubStates
        {
            get { return dipState.SubStates; }
        }

        public List<StateAction> Actions
        {
            get { return dipState.Actions; }
        }

        public List<LogEntry> Log
        {
            get { return dipState.Log; }
        }

        public DipStateStatus Status
        {
            get { return dipState.Status; }
            internal set
            {
                if (dipState.Status != value)
                {
                    ((DipState) dipState).Status = value;
                }
            }
        }

        public bool CanComplete()
        {
            return dipState.CanComplete();
        }

        public void Reset()
        {
            ((DipState) dipState).Reset();
        }
    }
}
