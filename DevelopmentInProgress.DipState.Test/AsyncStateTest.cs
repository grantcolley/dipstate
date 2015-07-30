using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class AsyncStateTest
    {
        [TestInitialize]
        public void Initialise()
        {
            Debug.WriteLine(String.Format("AsyncStateEngineTest Start Act {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        [TestCleanup]
        public void Cleanup()
        {
            Debug.WriteLine(String.Format("AsyncStateEngineTest End Act {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        [TestMethod]
        public async Task Reset_StateIsReset()
        {
            // Arrange
            var mockAction = new Mock<Func<State,Task>>();

            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Reset, mockAction.Object);

            state = state.Execute(StateStatus.Initialised);

            // Act
            await state.ResetAsync(true);

            // Assert
            mockAction.Verify(a => a(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Uninitialised);
            Assert.IsFalse(state.IsDirty);
            Assert.IsTrue(state.Log.Count.Equals(0));
        }

        [TestMethod]
        public async Task Status_ChangeStatusAsync_StatusChangedAndIsDirtySetTrueAndLogEntry()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddAction(StateActionType.Entry,
                    s =>
                        Debug.WriteLine("{0}   Synchronous Entry action for {1}",
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), s.Name));
            
            // Act
            state = await state.ExecuteAsync(StateStatus.Initialised);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
            Assert.IsTrue(state.IsDirty);

            Assert.IsTrue(
                state.Log.Any(
                    l => l.Message.StartsWith(String.Format("{0} - {1}", state.Name, StateStatus.Initialised))));
        }

        [TestMethod]
        public async Task CanCompleteAsync_NoCanCompletePredicate_CanCompleteAsyncReturnsTrue()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            var canComplete = await state.CanCompleteAsync();

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(canComplete);
        }

        [TestMethod]
        public async Task CanComplete_PredicateReturnsTrue_CanCompleteReturnsTrue()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow", canCompleteAsync: AsyncTestMethods.AsyncCanCompleteTrue);

            // Act
            var canComplete = await state.CanCompleteAsync();

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(canComplete);
        }

        [TestMethod]
        public async Task CanComplete_PredicateReturnsFalse_CanCompleteReturnsFalse()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow", canCompleteAsync: AsyncTestMethods.AsyncCanCompleteFalse);

            // Act
            var canComplete = await state.CanCompleteAsync();

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsFalse(canComplete);
        }

        [TestMethod]
        public void AddAsyncAction_AddAsyncEntryAction_AsyncEntryActionAdded()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.Actions.Count().Equals(1));
            Assert.IsTrue(state.Actions.Count(a => a.ActionType.Equals(StateActionType.Entry)).Equals(1));
        }

        [TestMethod]
        public void AddAsyncAction_AddAsyncExitAction_AsyncExitActionAdded()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.Actions.Count().Equals(1));
            Assert.IsTrue(state.Actions.Count(a => a.ActionType.Equals(StateActionType.Exit)).Equals(1));
        }
    }
}
