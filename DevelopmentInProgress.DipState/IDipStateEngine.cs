namespace DevelopmentInProgress.DipState
{
    public interface IDipStateEngine
    {
        DipState Run(DipState state, DipStateStatus newStatus);
        DipState Run(DipState state, DipState transitionState);
        DipState Run(DipState state, DipStateStatus newStatus, DipState transitionState);
    }
}