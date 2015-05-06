using System;
using System.Collections.Generic;

namespace DevelopmentInProgress.DipState
{
    public interface IDipState
    {
        int Id { get; }
        string Name { get; }
        bool IsDirty { get; }
        bool InitialiseWithParent { get; }
        DipStateType Type { get; }
        DipStateStatus Status { get; }
        IDipState Parent { get; }
        IDipState Antecedent { get; }
        IDipState Transition { get; set; }
        List<IDipState> Transitions { get; }
        List<IDipState> Dependencies { get; }
        List<IDipState> SubStates { get; }
        List<StateAction> Actions { get; }
        List<LogEntry> Log { get; }
        bool CanComplete();
        void Reset();
        DipState AddTransition(IDipState transition);
        DipState AddDependency(IDipState dependency);
        DipState AddSubState(IDipState subState);
        DipState AddAction(DipStateActionType actionType, Action<IDipState> action);
    }
}