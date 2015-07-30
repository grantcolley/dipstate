using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class AsyncStateEngineTest
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
        public async Task RunAsync_InitialiseState_StateInitialised()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            state = await state.ExecuteAsync(StateStatus.Initialised);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_InitialiseStateWithAsyncEntryAction_StateInitialisedAsyncEntryActionExecuted()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction);

            // Act
            state = await state.ExecuteAsync(StateStatus.Initialised);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_InitialiseStateWithExceptionInAsyncEntryAction_StateNotInitialisedExceptionThrown()
        {
            // Arrange 
            var mockAction = new Mock<Func<State, Task>>();

            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, mockAction.Object);

            mockAction.Setup(a => a(state))
                .Throws(
                    new InvalidOperationException(
                        "RunAsync_InitialiseStateWithExceptionInAsyncEntryAction_StateNotInitialisedExceptionThrown"));

            // Act
            try
            {
                state = await state.ExecuteAsync(StateStatus.Initialised);
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
            Assert.AreEqual(state.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task Run_InitialiseStateWithAsyncStatusAction_StateInitialisedAsyncStatusActionExecuted()
        {
            // Arrange
            var mockAction = new Mock<Func<State, Task>>();

            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Status, mockAction.Object);

            // Act
            state = await state.ExecuteAsync(StateStatus.Initialised);

            // Assert
            mockAction.Verify(a => a(state), Times.Once);
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_InitialiseStateWithAsyncStatusAction_StateInitialisedAsyncStatusActionExecuted()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncStatusAction);

            // Act
            state = await state.ExecuteAsync(StateStatus.Initialised);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_InitialiseStateWhenStateAlreadyInitialised_StateStatusUnchanged()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow", status: StateStatus.Initialised)
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction);

            // Act
            state = await state.ExecuteAsync(StateStatus.Initialised);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_InitialiseStateWithDependencyComplete_StateInitialised()
        {
            // Arrange
            var dependency = new State(0, "Dependent State", status: StateStatus.Completed);

            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddDependency(dependency);

            // Act
            state = await state.ExecuteAsync(StateStatus.Initialised);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_InitialiseStateWithDependencyNotComplete_StateNotInitialised()
        {
            // Arrange
            var dependency = new State(0, "Dependent State", status: StateStatus.InProgress);

            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddDependency(dependency);

            // Act
            state = await state.ExecuteAsync(StateStatus.Initialised);

            // Assert
            var logEntry = state.Log.First();
            Assert.IsTrue(logEntry.Message.StartsWith(String.Format("{0} is dependent on", state.Name)));
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task RunAsync_DependencyCompletedWhereDependantHasInitialiseDependantWhenCompleteSetTrue_InitialiseDependant()
        {
            // Arrange
            var dependency = new State(0, "Dependent State", status: StateStatus.InProgress);

            var state = new State(1, "Pricing Workflow")
                .AddDependency(dependency, true);

            // Act
            dependency = await dependency.ExecuteAsync(StateStatus.Completed);

            // Assert
            Assert.IsNotNull(dependency);
            Assert.AreEqual(dependency.Id, 0);
            Assert.AreEqual(dependency.Name, "Dependent State");
            Assert.AreEqual(dependency.Status, StateStatus.Completed);
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_DependencyCompletedWhereDependantHasInitialiseDependantWhenCompleteSetFalse_DependantNotInitialised()
        {
            // Arrange
            var dependency = new State(0, "Dependent State", status: StateStatus.InProgress)
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction)
                .AddDependency(dependency);

            // Act
            dependency = await dependency.ExecuteAsync(StateStatus.Completed);

            // Assert
            Assert.IsNotNull(dependency);
            Assert.AreEqual(dependency.Id, 0);
            Assert.AreEqual(dependency.Name, "Dependent State");
            Assert.AreEqual(dependency.Status, StateStatus.Completed);
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task RunAsync_AutoStateInitialisedAndCompleteWithoutTransition_AutoStateCompleted()
        {
            // Arrange
            var autoState = new State(1, "Close Case", type: StateType.Auto)
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            // Act
            autoState = await autoState.ExecuteAsync(StateStatus.Initialised);

            // Assert
            Assert.AreEqual(autoState.Id, 1);
            Assert.AreEqual(autoState.Name, "Close Case");
            Assert.AreEqual(autoState.Status, StateStatus.Completed);
            Assert.IsNull(autoState.Transition);
        }

        [TestMethod]
        public async Task RunAsync_AutoStateInitialisedWithTransition_AutoStateTransitionedToFinalReview()
        {
            // Arrange
            var finalState = new State(2, "Final Review")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var overrideState = new State(3, "Override")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var autoState = new State(1, "Override Decision", type: StateType.Auto)
                .AddTransition(finalState)
                .AddTransition(overrideState)
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncAutoEnrtyActionTransitionToFinalReview)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            // Act
            var state = await autoState.ExecuteAsync(StateStatus.Initialised);

            // Assert
            Assert.AreEqual(autoState.Id, 1);
            Assert.AreEqual(autoState.Name, "Override Decision");
            Assert.AreEqual(autoState.Status, StateStatus.Completed);
            Assert.AreEqual(autoState.Transition.Name, "Final Review");

            Assert.AreEqual(state.Id, 2);
            Assert.AreEqual(state.Name, "Final Review");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
            Assert.AreEqual(state.Antecedent.Name, "Override Decision");

            Assert.AreEqual(overrideState.Id, 3);
            Assert.AreEqual(overrideState.Name, "Override");
            Assert.AreEqual(overrideState.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task RunAsync_AutoStateInitialisedWithTransition_AutoStateTransitionedToOverride()
        {
            // Arrange
            var finalState = new State(2, "Final")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var overrideState = new State(3, "Override")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var autoState = new State(1, "Override Decision", type: StateType.Auto)
                .AddTransition(finalState)
                .AddTransition(overrideState)
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncAutoEnrtyActionTransitionToOverride)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            // Act
            var state = await autoState.ExecuteAsync(StateStatus.Initialised);

            // Assert
            Assert.AreEqual(autoState.Id, 1);
            Assert.AreEqual(autoState.Name, "Override Decision");
            Assert.AreEqual(autoState.Status, StateStatus.Completed);

            Assert.AreEqual(finalState.Id, 2);
            Assert.AreEqual(finalState.Name, "Final");
            Assert.AreEqual(finalState.Status, StateStatus.Uninitialised);

            Assert.AreEqual(state.Id, 3);
            Assert.AreEqual(state.Name, "Override");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_AggregateStateInitialised_AggregateStateInitialisedWithTwoSubStatesInitialised()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction)
                .AddSubState(new State(2, "Pricing A", initialiseWithParent: true)
                    .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction))
                .AddSubState(new State(3, "Pricing B")
                    .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction))
                .AddSubState(new State(4, "Pricing C", initialiseWithParent: true)
                    .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction));

            // Act
            state = await state.ExecuteAsync(StateStatus.Initialised);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);

            var pricingA = state.SubStates.First(pa => pa.Name.Equals("Pricing A"));
            Assert.AreEqual(pricingA.Id, 2);
            Assert.AreEqual(pricingA.Name, "Pricing A");
            Assert.AreEqual(pricingA.Status, StateStatus.Initialised);

            var pricingB = state.SubStates.First(pa => pa.Name.Equals("Pricing B"));
            Assert.AreEqual(pricingB.Id, 3);
            Assert.AreEqual(pricingB.Name, "Pricing B");
            Assert.AreEqual(pricingB.Status, StateStatus.Uninitialised);

            var pricingC = state.SubStates.First(pa => pa.Name.Equals("Pricing C"));
            Assert.AreEqual(pricingC.Id, 4);
            Assert.AreEqual(pricingC.Name, "Pricing C");
            Assert.AreEqual(pricingC.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_AggregateStateChangeSubStateToInProgress_SubSateAndParentSetToInProgress()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction)
                .AddSubState(new State(2, "Pricing A", initialiseWithParent: true)
                    .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction));

            state = await state.ExecuteAsync(StateStatus.Initialised);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            // Act
            statePricingA = await statePricingA.ExecuteAsync(StateStatus.InProgress);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.InProgress);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, StateStatus.InProgress);
        }

        [TestMethod]
        public async Task RunAsync_AggregateStateChangeOneSubStateToComplete_SubStateCompletedAndParentSetToInProgress()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction)
                .AddSubState(new State(2, "Pricing A", initialiseWithParent: true)
                    .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction))
                .AddSubState(new State(3, "Pricing B", initialiseWithParent: true)
                    .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction));

            state = await state.ExecuteAsync(StateStatus.Initialised);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            // Act
            statePricingA = await statePricingA.ExecuteAsync(StateStatus.Completed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.InProgress);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, StateStatus.Completed);

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));
            Assert.AreEqual(statePricingB.Id, 3);
            Assert.AreEqual(statePricingB.Name, "Pricing B");
            Assert.AreEqual(statePricingB.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public async Task RunAsync_AggregateStateChangeAllSubStatesToCompleted_SubStatesCompletedAndParentCompleted()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction)
                .AddSubState(new State(2, "Pricing A", initialiseWithParent: true)
                    .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction))
                .AddSubState(new State(3, "Pricing B", initialiseWithParent: true)
                    .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction));

            state = await state.ExecuteAsync(StateStatus.Initialised);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            // Act
            statePricingA = await statePricingA.ExecuteAsync(StateStatus.Completed);

            state = await statePricingB.ExecuteAsync(StateStatus.Completed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Completed);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, StateStatus.Completed);

            Assert.AreEqual(statePricingB.Id, 3);
            Assert.AreEqual(statePricingB.Name, "Pricing B");
            Assert.AreEqual(statePricingB.Status, StateStatus.Completed);
        }

        [TestMethod]
        public async Task RunAsync_UninitialiseState_StateReset()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            state = await state.ExecuteAsync(StateStatus.Initialised);

            // Act
            state = await state.ExecuteAsync(StateStatus.Uninitialised);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task RunAsync_FailStateWithoutTransition_StateFailedWithLogEntry()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            // Act
            state = await state.ExecuteAsync(StateStatus.Failed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Uninitialised);
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
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction)
                .AddSubState(new State(2, "Pricing A", initialiseWithParent: true)
                    .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction))
                .AddSubState(new State(3, "Pricing B", initialiseWithParent: true)
                    .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction));

            state = await state.ExecuteAsync(StateStatus.Initialised);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            // Act
            statePricingA = await statePricingA.ExecuteAsync(StateStatus.Failed);

            statePricingB = await statePricingB.ExecuteAsync(StateStatus.Failed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Uninitialised);
            Assert.IsFalse(state.IsDirty);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, StateStatus.Uninitialised);
            Assert.IsFalse(statePricingA.IsDirty);
            var statePricingALogEntry =
                statePricingA.Log.FirstOrDefault(
                    e =>
                        e.Message.Contains(String.Format("{0} has failed but is unable to transition",
                            statePricingA.Name)));
            Assert.IsNotNull(statePricingALogEntry);

            Assert.AreEqual(statePricingB.Id, 3);
            Assert.AreEqual(statePricingB.Name, "Pricing B");
            Assert.AreEqual(statePricingB.Status, StateStatus.Uninitialised);
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
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction)
                .AddSubState(new State(2, "Pricing A", initialiseWithParent: true)
                    .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction))
                .AddSubState(new State(3, "Pricing B", initialiseWithParent: true)
                    .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction));

            state = await state.ExecuteAsync(StateStatus.Initialised);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            // Act
            statePricingA = await statePricingA.ExecuteAsync(StateStatus.Failed);

            statePricingB = await statePricingB.ExecuteAsync(StateStatus.Completed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.InProgress);
            Assert.IsTrue(state.IsDirty);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, StateStatus.Uninitialised);
            Assert.IsFalse(statePricingA.IsDirty);
            var logEntry =
                statePricingA.Log.FirstOrDefault(
                    e =>
                        e.Message.Contains(String.Format("{0} has failed but is unable to transition",
                            statePricingA.Name)));
            Assert.IsNotNull(logEntry);

            Assert.AreEqual(statePricingB.Id, 3);
            Assert.AreEqual(statePricingB.Name, "Pricing B");
            Assert.AreEqual(statePricingB.Status, StateStatus.Completed);
            Assert.IsTrue(statePricingB.IsDirty);
        }

        [TestMethod]
        public async Task RunAsync_FailState_StateReset()
        {
            // Arrange
            var review = new State(2, "Review")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var state = new State(1, "Pricing Workflow")
                .AddTransition(review)
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            state = await state.ExecuteAsync(StateStatus.Initialised);
            review = await state.ExecuteAsync(review);
            review = await review.ExecuteAsync(StateStatus.InProgress);

            // Act
            review = await review.ExecuteAsync(StateStatus.Failed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Completed);
            Assert.IsNotNull(state.Transition);
            Assert.IsTrue(state.IsDirty);

            Assert.AreEqual(review.Id, 2);
            Assert.AreEqual(review.Name, "Review");
            Assert.AreEqual(review.Status, StateStatus.Uninitialised);
            Assert.IsNull(review.Antecedent);
            Assert.IsNull(review.Transition);
            Assert.IsFalse(review.IsDirty);
        }

        [TestMethod]
        public async Task RunAsync_FailStateTransitionToAnother_TransitionStateInitialisedFailedStateAndAntecedentsReset()
        {
            // Arrange
            var execution = new State(3, "Execution")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var review = new State(2, "Review")
                .AddTransition(execution)
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var state = new State(1, "Pricing Workflow")
                .AddTransition(review)
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            execution.AddTransition(state);

            state = await state.ExecuteAsync(StateStatus.Initialised);
            review = await state.ExecuteAsync(review);
            execution = await review.ExecuteAsync(execution);

            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Completed);
            Assert.AreEqual(state.Transition.Name, "Review");
            Assert.IsTrue(state.IsDirty);

            Assert.AreEqual(review.Id, 2);
            Assert.AreEqual(review.Name, "Review");
            Assert.AreEqual(review.Status, StateStatus.Completed);
            Assert.AreEqual(review.Antecedent.Name, "Pricing Workflow");
            Assert.AreEqual(review.Transition.Name, "Execution");
            Assert.IsTrue(review.IsDirty);

            Assert.AreEqual(execution.Id, 3);
            Assert.AreEqual(execution.Name, "Execution");
            Assert.AreEqual(execution.Status, StateStatus.Initialised);
            Assert.AreEqual(execution.Antecedent.Name, "Review");
            Assert.IsTrue(execution.IsDirty);

            // Act
            state = await execution.ExecuteAsync(StateStatus.Failed, state);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
            Assert.IsNull(state.Transition);
            Assert.IsTrue(state.IsDirty);

            Assert.AreEqual(review.Id, 2);
            Assert.AreEqual(review.Name, "Review");
            Assert.AreEqual(review.Status, StateStatus.Uninitialised);
            Assert.IsNull(review.Antecedent);
            Assert.IsNull(review.Transition);
            Assert.IsFalse(review.IsDirty);

            Assert.AreEqual(execution.Id, 3);
            Assert.AreEqual(execution.Name, "Execution");
            Assert.AreEqual(execution.Status, StateStatus.Uninitialised);
            Assert.IsNull(execution.Antecedent);
            Assert.IsNull(execution.Transition);
            Assert.IsFalse(execution.IsDirty);
        }

        [TestMethod]
        public async Task RunAsync_CompleteStateWithoutTransition_StateCompleted()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            // Act
            state = await state.ExecuteAsync(StateStatus.Completed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Completed);
            Assert.IsNull(state.Transition);
        }

        [TestMethod]
        public async Task RunAsync_CompleteStateWithTransition_StateCompletedAndTransitionStateInitialised()
        {
            // Arrange
            var review = new State(2, "Review")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var state = new State(1, "Pricing Workflow")
                .AddTransition(review)
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            state = await state.ExecuteAsync(StateStatus.Initialised);

            // Act
            review = await state.ExecuteAsync(review);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Completed);
            Assert.AreEqual(state.Transition.Name, "Review");

            Assert.AreEqual(review.Id, 2);
            Assert.AreEqual(review.Name, "Review");
            Assert.AreEqual(review.Status, StateStatus.Initialised);
            Assert.AreEqual(review.Antecedent.Name, "Pricing Workflow");
        }

        [TestMethod]
        public async Task RunAsync_CompleteSubStateAndTransitionParent_SubStateCompletedAndParentSompletedAndTransitionStateInitialised()
        {
            // Arrange
            var review = new State(3, "Review")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing", initialiseWithParent: true)
                    .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                    .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction))
                .AddTransition(review)
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            state = await state.ExecuteAsync(StateStatus.Initialised);

            var pricing = state.SubStates.First();

            // Act
            review = await pricing.ExecuteAsync(StateStatus.Completed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Completed);
            Assert.AreEqual(state.Transition.Name, "Review");

            Assert.AreEqual(pricing.Id, 2);
            Assert.AreEqual(pricing.Name, "Pricing");
            Assert.AreEqual(pricing.Status, StateStatus.Completed);

            Assert.AreEqual(review.Id, 3);
            Assert.AreEqual(review.Name, "Review");
            Assert.AreEqual(review.Status, StateStatus.Initialised);
            Assert.AreEqual(review.Antecedent.Name, "Pricing Workflow");
        }

        [TestMethod]
        [ExpectedException(typeof(StateException))]
        public async Task RunAsync_TransitionToStateNotInTransitionList_ExceptionThrownTransitionAborted()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            var review = new State(2, "Review")
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.AsyncEntryAction)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.AsyncExitAction);

            state = await state.ExecuteAsync(StateStatus.Initialised);

            State result = null;

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
            Assert.AreEqual(result.Status, StateStatus.Initialised);
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
