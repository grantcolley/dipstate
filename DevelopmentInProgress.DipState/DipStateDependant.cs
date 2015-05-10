namespace DevelopmentInProgress.DipState
{
    public class DipStateDependant
    {
        public bool InitialiseDependantWhenComplete { get; set; }
        public DipState Dependant { get; set; }
    }
}
