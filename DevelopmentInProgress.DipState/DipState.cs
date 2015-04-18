﻿using System;
using System.Collections.Generic;

namespace DevelopmentInProgress.DipState
{
    public class DipState : IDipState
    {
        private readonly Predicate<IDipState> canComplete;
        private DipStateStatus status;

        public DipState(int id = 0, string name = "", DipStateStatus status = DipStateStatus.NotStarted, Predicate<IDipState> canComplete = null)
        {
            Id = id;
            Name = name;
            this.status = status;
            this.canComplete = canComplete;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public bool IsDirty { get; private set; }
        public IDipState Parent { get; set; }
        public IDipState AutoTransition { get; set; }
        public DipStateType Type { get; set; }
        public IEnumerable<IDipState> Transitions { get; set; }
        public IEnumerable<IDipState> Dependencies { get; set; }
        public IEnumerable<IDipState> SubStates { get; set; }
        public IEnumerable<StateAction> Actions { get; set; }

        public DipStateStatus Status
        {
            get { return status; }
            set
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
    }
}