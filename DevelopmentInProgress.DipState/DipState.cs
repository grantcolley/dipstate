﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DevelopmentInProgress.DipState
{
    public class DipState<T> : DipState
    {
        public DipState(T context, int id = 0, string name = "", bool initialiseWithParent = false,
            bool canCompleteParent = false, DipStateType type = DipStateType.Standard,
            DipStateStatus status = DipStateStatus.Uninitialised, Predicate<DipState> canComplete = null)
            : base(id, name, initialiseWithParent, canCompleteParent, type, status, canComplete)
        {
            Context = context;
        }

        public DipState(int id = 0, string name = "", bool initialiseWithParent = false,
            bool canCompleteParent = false, DipStateType type = DipStateType.Standard,
            DipStateStatus status = DipStateStatus.Uninitialised, Predicate<DipState> canComplete = null)
            : base(id, name, initialiseWithParent, canCompleteParent, type, status, canComplete)
        {            
        }

        public new T Context { get; set; }
    }

    public class DipState
    {
        private readonly Predicate<DipState> canComplete;
        private readonly Func<DipState, Task<bool>> canCompleteAsync;
        private DipStateStatus status;

        public DipState(int id = 0, string name = "", bool initialiseWithParent = false, 
            bool canCompleteParent = false, DipStateType type = DipStateType.Standard, 
            DipStateStatus status = DipStateStatus.Uninitialised, Predicate<DipState> canComplete = null,
            Func<DipState, Task<bool>> canCompleteAsync = null)
        {
            Id = id;
            Name = name;
            Type = type;
            InitialiseWithParent = initialiseWithParent;
            CanCompleteParent = canCompleteParent;
            this.status = status;            
            this.canComplete = canComplete;
            this.canCompleteAsync = canCompleteAsync;
            Transitions = new List<DipState>();
            SubStates = new List<DipState>();
            Actions = new List<DipStateAction>();
            Dependencies = new List<DipState>();
            Dependants = new List<DipStateDependant>();
            Log = new List<LogEntry>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDirty { get; internal set; }
        public bool InitialiseWithParent { get; set; }
        public bool CanCompleteParent { get; set; }
        public object Context { get; set; }
        public DipStateType Type { get; set; }
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
                    var logEntry = new LogEntry(String.Format("{0} - {1}", Name ?? String.Empty, status));
                    Log.Add(logEntry);

                    #if DEBUG

                    Debug.WriteLine(logEntry);

                    #endif
                }
            }
        }

        public bool CanComplete()
        {
            return canComplete == null || canComplete(this);
        }

        public async Task<bool> CanCompleteAsync()
        {
            if (canCompleteAsync != null)
            {
                return await canCompleteAsync(this);
            }

            return true;
        }
    }
}