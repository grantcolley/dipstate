using System;

namespace DevelopmentInProgress.DipState
{
    public class DipStateAction
    {
        public DipStateActionType ActionType { get; set; }
        public Action<DipState> Action { get; set; }
    }
}
