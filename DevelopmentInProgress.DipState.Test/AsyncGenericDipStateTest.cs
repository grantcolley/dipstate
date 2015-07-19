using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class AsyncGenericDipStateTest
    {
        private DipStateEngine dipStateEngine;

        [TestInitialize]
        public void Initialise()
        {
            dipStateEngine = new DipStateEngine();
        }

        [TestMethod]
        public async Task Run_InitialiseStateWithEntryAction_StateInitialisedEntryActionExecuted()
        {
            // Arrange
            var state = new DipState<ContextText>(
                new ContextText() {Text = "Uninitialised"}, 1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncGenericEntryAction);

            // Act
            state = await dipStateEngine.RunAsync(state, DipStateStatus.Initialised);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
            Assert.AreEqual(((DipState<ContextText>)state).Context.Text, "Entry Action");
        }
    }
}
