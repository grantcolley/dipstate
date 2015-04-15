using System;
using System.Collections.Generic;

namespace DevelopmentInProgress.DipState
{
    public class DipState : IDipState
    {
        private readonly Predicate<IDipState> canComplete;

        public DipState(int id = 0, string name = "", Predicate<IDipState> canComplete = null)
        {
            Id = id;
            Name = name;
            this.canComplete = canComplete;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public IDipState Parent { get; set; }
        public IDipState AutoTransition { get; set; }
        public DipStateStatus Status { get; set; }
        public DipStateType Type { get; set; }
        public IEnumerable<IDipState> Transitions { get; set; }
        public IEnumerable<IDipState> Dependencies { get; set; }
        public IEnumerable<IDipState> SubStates { get; set; }
        public IEnumerable<Action<IDipState>> Actions { get; set; }

        public bool CanComplete()
        {
            return canComplete == null || canComplete(this);
        }
    }
}
