using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class AsyncGenericStateTest
    {
        [TestMethod]
        public async Task Run_InitialiseStateWithEntryAction_StateInitialisedEntryActionExecuted()
        {
            // Arrange
            var state = new State<ContextText>(
                new ContextText() {Text = "Uninitialise"}, 1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncGenericEntryAction);

            // Act
            state = await state.ExecuteAsync(StateStatus.Initialise);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialise);
            Assert.AreEqual(((State<ContextText>)state).Context.Text, "Entry Action");
        }
    }
}
