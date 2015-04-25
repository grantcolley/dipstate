using System.Collections.Generic;

namespace DevelopmentInProgress.DipState
{
    public interface IDipState
    {
        int Id { get; }
        string Name { get; }
        bool IsDirty { get; }
        DipStateType Type { get; }
        DipStateStatus Status { get; }
        IDipState Parent { get; }
        IDipState Antecedent { get; }
        IDipState Transition { get; }
        List<IDipState> Transitions { get; }
        List<IDipState> Dependencies { get; }
        List<IDipState> SubStates { get; }
        List<StateAction> Actions { get; }
        List<LogEntry> Log { get; }
        bool CanComplete();
        void Reset();
    }
}