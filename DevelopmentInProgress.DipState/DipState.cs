﻿using System;
using System.Collections.Generic;

namespace DevelopmentInProgress.DipState
{
    public class DipState
    {
        private readonly Predicate<DipState> canComplete;
        private DipStateStatus status;

        public DipState(int id = 0, string name = "", DipStateType type = DipStateType.Standard, 
            bool initialiseWithParent = false, DipStateStatus status = DipStateStatus.Uninitialised, Predicate<DipState> canComplete = null)
        {
            Id = id;
            Name = name;
            Type = type;
            InitialiseWithParent = initialiseWithParent;
            this.status = status;            
            this.canComplete = canComplete;
            Transitions = new List<DipState>();
            SubStates = new List<DipState>();
            Actions = new List<DipStateAction>();
            Dependencies = new List<DipState>();
            Dependants = new List<DipState>();
            Log = new List<LogEntry>();
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool IsDirty { get; private set; }
        public bool InitialiseWithParent { get; private set; }
        public DipStateType Type { get; private set; }        
        public DipState Parent { get; private set; }
        public DipState Antecedent { get; internal set; }
        public DipState Transition { get; set; }
        public List<DipState> Transitions { get; private set; }
        public List<DipState> Dependencies { get; private set; }
        internal List<DipState> Dependants { get; private set; }
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

        public void Reset()
        {
            Transition = null;
            Antecedent = null;
            Status = DipStateStatus.Uninitialised;
            SubStates.ForEach(s => s.Reset());
            IsDirty = false;
        }

        public DipState AddSubState(DipState subState)
        {
            subState.Parent = this;
            SubStates.Add(subState);
            return this;
        }

        public DipState AddTransition(DipState transition)
        {
            Transitions.Add(transition);
            return this;
        }

        public DipState AddAction(DipStateActionType actionType, Action<DipState> action)
        {
            Actions.Add(new DipStateAction() { ActionType = actionType, Action = action });
            return this;
        }

        public DipState AddDependency(DipState dependency, bool initialiseWhenDependentCompleted = false)
        {
            dependency.Dependants.Add(this);
            Dependencies.Add(dependency);
            return this;
        }
    }
}