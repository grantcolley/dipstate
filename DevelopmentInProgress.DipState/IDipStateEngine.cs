using System.Threading.Tasks;

namespace DevelopmentInProgress.DipState
{
    public interface IDipStateEngine
    {
        DipState Run(DipState state, DipStateStatus newStatus);
        DipState Run(DipState state, DipState transitionState);
        DipState Run(DipState state, DipStateStatus newStatus, DipState transitionState);

        Task<DipState> RunAsync(DipState state, DipStateStatus newStatus);
        Task<DipState> RunAsync(DipState state, DipState transitionState);
        Task<DipState> RunAsync(DipState state, DipStateStatus newStatus, DipState transitionState);
    }
}