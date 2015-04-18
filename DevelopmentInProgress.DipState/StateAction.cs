using System;

namespace DevelopmentInProgress.DipState
{
    public class StateAction
    {
        public DipStateAction Action { get; set; }
        public Action<IDipState> Delegate { get; set; }
    }
}
