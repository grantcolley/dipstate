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
        public async Task CanInitialiseAsync_NoPredicate_CanInitialiseAsyncReturnsTrueByDefault()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            var result = await state.CanInitialiseAsync();

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CanInitialiseAsync_PredicateReturnsTrue_CanInitialiseAsyncReturnsTrue()
        {
            // Arrange
            var mockPredicate = new Mock<Func<State, Task<bool>>>();
            mockPredicate.SetReturnsDefault(Task.FromResult(true));

            var state = new State(1, "Pricing Workflow")
                .AddCanInitialisePredicateAsync(mockPredicate.Object);

            // Act
            var result = await state.CanInitialiseAsync();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CanInitialiseAsync_PredicateReturnsFalse_CanInitialiseAsyncReturnsFalse()
        {
            // Arrange
            var mockPredicate = new Mock<Func<State, Task<bool>>>();
            mockPredicate.SetReturnsDefault(Task.FromResult(false));

            var state = new State(1, "Pricing Workflow")
                .AddCanInitialisePredicateAsync(mockPredicate.Object);

            // Act
            var result = await state.CanInitialiseAsync();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CanChangeStatusAsync_NoPredicate_CanChangeStatusAsyncReturnsTrueByDefault()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            var result = await state.CanChangeStatusAsync();

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CanChangeStatusAsync_PredicateReturnsTrue_CanChangeStatusAsyncReturnsTrue()
        {
            // Arrange
            var mockPredicate = new Mock<Func<State, Task<bool>>>();
            mockPredicate.SetReturnsDefault(Task.FromResult(true));

            var state = new State(1, "Pricing Workflow")
                .AddCanChangeStatusPredicateAsync(mockPredicate.Object);

            // Act
            var result = await state.CanChangeStatusAsync();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CanChangeStatusAsync_PredicateReturnsFalse_CanChangeStatusAsyncReturnsFalse()
        {
            // Arrange
            var mockPredicate = new Mock<Func<State, Task<bool>>>();
            mockPredicate.SetReturnsDefault(Task.FromResult(false));

            var state = new State(1, "Pricing Workflow")
                .AddCanChangeStatusPredicateAsync(mockPredicate.Object);

            // Act
            var result = await state.CanChangeStatusAsync();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CanCompleteAsync_NoPredicate_CanCompleteAsyncReturnsTrueByDefault()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            var result = await state.CanCompleteAsync();

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CanCompleteAsync_PredicateReturnsTrue_CanCompleteAsyncReturnsTrue()
        {
            // Arrange
            var mockPredicate = new Mock<Func<State, Task<bool>>>();
            mockPredicate.SetReturnsDefault(Task.FromResult(true));

            var state = new State(1, "Pricing Workflow")
                .AddCanCompletePredicateAsync(mockPredicate.Object);

            // Act
            var result = await state.CanCompleteAsync();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CanCompleteAsync_PredicateReturnsFalse_CanCompleteAsyncReturnsFalse()
        {
            // Arrange
            var mockPredicate = new Mock<Func<State, Task<bool>>>();
            mockPredicate.SetReturnsDefault(Task.FromResult(false));

            var state = new State(1, "Pricing Workflow")
                .AddCanCompletePredicateAsync(mockPredicate.Object);

            // Act
            var result = await state.CanCompleteAsync();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CanResetAsync_NoPredicate_CanResetAsyncReturnsTrueByDefault()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            var result = await state.CanResetAsync();

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CanResetAsync_PredicateReturnsTrue_CanResetAsyncReturnsTrue()
        {
            // Arrange
            var mockPredicate = new Mock<Func<State, Task<bool>>>();
            mockPredicate.SetReturnsDefault(Task.FromResult(true));

            var state = new State(1, "Pricing Workflow")
                .AddCanResetPredicateAsync(mockPredicate.Object);

            // Act
            var result = await state.CanResetAsync();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CanResetAsync_PredicateReturnsFalse_CanResetAsyncReturnsFalse()
        {
            // Arrange
            var mockPredicate = new Mock<Func<State, Task<bool>>>();
            mockPredicate.SetReturnsDefault(Task.FromResult(false));

            var state = new State(1, "Pricing Workflow")
                .AddCanResetPredicateAsync(mockPredicate.Object);

            // Act
            var result = await state.CanResetAsync();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AddActionAsync_AddEntryActionAsync_AsyncEntryActionAdded()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.OnEntry, TraceWrite);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.Actions.Count() == 1);
            Assert.IsTrue(
                state.Actions.Count(a => a.IsActionAsync
                                         && a.ActionType == StateActionType.OnEntry) == 1);
        }

        [TestMethod]
        public void AddActionAsync_AddExitActionAsync_AsyncExitActionAdded()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.OnExit, TraceWrite);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.Actions.Count() == 1);
            Assert.IsTrue(
                state.Actions.Count(a => a.IsActionAsync && a.ActionType == StateActionType.OnExit) == 1);
        }

        [TestMethod]
        public async Task ExecuteAsync_InitialiseState_StateInitialised()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
            Assert.IsTrue(state.IsDirty);

            var logEntry = state.Log.First();
            Assert.IsTrue(logEntry.Message.StartsWith(String.Format("{0} - {1}", state.Name, StateStatus.Initialised)));
        }

        [TestMethod]
        public async Task ExecuteAsync_InitialiseStateWhenStateAlreadyInitialised_StateStatusUnchanged()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow", status: StateStatus.Initialised);

            // Act
            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public async Task ExecuteAsync_InitialisedStateWithSubStates_StateInitialisedSubstateInitialised()
        {
            // Arrange
            var pricingWorkflow = new State(1, "Pricing Workflow");
            var dataCapture = new State(2, "Data Capture");
            var modelling = new State(3, "Modelling");

            pricingWorkflow
                .AddSubState(dataCapture, initialiseWithParent: true)
                .AddSubState(modelling);

            // Act
            pricingWorkflow = await pricingWorkflow.ExecuteAsync(StateExecutionType.Initialise);

            // Assert
            Assert.AreEqual(pricingWorkflow.Id, 1);
            Assert.AreEqual(pricingWorkflow.Name, "Pricing Workflow");
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Initialised);
            Assert.IsTrue(pricingWorkflow.IsDirty);

            var logEntry = pricingWorkflow.Log.First();
            Assert.IsTrue(logEntry.Message.StartsWith(String.Format("{0} - {1}", pricingWorkflow.Name, StateStatus.Initialised)));

            Assert.AreEqual(dataCapture.Id, 2);
            Assert.AreEqual(dataCapture.Name, "Data Capture");
            Assert.AreEqual(dataCapture.Status, StateStatus.Initialised);
            Assert.IsTrue(dataCapture.IsDirty);

            logEntry = dataCapture.Log.First();
            Assert.IsTrue(logEntry.Message.StartsWith(String.Format("{0} - {1}", dataCapture.Name, StateStatus.Initialised)));

            Assert.AreEqual(modelling.Id, 3);
            Assert.AreEqual(modelling.Name, "Modelling");
            Assert.AreEqual(modelling.Status, StateStatus.Uninitialised);
            Assert.IsFalse(modelling.IsDirty);
            Assert.IsFalse(modelling.Log.Any());
        }

        [TestMethod]
        public async Task ExecuteAsync_InitialiseStateWithCanInitialiseFalse_StateUninitialised()
        {
            // Arrange
            var mockPredicate = new Mock<Func<State, Task<bool>>>();
            mockPredicate.SetReturnsDefault(Task.FromResult(false));

            var state = new State(1, "Pricing Workflow")
                .AddCanInitialisePredicateAsync(mockPredicate.Object);

            // Act
            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            // Assert
            mockPredicate.Verify(a => a(state), Times.Once);
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Uninitialised);

            var logEntry = state.Log.First();
            Assert.IsTrue(logEntry.Message.Contains(String.Format("{0} is unable to initialise", state.Name)));
        }

        [TestMethod]
        public async Task ExecuteAsync_InitialiseStateWithCanInitialiseTrue_StateInitialised()
        {
            // Arrange
            var mockPredicate = new Mock<Func<State, Task<bool>>>();
            mockPredicate.SetReturnsDefault(Task.FromResult(true));

            var state = new State(1, "Pricing Workflow")
                .AddCanInitialisePredicateAsync(mockPredicate.Object);

            // Act
            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            // Assert
            mockPredicate.Verify(a => a(state), Times.Once);
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);

            var logEntry = state.Log.First();
            Assert.IsTrue(logEntry.Message.Contains(String.Format("{0} - {1}", state.Name, state.Status)));
        }

        [TestMethod]
        public async Task ExecuteAsync_InitialiseStateWithActions_StateInitialisedActionsExecuted()
        {
            // Arrange
            var mockEntryAction = new Mock<Func<State, Task>>();
            var mockStatusAction = new Mock<Func<State, Task>>();

            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.OnEntry, mockEntryAction.Object)
                .AddActionAsync(StateActionType.OnStatusChanged, mockStatusAction.Object);

            // Act
            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            // Assert
            mockEntryAction.Verify(a => a(state), Times.Once);
            mockStatusAction.Verify(a => a(state), Times.Once);
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public async Task ExecuteAsync_InitialiseStateWithExceptionInEntryAction_StateNotInitialisedExceptionThrown()
        {
            // Arrange 
            var mockAction = new Mock<Func<State, Task>>();

            var state = new State(1, "Pricing Workflow")
                .AddActionAsync(StateActionType.OnEntry, mockAction.Object);

            mockAction.Setup(a => a(state))
                .Throws(
                    new InvalidOperationException(
                        "Run_InitialiseStateWithExceptionInEntryAction_StateNotInitialisedExceptionThrown"));

            // Act
            try
            {
                state = await state.ExecuteAsync(StateExecutionType.Initialise);
            }
            catch (InvalidOperationException ex)
            {
                if (
                    !ex.Message.Equals(
                        "Run_InitialiseStateWithExceptionInEntryAction_StateNotInitialisedExceptionThrown"))
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
        public async Task ExecuteAsync_InitialiseStateWithDependencyComplete_StateInitialised()
        {
            // Arrange
            var dependency = new State(0, "Dependent State", status: StateStatus.Completed);

            var state = new State(1, "Pricing Workflow")
                .AddDependency(dependency);

            // Act
            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public async Task ExecuteAsync_InitialiseStateWithDependencyNotComplete_StateNotInitialised()
        {
            // Arrange
            var dependency = new State(0, "Dependent State", status: StateStatus.InProgress);

            var state = new State(1, "Pricing Workflow")
                .AddDependency(dependency);

            // Act
            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            // Assert
            var logEntry = state.Log.First();
            Assert.IsTrue(logEntry.Message.StartsWith(String.Format("{0} is dependent on", state.Name)));
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task ExecuteAsync_CompletedDependencyAndInitialiseDependent_DependencyCompletedDependantInitialised()
        {
            // Arrange
            var dependency = new State(0, "Dependency State", status: StateStatus.InProgress);

            var state = new State(1, "Pricing Workflow")
                .AddDependency(dependency, true);

            // Act
            dependency = await dependency.ExecuteAsync(StateExecutionType.Complete);

            // Assert
            Assert.IsNotNull(dependency);
            Assert.AreEqual(dependency.Id, 0);
            Assert.AreEqual(dependency.Name, "Dependency State");
            Assert.AreEqual(dependency.Status, StateStatus.Completed);
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public async Task ExecuteAsync_CompletedDependency_DependencyCompletedDependantInitialised()
        {
            // Arrange
            var dependency = new State(0, "Dependency State", status: StateStatus.InProgress);

            var state = new State(1, "Pricing Workflow")
                .AddDependency(dependency);

            // Act
            dependency = await dependency.ExecuteAsync(StateExecutionType.Complete);

            // Assert
            Assert.IsNotNull(dependency);
            Assert.AreEqual(dependency.Id, 0);
            Assert.AreEqual(dependency.Name, "Dependency State");
            Assert.AreEqual(dependency.Status, StateStatus.Completed);
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task ExecuteAsync_AutoStateInitialisedAndCompleteWithoutTransition_AutoStateCompleted()
        {
            // Arrange
            var mockEntryAction = new Mock<Func<State, Task>>();
            var mockStatusAction = new Mock<Func<State, Task>>();

            var autoState = new State(1, "Close Case", StateType.Auto)
                .AddActionAsync(StateActionType.OnEntry, mockEntryAction.Object)
                .AddActionAsync(StateActionType.OnStatusChanged, mockStatusAction.Object);

            // Act
            autoState = await autoState.ExecuteAsync(StateExecutionType.Initialise);

            // Assert
            mockEntryAction.Verify(a => a(autoState), Times.Once);
            mockStatusAction.Verify(a => a(autoState), Times.Exactly(2));
            Assert.AreEqual(autoState.Id, 1);
            Assert.AreEqual(autoState.Name, "Close Case");
            Assert.AreEqual(autoState.Status, StateStatus.Completed);
            Assert.IsNull(autoState.Transition);
        }

        [TestMethod]
        public async Task ExecuteAsync_AutoStateInitialisedWithTransition_AutoStateTransitionedToFinalReview()
        {
            // Arrange
            var mockEntryAction = new Mock<Func<State, Task>>();
            var mockStatusAction = new Mock<Func<State, Task>>();

            var finalState = new State(2, "Final Review")
                .AddActionAsync(StateActionType.OnEntry, mockEntryAction.Object);

            var overrideState = new State(3, "Override");

            var autoState = new State(1, "Override Decision", StateType.Auto)
                .AddTransition(finalState)
                .AddTransition(overrideState)
                .AddActionAsync(StateActionType.OnEntry, AsyncTestMethods.AsyncAutoEnrtyActionTransitionToFinalReview)
                .AddActionAsync(StateActionType.OnStatusChanged, mockStatusAction.Object);

            // Act
            var state = await autoState.ExecuteAsync(StateExecutionType.Initialise);

            // Assert
            mockStatusAction.Verify(a => a(autoState), Times.Exactly(2));
            Assert.AreEqual(autoState.Id, 1);
            Assert.AreEqual(autoState.Name, "Override Decision");
            Assert.AreEqual(autoState.Status, StateStatus.Completed);
            Assert.AreEqual(autoState.Transition.Name, "Final Review");

            mockEntryAction.Verify(a => a(state), Times.Once);
            Assert.AreEqual(state.Id, 2);
            Assert.AreEqual(state.Name, "Final Review");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
            Assert.AreEqual(state.Antecedent.Name, "Override Decision");

            Assert.AreEqual(overrideState.Id, 3);
            Assert.AreEqual(overrideState.Name, "Override");
            Assert.AreEqual(overrideState.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task ExecuteAsync_AutoStateInitialisedWithTransition_AutoStateTransitionedToOverride()
        {
            // Arrange
            var mockEntryAction = new Mock<Func<State, Task>>();
            var mockStatusAction = new Mock<Func<State, Task>>();

            var finalState = new State(2, "Final");

            var overrideState = new State(3, "Override")
                .AddActionAsync(StateActionType.OnEntry, mockEntryAction.Object);

            var autoState = new State(1, "Override Decision", type: StateType.Auto)
                .AddTransition(finalState)
                .AddTransition(overrideState)
                .AddActionAsync(StateActionType.OnEntry, AsyncTestMethods.AsyncAutoEnrtyActionTransitionToOverride)
                .AddActionAsync(StateActionType.OnStatusChanged, mockStatusAction.Object);

            // Act
            var state = await autoState.ExecuteAsync(StateExecutionType.Initialise);

            // Assert
            Assert.AreEqual(autoState.Id, 1);
            Assert.AreEqual(autoState.Name, "Override Decision");
            Assert.AreEqual(autoState.Status, StateStatus.Completed);

            Assert.AreEqual(finalState.Id, 2);
            Assert.AreEqual(finalState.Name, "Final");
            Assert.AreEqual(finalState.Status, StateStatus.Uninitialised);

            mockEntryAction.Verify(a => a(state), Times.Once);
            Assert.AreEqual(state.Id, 3);
            Assert.AreEqual(state.Name, "Override");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
            Assert.AreEqual(state.Antecedent.Name, "Override Decision");
        }

        [TestMethod]
        public async Task ExecuteAsync_InitialiseAggregateState_AggregateStateInitialisedWithTwoSubStatesInitialised()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A"), true)
                .AddSubState(new State(3, "Pricing B"))
                .AddSubState(new State(4, "Pricing C"), true);

            // Act
            state = await state.ExecuteAsync(StateExecutionType.Initialise);

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
        public async Task ExecuteAsync_ChangeStatusForSubStateToInProgress_SubSateAndParentSetToInProgress()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A"), true);

            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            // Act
            statePricingA = await statePricingA.ExecuteAsync(StateExecutionType.InProgress);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.InProgress);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, StateStatus.InProgress);
        }

        [TestMethod]
        public async Task ExecuteAsync_ChangeStatusSubStateToComplete_SubStateCompletedAndParentRemainsInProgress()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A"), true)
                .AddSubState(new State(3, "Pricing B"), true);

            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            // Act
            statePricingA = await statePricingA.ExecuteAsync(StateExecutionType.Complete);

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
        public async Task ExecuteAsync_CompleteAllSubStates_SubStatesCompletedAndParentCompleted()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A"), true)
                .AddSubState(new State(3, "Pricing B"), true);

            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            // Act
            statePricingA = await statePricingA.ExecuteAsync(StateExecutionType.Complete);

            state = await statePricingB.ExecuteAsync(StateExecutionType.Complete);

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
        public async Task ExecuteAsync_CompleteAllSubStatesWithCompletion_SubStatesCompletedAndParentCompleted()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A"), true, true)
                .AddSubState(new State(3, "Pricing B"), true, false)
                .AddSubState(new State(4, "Pricing C"), true, true);

            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            var statePricingC = state.SubStates.Single(s => s.Name.Equals("Pricing C"));

            // Act
            statePricingA = await statePricingA.ExecuteAsync(StateExecutionType.Complete);

            statePricingB = await statePricingB.ExecuteAsync(StateExecutionType.Initialise);

            state = await statePricingC.ExecuteAsync(StateExecutionType.Complete);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Completed);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, StateStatus.Completed);

            Assert.AreEqual(statePricingB.Id, 3);
            Assert.AreEqual(statePricingB.Name, "Pricing B");
            Assert.AreEqual(statePricingB.Status, StateStatus.Initialised);

            Assert.AreEqual(statePricingC.Id, 4);
            Assert.AreEqual(statePricingC.Name, "Pricing C");
            Assert.AreEqual(statePricingC.Status, StateStatus.Completed);
        }

        [TestMethod]
        public async Task ExecuteAsync_TransitionState_StateCompleted()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            state = await state.ExecuteAsync(StateExecutionType.Complete);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Completed);
            Assert.IsNull(state.Transition);
        }

        [TestMethod]
        [ExpectedException(typeof(StateException))]
        public async Task ExecuteAsync_TransitionStateToStateNotOnTransitionList_StateExceptionRaised()
        {
            // Arrange
            var review = new State(2, "Review");
            var state = new State(1, "Pricing Workflow");

            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            // Act
            review = await state.ExecuteAsync(review);
        }

        [TestMethod]
        public async Task ExecuteAsync_TransitionStateToStateNotOnTransitionList_StateFailedToCompleteOrTransition()
        {
            // Arrange
            var review = new State(2, "Review");
            var state = new State(1, "Pricing Workflow");

            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            try
            {
                // Act
                review = await state.ExecuteAsync(review);
            }
            catch (StateException ex)
            {
                var logEntry =
                    state.Log.FirstOrDefault(
                        e =>
                            e.Message.Contains(
                                String.Format(
                                    "{0} cannot transition to {1} as it is not registered in the transition list.",
                                    state.Name, review.Name)));

                Assert.IsNotNull(logEntry);
                Assert.IsTrue(ex.Message.Contains(String.Format("{0} failed to transition. Check logs.", state.Name)));
            }

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
            Assert.IsNull(state.Transition);
        }

        [TestMethod]
        public async Task ExecuteAsync_TransitionState_StateCompletedAndTransitionStateInitialised()
        {
            // Arrange
            var review = new State(2, "Review");
            var state = new State(1, "Pricing Workflow")
                .AddTransition(review);

            state = await state.ExecuteAsync(StateExecutionType.Initialise);

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
        public async Task ExecuteAsync_TransitionSubStateAndTransitionParent_SubStateCompletedAndParentCompletedAndTransitionStateInitialised()
        {
            // Arrange
            var review = new State(3, "Review");
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing"), true)
                .AddTransition(review, true);

            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            var pricing = state.SubStates.First();

            // Act
            review = await pricing.ExecuteAsync(StateExecutionType.Complete);

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
        public async Task ExecuteAsync_TransitionStateCanCompleteFalse_StateNotTransitioned()
        {
            // Arrange
            var mockPredicate = new Mock<Func<State, Task<bool>>>();
            mockPredicate.SetReturnsDefault(Task.FromResult(false));

            var transitionState = new State(0, "Transition State");
            var state = new State(1, "State")
                .AddCanCompletePredicateAsync(mockPredicate.Object)
                .AddTransition(transitionState);

            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            // Act
            var result = await state.ExecuteAsync(transitionState);

            // Assert
            Assert.AreEqual(result.Id, 1);
            Assert.AreEqual(result.Name, "State");
            Assert.AreEqual(result.Status, StateStatus.Initialised);
            Assert.IsNull(result.Transition);

            Assert.AreEqual(transitionState.Id, 0);
            Assert.AreEqual(transitionState.Name, "Transition State");
            Assert.AreEqual(transitionState.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task ExecuteAsync_TransitionStateToAntecedentWithoutCompleteTrue_AntecedentResetAndThenInitialised()
        {
            // Arrange
            var pricingWorkflow = new State(1, "Pricing Workflow");
            var review = new State(2, "Review");
            var outcome = new State(3, "Outcome");

            pricingWorkflow.AddTransition(review);
            review.AddTransition(outcome);
            outcome.AddTransition(review);

            await pricingWorkflow.ExecuteAsync(StateExecutionType.Initialise);
            review = await pricingWorkflow.ExecuteAsync(review);
            outcome = await review.ExecuteAsync(outcome);

            // Act
            await outcome.ExecuteAsync(review, true);

            // Assert
            Assert.AreEqual(pricingWorkflow.Id, 1);
            Assert.AreEqual(pricingWorkflow.Name, "Pricing Workflow");
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Completed);
            Assert.AreEqual(pricingWorkflow.Transition, review);
            Assert.IsTrue(pricingWorkflow.IsDirty);

            Assert.AreEqual(review.Id, 2);
            Assert.AreEqual(review.Name, "Review");
            Assert.AreEqual(review.Status, StateStatus.Initialised);
            Assert.IsNull(review.Transition);
            Assert.AreEqual(review.Antecedent, pricingWorkflow);
            Assert.IsTrue(review.IsDirty);

            Assert.AreEqual(outcome.Id, 3);
            Assert.AreEqual(outcome.Name, "Outcome");
            Assert.AreEqual(outcome.Status, StateStatus.Uninitialised);
            Assert.IsNull(outcome.Transition);
            Assert.IsNull(outcome.Antecedent);
            Assert.IsFalse(outcome.IsDirty);
        }

        /// <summary>
        /// NOTE: This demonstrates potential unexpected results when transitioning backwards i.e. a previous state in the workflow.
        /// If the transitionWithoutComplete argument is not explicitly set to true (the default is false), then the current state being 
        /// transitioned from will complete (and if it has any dependant states they will be initialised) and the target transition state 
        /// will not be reset, which means all the states in between will also not be reset.
        /// If the intention is to "revert" or "regress" a state backwards in a workflow then explicitly set transitionWithoutComplete to true.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_TransitionStateToAntecedentWithoutCompleteFalse_AntecedentResetAndThenInitialised()
        {
            // Arrange
            var pricingWorkflow = new State(1, "Pricing Workflow");
            var review = new State(2, "Review");
            var outcome = new State(3, "Outcome");

            pricingWorkflow.AddTransition(review);
            review.AddTransition(outcome);
            outcome.AddTransition(review);

            await pricingWorkflow.ExecuteAsync(StateExecutionType.Initialise);
            review = await pricingWorkflow.ExecuteAsync(review);
            outcome = await review.ExecuteAsync(outcome);

            // Act
            await outcome.ExecuteAsync(review);

            // Assert
            Assert.AreEqual(pricingWorkflow.Id, 1);
            Assert.AreEqual(pricingWorkflow.Name, "Pricing Workflow");
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Completed);
            Assert.AreEqual(pricingWorkflow.Transition, review);
            Assert.IsTrue(pricingWorkflow.IsDirty);

            Assert.AreEqual(review.Id, 2);
            Assert.AreEqual(review.Name, "Review");
            Assert.AreEqual(review.Status, StateStatus.Initialised);
            Assert.AreEqual(review.Transition, outcome);
            Assert.AreEqual(review.Antecedent, outcome);
            Assert.IsTrue(review.IsDirty);

            Assert.AreEqual(outcome.Id, 3);
            Assert.AreEqual(outcome.Name, "Outcome");
            Assert.AreEqual(outcome.Status, StateStatus.Completed);
            Assert.AreEqual(outcome.Transition, review);
            Assert.AreEqual(outcome.Antecedent, review);
            Assert.IsTrue(outcome.IsDirty);
        }

        [TestMethod]
        public async Task ExecuteAsync_ChangeStatusCanChangeStatusReturnsTrue_StatusChanged()
        {
            // Arrange
            var mockPredicate = new Mock<Func<State, Task<bool>>>();
            mockPredicate.SetReturnsDefault(Task.FromResult(true));

            var state = new State(1, "Pricing Workflow")
                .AddCanChangeStatusPredicateAsync(mockPredicate.Object);

            await state.ExecuteAsync(StateExecutionType.Initialise);

            // Act
            await state.ExecuteAsync(StateExecutionType.InProgress);

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.InProgress);
        }

        [TestMethod]
        public async Task ExecuteAsync_ChangeStatusCanChangeStatusReturnsFalse_StatusUnchanged()
        {
            // Arrange
            var mockPredicate = new Mock<Func<State, Task<bool>>>();
            mockPredicate.SetReturnsDefault(Task.FromResult(false));

            var state = new State(1, "Pricing Workflow")
                .AddCanChangeStatusPredicateAsync(mockPredicate.Object);

            await state.ExecuteAsync(StateExecutionType.Initialise);

            // Act
            await state.ExecuteAsync(StateExecutionType.InProgress);

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public async Task ExecuteAsync_ResetStateCanResetFalse_StateNotReset()
        {
            // Arrange
            var mockPredicate = new Mock<Func<State, Task<bool>>>();
            mockPredicate.SetReturnsDefault(Task.FromResult(false));

            var outcome = new State(3, "Outcome");

            var review = new State(2, "Review")
                .AddTransition(outcome)
                .AddCanResetPredicateAsync(mockPredicate.Object);

            var pricingWorkflow = new State(1, "Pricing Workflow")
                .AddTransition(review);

            await pricingWorkflow.ExecuteAsync(StateExecutionType.Initialise);
            review = await pricingWorkflow.ExecuteAsync(review);
            outcome = await review.ExecuteAsync(outcome);

            // Act
            await review.ExecuteAsync(StateExecutionType.Reset);

            // Assert
            Assert.AreEqual(pricingWorkflow.Id, 1);
            Assert.AreEqual(pricingWorkflow.Name, "Pricing Workflow");
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Completed);
            Assert.AreEqual(pricingWorkflow.Transition, review);
            Assert.IsTrue(pricingWorkflow.IsDirty);

            Assert.AreEqual(review.Id, 2);
            Assert.AreEqual(review.Name, "Review");
            Assert.AreEqual(review.Status, StateStatus.Completed);
            Assert.AreEqual(review.Transition, outcome);
            Assert.AreEqual(review.Antecedent, pricingWorkflow);
            Assert.IsTrue(review.IsDirty);

            Assert.AreEqual(outcome.Id, 3);
            Assert.AreEqual(outcome.Name, "Outcome");
            Assert.AreEqual(outcome.Status, StateStatus.Initialised);
            Assert.AreEqual(outcome.Antecedent, review);
            Assert.IsTrue(outcome.IsDirty);
        }

        [TestMethod]
        public async Task ExecuteAsync_ResetState_StateResetAndTransitions()
        {
            // Arrange
            var outcome = new State(3, "Outcome");

            var review = new State(2, "Review")
                .AddTransition(outcome);

            var pricingWorkflow = new State(1, "Pricing Workflow")
                .AddTransition(review);

            await pricingWorkflow.ExecuteAsync(StateExecutionType.Initialise);
            review = await pricingWorkflow.ExecuteAsync(review);
            outcome = await review.ExecuteAsync(outcome);

            // Act
            await review.ExecuteAsync(StateExecutionType.Reset);

            // Assert
            Assert.AreEqual(pricingWorkflow.Id, 1);
            Assert.AreEqual(pricingWorkflow.Name, "Pricing Workflow");
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Completed);
            Assert.AreEqual(pricingWorkflow.Transition, review);
            Assert.IsTrue(pricingWorkflow.IsDirty);

            Assert.AreEqual(review.Id, 2);
            Assert.AreEqual(review.Name, "Review");
            Assert.AreEqual(review.Status, StateStatus.Uninitialised);
            Assert.IsNull(review.Transition);
            Assert.IsNull(review.Antecedent);
            Assert.IsFalse(review.IsDirty);

            Assert.AreEqual(outcome.Id, 3);
            Assert.AreEqual(outcome.Name, "Outcome");
            Assert.AreEqual(outcome.Status, StateStatus.Uninitialised);
            Assert.IsNull(outcome.Transition);
            Assert.IsNull(outcome.Antecedent);
            Assert.IsFalse(outcome.IsDirty);
        }

        [TestMethod]
        public async Task ExecuteAsync_ResetState_StateResetAndDependenciesReset()
        {
            // Arrange
            var outcome = new State(3, "Outcome");

            var review = new State(2, "Review")
                .AddDependant(outcome, true);

            var pricingWorkflow = new State(1, "Pricing Workflow")
                .AddTransition(review);

            await pricingWorkflow.ExecuteAsync(StateExecutionType.Initialise);
            review = await pricingWorkflow.ExecuteAsync(review);
            await review.ExecuteAsync(StateExecutionType.Complete);

            // Act
            await review.ExecuteAsync(StateExecutionType.Reset);

            // Assert
            Assert.AreEqual(pricingWorkflow.Id, 1);
            Assert.AreEqual(pricingWorkflow.Name, "Pricing Workflow");
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Completed);
            Assert.AreEqual(pricingWorkflow.Transition, review);
            Assert.IsTrue(pricingWorkflow.IsDirty);

            Assert.AreEqual(review.Id, 2);
            Assert.AreEqual(review.Name, "Review");
            Assert.AreEqual(review.Status, StateStatus.Uninitialised);
            Assert.IsNull(review.Transition);
            Assert.IsNull(review.Antecedent);
            Assert.IsFalse(review.IsDirty);

            Assert.AreEqual(outcome.Id, 3);
            Assert.AreEqual(outcome.Name, "Outcome");
            Assert.AreEqual(outcome.Status, StateStatus.Uninitialised);
            Assert.IsNull(outcome.Transition);
            Assert.IsNull(outcome.Antecedent);
            Assert.IsFalse(outcome.IsDirty);
        }

        [TestMethod]
        public async Task ExecuteAsync_ResetAllSubStatesResetsParent_ParentAndSubStatesReset()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A"), true)
                .AddSubState(new State(3, "Pricing B"), true);

            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            // Act
            statePricingA = await statePricingA.ExecuteAsync(StateExecutionType.Reset);

            statePricingB = await statePricingB.ExecuteAsync(StateExecutionType.Reset);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Uninitialised);
            Assert.IsTrue(state.IsDirty);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, StateStatus.Uninitialised);
            Assert.IsFalse(statePricingA.IsDirty);

            Assert.AreEqual(statePricingB.Id, 3);
            Assert.AreEqual(statePricingB.Name, "Pricing B");
            Assert.AreEqual(statePricingB.Status, StateStatus.Uninitialised);
            Assert.IsFalse(statePricingB.IsDirty);
        }

        [TestMethod]
        public async Task ExecuteAsync_ResetParentResetsSubStates_ParentAndSubStatesReset()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A"), true)
                .AddSubState(new State(3, "Pricing B"), true)
                .AddSubState(new State(4, "Pricing C"));

            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            var statePricingC = state.SubStates.Single(s => s.Name.Equals("Pricing C"));

            // Act
            await state.ExecuteAsync(StateExecutionType.Reset);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Uninitialised);
            Assert.IsFalse(state.IsDirty);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, StateStatus.Uninitialised);
            Assert.IsFalse(statePricingA.IsDirty);

            Assert.AreEqual(statePricingC.Id, 4);
            Assert.AreEqual(statePricingC.Name, "Pricing C");
            Assert.AreEqual(statePricingC.Status, StateStatus.Uninitialised);
            Assert.IsFalse(statePricingC.IsDirty);
        }

        [TestMethod]
        public async Task ExecuteAsync_ResetParentSubStateCanResetFalse_ParentAndSubStateNotReset()
        {
            // Arrange
            var mockPredicate = new Mock<Func<State, Task<bool>>>();
            mockPredicate.SetReturnsDefault(Task.FromResult(false));

            var statePricingA = new State(2, "Pricing A")
                .AddCanResetPredicateAsync(mockPredicate.Object);

            var statePricingB = new State(3, "Pricing B");

            var state = new State(1, "Pricing Workflow")
                .AddSubState(statePricingA, true)
                .AddSubState(statePricingB, true);

            state = await state.ExecuteAsync(StateExecutionType.Initialise);

            // Act
            await state.ExecuteAsync(StateExecutionType.Reset);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
            Assert.IsTrue(state.IsDirty);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, StateStatus.Initialised);
            Assert.IsTrue(statePricingA.IsDirty);

            Assert.AreEqual(statePricingB.Id, 3);
            Assert.AreEqual(statePricingB.Name, "Pricing B");
            Assert.AreEqual(statePricingB.Status, StateStatus.Uninitialised);
            Assert.IsFalse(statePricingB.IsDirty);
        }

        private static async Task TraceWrite(State state)
        {
            await Task.Run(async delegate
            {
                var random = new Random();
                int milliseconds = random.Next(3) * 1000;
                await Task.Delay(milliseconds);
            });

            Trace.WriteLine(String.Format("{0} {1} - {2}", state.Id, state.Name, state.Status));
            foreach (var logEntry in state.Log)
            {
                Trace.WriteLine(String.Format("     {0}", logEntry.Message));
            }
        }
    }
}
