using System;
using System.Threading.Tasks;

namespace DevelopmentInProgress.DipState
{
    public class DipStateAction
    {
        public DipStateActionType ActionType { get; set; }
        public Action<DipState> Action { get; set; }
        public Func<DipState, Task> ActionAsync { get; set; }

        public bool IsActionAsync
        {
            get { return (ActionAsync != null); }
        }
    }
}