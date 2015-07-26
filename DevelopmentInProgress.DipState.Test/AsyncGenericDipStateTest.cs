using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class AsyncGenericDipStateTest
    {
        [TestMethod]
        public async Task Run_InitialiseStateWithEntryAction_StateInitialisedEntryActionExecuted()
        {
            // Arrange
            var state = new DipState<ContextText>(
                new ContextText() {Text = "Uninitialised"}, 1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncGenericEntryAction);

            // Act
            state = await state.RunAsync(DipStateStatus.Initialised);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
            Assert.AreEqual(((DipState<ContextText>)state).Context.Text, "Entry Action");
        }
    }
}
