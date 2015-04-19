using System.Collections.Generic;

namespace DevelopmentInProgress.DipState
{
    public interface IDipState
    {
        int Id { get; }
        string Name { get; }
        int Position { get; }
        bool IsDirty { get; }
        IDipState Parent { get; }
        IDipState Antecedent { get; }
        IDipState Transition { get; }
        DipStateType Type { get; }
        IEnumerable<IDipState> Transitions { get; }
        IEnumerable<IDipState> Dependencies { get; }
        IEnumerable<IDipState> SubStates { get; }
        IEnumerable<StateAction> Actions { get; }
        DipStateStatus Status { get; }
        bool CanComplete();
    }
}