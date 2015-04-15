using System;

namespace DevelopmentInProgress.DipState
{
    public class DipStateEngine : IDipStateEngine
    {
        public IDipState Run(IDipState state, IDipState transitionState)
        {
            return Run(state, DipStateStatus.Completed, transitionState);
        }

        public IDipState Run(IDipState state, DipStateStatus newStatus, IDipState transitionState = null)
        {
            throw new NotImplementedException();
        }
    }
}
