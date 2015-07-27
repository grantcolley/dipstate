using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class AsyncDipStateEngineTest
    {
        [TestInitialize]
        public void Initialise()
        {
            Debug.WriteLine(String.Format("AsyncDipStateEngineTest Start Act {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        [TestCleanup]
        public void Cleanup()
        {
            Debug.WriteLine(String.Format("AsyncDipStateEngineTest End Act {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));            
        }

        [TestMethod]
        public async Task RunAsync_InitialiseState_StateInitialised()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow");

            // Act
            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_InitialiseStateWithAsyncEntryAction_StateInitialisedAsyncEntryActionExecuted()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction);

            // Act
            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_InitialiseStateWithExceptionInAsyncEntryAction_StateNotInitialisedExceptionThrown()
        {
            // Arrange 
            var mockAction = new Mock<Func<DipState, Task>>();

            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, mockAction.Object);

            mockAction.Setup(a => a(state))
                .Throws(
                    new InvalidOperationException(
                        "RunAsync_InitialiseStateWithExceptionInAsyncEntryAction_StateNotInitialisedExceptionThrown"));

            // Act
            try
            {
                state = await state.ExecuteAsync(DipStateStatus.Initialised);
            }
            catch (InvalidOperationException ex)
            {
                if (
                    !ex.Message.Equals(
                        "RunAsync_InitialiseStateWithExceptionInAsyncEntryAction_StateNotInitialisedExceptionThrown"))
                {
                    throw;
                }
            }

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task Run_InitialiseStateWithAsyncStatusAction_StateInitialisedAsyncStatusActionExecuted()
        {
            // Arrange
            var mockAction = new Mock<Func<DipState, Task>>();

            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Status, mockAction.Object);

            // Act
            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            // Assert
            mockAction.Verify(a => a(state), Times.Once);
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_InitialiseStateWithAsyncStatusAction_StateInitialisedAsyncStatusActionExecuted()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncStatusAction);

            // Act
            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_InitialiseStateWhenStateAlreadyInitialised_StateStatusUnchanged()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow", status: DipStateStatus.Initialised)
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction);

            // Act
            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_InitialiseStateWithDependencyComplete_StateInitialised()
        {
            // Arrange
            var dependency = new DipState(0, "Dependent State", status: DipStateStatus.Completed);

            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddDependency(dependency);

            // Act
            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_InitialiseStateWithDependencyNotComplete_StateNotInitialised()
        {
            // Arrange
            var dependency = new DipState(0, "Dependent State", status: DipStateStatus.InProgress);

            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddDependency(dependency);

            // Act
            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            // Assert
            var logEntry = state.Log.First();
            Assert.IsTrue(logEntry.Message.StartsWith(String.Format("{0} is dependent on", state.Name)));
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task RunAsync_DependencyCompletedWhereDependantHasInitialiseDependantWhenCompleteSetTrue_InitialiseDependant()
        {
            // Arrange
            var dependency = new DipState(0, "Dependent State", status: DipStateStatus.InProgress);

            var state = new DipState(1, "Pricing Workflow")
                .AddDependency(dependency, true);

            // Act
            dependency = await dependency.ExecuteAsync(DipStateStatus.Completed);

            // Assert
            Assert.IsNotNull(dependency);
            Assert.AreEqual(dependency.Id, 0);
            Assert.AreEqual(dependency.Name, "Dependent State");
            Assert.AreEqual(dependency.Status, DipStateStatus.Completed);
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_DependencyCompletedWhereDependantHasInitialiseDependantWhenCompleteSetFalse_DependantNotInitialised()
        {
            // Arrange
            var dependency = new DipState(0, "Dependent State", status: DipStateStatus.InProgress)
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction)
                .AddDependency(dependency);

            // Act
            dependency = await dependency.ExecuteAsync(DipStateStatus.Completed);

            // Assert
            Assert.IsNotNull(dependency);
            Assert.AreEqual(dependency.Id, 0);
            Assert.AreEqual(dependency.Name, "Dependent State");
            Assert.AreEqual(dependency.Status, DipStateStatus.Completed);
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task RunAsync_AutoStateInitialisedAndCompleteWithoutTransition_AutoStateCompleted()
        {
            // Arrange
            var autoState = new DipState(1, "Close Case", type: DipStateType.Auto)
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            // Act
            autoState = await autoState.ExecuteAsync(DipStateStatus.Initialised);

            // Assert
            Assert.AreEqual(autoState.Id, 1);
            Assert.AreEqual(autoState.Name, "Close Case");
            Assert.AreEqual(autoState.Status, DipStateStatus.Completed);
            Assert.IsNull(autoState.Transition);
        }

        [TestMethod]
        public async Task RunAsync_AutoStateInitialisedWithTransition_AutoStateTransitionedToFinalReview()
        {
            // Arrange
            var finalState = new DipState(2, "Final Review")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var overrideState = new DipState(3, "Override")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var autoState = new DipState(1, "Override Decision", type: DipStateType.Auto)
                .AddTransition(finalState)
                .AddTransition(overrideState)
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncAutoEnrtyActionTransitionToFinalReview)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            // Act
            var state = await autoState.ExecuteAsync(DipStateStatus.Initialised);

            // Assert
            Assert.AreEqual(autoState.Id, 1);
            Assert.AreEqual(autoState.Name, "Override Decision");
            Assert.AreEqual(autoState.Status, DipStateStatus.Completed);
            Assert.AreEqual(autoState.Transition.Name, "Final Review");

            Assert.AreEqual(state.Id, 2);
            Assert.AreEqual(state.Name, "Final Review");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
            Assert.AreEqual(state.Antecedent.Name, "Override Decision");

            Assert.AreEqual(overrideState.Id, 3);
            Assert.AreEqual(overrideState.Name, "Override");
            Assert.AreEqual(overrideState.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task RunAsync_AutoStateInitialisedWithTransition_AutoStateTransitionedToOverride()
        {
            // Arrange
            var finalState = new DipState(2, "Final")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var overrideState = new DipState(3, "Override")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var autoState = new DipState(1, "Override Decision", type: DipStateType.Auto)
                .AddTransition(finalState)
                .AddTransition(overrideState)
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncAutoEnrtyActionTransitionToOverride)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            // Act
            var state = await autoState.ExecuteAsync(DipStateStatus.Initialised);

            // Assert
            Assert.AreEqual(autoState.Id, 1);
            Assert.AreEqual(autoState.Name, "Override Decision");
            Assert.AreEqual(autoState.Status, DipStateStatus.Completed);

            Assert.AreEqual(finalState.Id, 2);
            Assert.AreEqual(finalState.Name, "Final");
            Assert.AreEqual(finalState.Status, DipStateStatus.Uninitialised);

            Assert.AreEqual(state.Id, 3);
            Assert.AreEqual(state.Name, "Override");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_AggregateStateInitialised_AggregateStateInitialisedWithTwoSubStatesInitialised()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction)
                .AddSubState(new DipState(2, "Pricing A", initialiseWithParent: true)
                    .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction))
                .AddSubState(new DipState(3, "Pricing B")
                    .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction))
                .AddSubState(new DipState(4, "Pricing C", initialiseWithParent: true)
                    .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction));

            // Act
            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);

            var pricingA = state.SubStates.First(pa => pa.Name.Equals("Pricing A"));
            Assert.AreEqual(pricingA.Id, 2);
            Assert.AreEqual(pricingA.Name, "Pricing A");
            Assert.AreEqual(pricingA.Status, DipStateStatus.Initialised);

            var pricingB = state.SubStates.First(pa => pa.Name.Equals("Pricing B"));
            Assert.AreEqual(pricingB.Id, 3);
            Assert.AreEqual(pricingB.Name, "Pricing B");
            Assert.AreEqual(pricingB.Status, DipStateStatus.Uninitialised);

            var pricingC = state.SubStates.First(pa => pa.Name.Equals("Pricing C"));
            Assert.AreEqual(pricingC.Id, 4);
            Assert.AreEqual(pricingC.Name, "Pricing C");
            Assert.AreEqual(pricingC.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_AggregateStateChangeSubStateToInProgress_SubSateAndParentSetToInProgress()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction)
                .AddSubState(new DipState(2, "Pricing A", initialiseWithParent: true)
                    .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction));

            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            // Act
            statePricingA = await statePricingA.ExecuteAsync(DipStateStatus.InProgress);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.InProgress);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, DipStateStatus.InProgress);
        }

        [TestMethod]
        public async Task RunAsync_AggregateStateChangeOneSubStateToComplete_SubStateCompletedAndParentSetToInProgress()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction)
                .AddSubState(new DipState(2, "Pricing A", initialiseWithParent: true)
                    .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction))
                .AddSubState(new DipState(3, "Pricing B", initialiseWithParent: true)
                    .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction));

            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            // Act
            statePricingA = await statePricingA.ExecuteAsync(DipStateStatus.Completed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.InProgress);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, DipStateStatus.Completed);

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));
            Assert.AreEqual(statePricingB.Id, 3);
            Assert.AreEqual(statePricingB.Name, "Pricing B");
            Assert.AreEqual(statePricingB.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_AggregateStateChangeAllSubStatesToCompleted_SubStatesCompletedAndParentCompleted()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction)
                .AddSubState(new DipState(2, "Pricing A", initialiseWithParent: true)
                    .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction))
                .AddSubState(new DipState(3, "Pricing B", initialiseWithParent: true)
                    .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction));

            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            // Act
            statePricingA = await statePricingA.ExecuteAsync(DipStateStatus.Completed);

            state = await statePricingB.ExecuteAsync(DipStateStatus.Completed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Completed);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, DipStateStatus.Completed);

            Assert.AreEqual(statePricingB.Id, 3);
            Assert.AreEqual(statePricingB.Name, "Pricing B");
            Assert.AreEqual(statePricingB.Status, DipStateStatus.Completed);
        }

        [TestMethod]
        public async Task RunAsync_UninitialiseState_StateReset()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            // Act
            state = await state.ExecuteAsync(DipStateStatus.Uninitialised);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task RunAsync_FailStateWithoutTransition_StateFailedWithLogEntry()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            // Act
            state = await state.ExecuteAsync(DipStateStatus.Failed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Uninitialised);
            Assert.IsNull(state.Transition);
            Assert.IsFalse(state.IsDirty);

            var logEntry =
                state.Log.FirstOrDefault(
                    e => e.Message.Contains(String.Format("{0} has failed but is unable to transition", state.Name)));
            Assert.IsNotNull(logEntry);
        }

        [TestMethod]
        public async Task RunAsync_FailAllSubStatesFailsParent_SubStatesFailedAndParentFailed()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction)
                .AddSubState(new DipState(2, "Pricing A", initialiseWithParent: true)
                    .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction))
                .AddSubState(new DipState(3, "Pricing B", initialiseWithParent: true)
                    .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction));

            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            // Act
            statePricingA = await statePricingA.ExecuteAsync(DipStateStatus.Failed);

            statePricingB = await statePricingB.ExecuteAsync(DipStateStatus.Failed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Uninitialised);
            Assert.IsFalse(state.IsDirty);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, DipStateStatus.Uninitialised);
            Assert.IsFalse(statePricingA.IsDirty);
            var statePricingALogEntry =
                statePricingA.Log.FirstOrDefault(
                    e =>
                        e.Message.Contains(String.Format("{0} has failed but is unable to transition",
                            statePricingA.Name)));
            Assert.IsNotNull(statePricingALogEntry);

            Assert.AreEqual(statePricingB.Id, 3);
            Assert.AreEqual(statePricingB.Name, "Pricing B");
            Assert.AreEqual(statePricingB.Status, DipStateStatus.Uninitialised);
            Assert.IsFalse(statePricingB.IsDirty);
            var statePricingBLogEntry =
                statePricingB.Log.FirstOrDefault(
                    e =>
                        e.Message.Contains(String.Format("{0} has failed but is unable to transition",
                            statePricingB.Name)));
            Assert.IsNotNull(statePricingBLogEntry);
        }

        [TestMethod]
        public async Task RunAsync_FailOneSubStateAndCompleteAnother_OneSubStateFailedAndOneCompletedWithParentRemainingInProgress()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction)
                .AddSubState(new DipState(2, "Pricing A", initialiseWithParent: true)
                    .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction))
                .AddSubState(new DipState(3, "Pricing B", initialiseWithParent: true)
                    .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction));

            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            // Act
            statePricingA = await statePricingA.ExecuteAsync(DipStateStatus.Failed);

            statePricingB = await statePricingB.ExecuteAsync(DipStateStatus.Completed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.InProgress);
            Assert.IsTrue(state.IsDirty);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, DipStateStatus.Uninitialised);
            Assert.IsFalse(statePricingA.IsDirty);
            var logEntry =
                statePricingA.Log.FirstOrDefault(
                    e =>
                        e.Message.Contains(String.Format("{0} has failed but is unable to transition",
                            statePricingA.Name)));
            Assert.IsNotNull(logEntry);

            Assert.AreEqual(statePricingB.Id, 3);
            Assert.AreEqual(statePricingB.Name, "Pricing B");
            Assert.AreEqual(statePricingB.Status, DipStateStatus.Completed);
            Assert.IsTrue(statePricingB.IsDirty);
        }

        [TestMethod]
        public async Task RunAsync_FailState_StateReset()
        {
            // Arrange
            var review = new DipState(2, "Review")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var state = new DipState(1, "Pricing Workflow")
                .AddTransition(review)
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            state = await state.ExecuteAsync(DipStateStatus.Initialised);
            review = await state.ExecuteAsync(review);
            review = await review.ExecuteAsync(DipStateStatus.InProgress);

            // Act
            review = await review.ExecuteAsync(DipStateStatus.Failed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Completed);
            Assert.IsNotNull(state.Transition);
            Assert.IsTrue(state.IsDirty);

            Assert.AreEqual(review.Id, 2);
            Assert.AreEqual(review.Name, "Review");
            Assert.AreEqual(review.Status, DipStateStatus.Uninitialised);
            Assert.IsNull(review.Antecedent);
            Assert.IsNull(review.Transition);
            Assert.IsFalse(review.IsDirty);
        }

        [TestMethod]
        public async Task RunAsync_FailStateTransitionToAnother_TransitionStateInitialisedFailedStateAndAntecedentsReset()
        {
            // Arrange
            var execution = new DipState(3, "Execution")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var review = new DipState(2, "Review")
                .AddTransition(execution)
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var state = new DipState(1, "Pricing Workflow")
                .AddTransition(review)
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            execution.AddTransition(state);

            state = await state.ExecuteAsync(DipStateStatus.Initialised);
            review = await state.ExecuteAsync(review);
            execution = await review.ExecuteAsync(execution);

            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Completed);
            Assert.AreEqual(state.Transition.Name, "Review");
            Assert.IsTrue(state.IsDirty);

            Assert.AreEqual(review.Id, 2);
            Assert.AreEqual(review.Name, "Review");
            Assert.AreEqual(review.Status, DipStateStatus.Completed);
            Assert.AreEqual(review.Antecedent.Name, "Pricing Workflow");
            Assert.AreEqual(review.Transition.Name, "Execution");
            Assert.IsTrue(review.IsDirty);

            Assert.AreEqual(execution.Id, 3);
            Assert.AreEqual(execution.Name, "Execution");
            Assert.AreEqual(execution.Status, DipStateStatus.Initialised);
            Assert.AreEqual(execution.Antecedent.Name, "Review");
            Assert.IsTrue(execution.IsDirty);

            // Act
            state = await execution.ExecuteAsync(DipStateStatus.Failed, state);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
            Assert.IsNull(state.Transition);
            Assert.IsTrue(state.IsDirty);

            Assert.AreEqual(review.Id, 2);
            Assert.AreEqual(review.Name, "Review");
            Assert.AreEqual(review.Status, DipStateStatus.Uninitialised);
            Assert.IsNull(review.Antecedent);
            Assert.IsNull(review.Transition);
            Assert.IsFalse(review.IsDirty);

            Assert.AreEqual(execution.Id, 3);
            Assert.AreEqual(execution.Name, "Execution");
            Assert.AreEqual(execution.Status, DipStateStatus.Uninitialised);
            Assert.IsNull(execution.Antecedent);
            Assert.IsNull(execution.Transition);
            Assert.IsFalse(execution.IsDirty);
        }

        [TestMethod]
        public async Task RunAsync_CompleteStateWithoutTransition_StateCompleted()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            // Act
            state = await state.ExecuteAsync(DipStateStatus.Completed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Completed);
            Assert.IsNull(state.Transition);
        }

        [TestMethod]
        public async Task RunAsync_CompleteStateWithTransition_StateCompletedAndTransitionStateInitialised()
        {
            // Arrange
            var review = new DipState(2, "Review")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var state = new DipState(1, "Pricing Workflow")
                .AddTransition(review)
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            // Act
            review = await state.ExecuteAsync(review);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Completed);
            Assert.AreEqual(state.Transition.Name, "Review");

            Assert.AreEqual(review.Id, 2);
            Assert.AreEqual(review.Name, "Review");
            Assert.AreEqual(review.Status, DipStateStatus.Initialised);
            Assert.AreEqual(review.Antecedent.Name, "Pricing Workflow");
        }

        [TestMethod]
        public async Task RunAsync_CompleteSubStateAndTransitionParent_SubStateCompletedAndParentSompletedAndTransitionStateInitialised()
        {
            // Arrange
            var review = new DipState(3, "Review")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var state = new DipState(1, "Pricing Workflow")
                .AddSubState(new DipState(2, "Pricing", initialiseWithParent: true)
                    .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction))
                .AddTransition(review)
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            var pricing = state.SubStates.First();

            // Act
            review = await pricing.ExecuteAsync(DipStateStatus.Completed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Completed);
            Assert.AreEqual(state.Transition.Name, "Review");

            Assert.AreEqual(pricing.Id, 2);
            Assert.AreEqual(pricing.Name, "Pricing");
            Assert.AreEqual(pricing.Status, DipStateStatus.Completed);

            Assert.AreEqual(review.Id, 3);
            Assert.AreEqual(review.Name, "Review");
            Assert.AreEqual(review.Status, DipStateStatus.Initialised);
            Assert.AreEqual(review.Antecedent.Name, "Pricing Workflow");
        }

        [TestMethod]
        [ExpectedException(typeof(DipStateException))]
        public async Task RunAsync_TransitionToStateNotInTransitionList_ExceptionThrownTransitionAborted()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var review = new DipState(2, "Review")
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            state = await state.ExecuteAsync(DipStateStatus.Initialised);

            DipState result = null;

            // Act
            try
            {
                result = await state.ExecuteAsync(review);
            }
            catch (InvalidOperationException ex)
            {
                if (
                    !ex.Message.Equals(
                        "Run_InitialiseStateWithExceptionInEntryAction_ExceptionThrownStateNotInitialised"))
                {
                    throw;
                }
            }

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, 2);
            Assert.AreEqual(result.Name, "Pricing Workflow");
            Assert.AreEqual(result.Status, DipStateStatus.Initialised);
            Assert.IsNull(result.Transition);

            var logEntry =
                review.Log.FirstOrDefault(
                    e =>
                        e.Message.Contains(
                            String.Format(
                                "{0} cannot transition to {1} as it is not registered in the transition list.",
                                result.Name, review.Name)));

            Assert.IsNotNull(logEntry);
        }
    }
}
