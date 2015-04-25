using System;

namespace DevelopmentInProgress.DipState
{
    public class StateAction
    {
        public DipStateActionType ActionType { get; set; }
        public Action<IDipState> Action { get; set; }
    }
}
