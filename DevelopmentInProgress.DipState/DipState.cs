using System;
using System.Collections.Generic;

namespace DevelopmentInProgress.DipState
{
    public class DipState<T> : DipState
    {
        public DipState(T context, int id = 0, string name = "", DipStateType type = DipStateType.Standard,
            bool initialiseWithParent = false, bool canCompleteParent = false,
            DipStateStatus status = DipStateStatus.Uninitialised, Predicate<DipState> canComplete = null)
            : base(id, name, type, initialiseWithParent, canCompleteParent, status, canComplete)
        {
            Context = context;
        }

        public DipState(int id = 0, string name = "", DipStateType type = DipStateType.Standard,
            bool initialiseWithParent = false, bool canCompleteParent = false,
            DipStateStatus status = DipStateStatus.Uninitialised, Predicate<DipState> canComplete = null)
            : base (id, name, type, initialiseWithParent, canCompleteParent, status, canComplete)
        {            
        }

        public T Context { get; set; }
    }

    public class DipState
    {
        private readonly Predicate<DipState> canComplete;
        private DipStateStatus status;

        public DipState(int id = 0, string name = "", DipStateType type = DipStateType.Standard, 
            bool initialiseWithParent = false, bool canCompleteParent = false, 
            DipStateStatus status = DipStateStatus.Uninitialised, Predicate<DipState> canComplete = null)
        {
            Id = id;
            Name = name;
            Type = type;
            InitialiseWithParent = initialiseWithParent;
            CanCompleteParent = canCompleteParent;
            this.status = status;            
            this.canComplete = canComplete;
            Transitions = new List<DipState>();
            SubStates = new List<DipState>();
            Actions = new List<DipStateAction>();
            Dependencies = new List<DipState>();
            Dependants = new List<DipStateDependant>();
            Log = new List<LogEntry>();
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool IsDirty { get; internal set; }
        public bool InitialiseWithParent { get; private set; }
        public bool CanCompleteParent { get; private set; }
        public DipStateType Type { get; private set; }
        public DipState Parent { get; internal set; }
        public DipState Antecedent { get; internal set; }
        public DipState Transition { get; set; }
        public List<DipState> Transitions { get; private set; }
        public List<DipState> Dependencies { get; private set; }
        public List<DipStateDependant> Dependants { get; private set; }
        public List<DipState> SubStates { get; private set; }
        public List<DipStateAction> Actions { get; private set; }
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
                    Log.Add(new LogEntry(String.Format("{0} - {1}", Name ?? String.Empty, status)));
                }
            }
        }

        public bool CanComplete()
        {
            return canComplete == null || canComplete(this);
        }
    }
}