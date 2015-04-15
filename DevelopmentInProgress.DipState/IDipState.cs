using System;
using System.Collections.Generic;

namespace DevelopmentInProgress.DipState
{
    public interface IDipState
    {
        int Id { get; set; }
        string Name { get; set; }
        IDipState Parent { get; set; }
        IDipState AutoTransition { get; set; }
        DipStateStatus Status { get; set; }
        DipStateType Type { get; set; }
        IEnumerable<IDipState> Transitions { get; set; }
        IEnumerable<IDipState> SubStates { get; set; }
        IEnumerable<Action<IDipState>> Actions { get; set; }
        bool CanComplete();
    }
}