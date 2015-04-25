namespace DevelopmentInProgress.DipState
{
    public class SubState : DipStateDecorator
    {
        public SubState(IDipState state)
            : base(state)
        {
        }

        public bool InitialiseWithParent { get; set; }
    }
}
