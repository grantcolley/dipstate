namespace DevelopmentInProgress.DipState
{
    public interface IDipStateEngine
    {
        IDipState Run(IDipState state, IDipState transitionState);
        IDipState Run(IDipState state, DipStateStatus newStatus, IDipState transitionState = null);
    }
}
