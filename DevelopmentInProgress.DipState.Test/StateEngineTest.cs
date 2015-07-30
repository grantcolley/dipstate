using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class StateEngineTest
    {
        [TestMethod]
        public void Run_InitialiseState_StateInitialised()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            state = state.Execute(StateStatus.Initialised);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public void Run_InitialiseStateWithEntryAction_StateInitialisedEntryActionExecuted()
        {
            // Arrange
            var mockAction = new Mock<Action<State>>();

            var state = new State(1, "Pricing Workflow")
                .AddAction(StateActionType.Entry, mockAction.Object);

            // Act
            state = state.Execute(StateStatus.Initialised);

            // Assert
            mockAction.Verify(a => a(state), Times.Once);
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public void Run_InitialiseStateWithExceptionInEntryAction_StateNotInitialisedExceptionThrown()
        {
            // Arrange 
            var mockAction = new Mock<Action<State>>();

            var state = new State(1, "Pricing Workflow")
                .AddAction(StateActionType.Entry, mockAction.Object);

            mockAction.Setup(a => a(state))
                .Throws(
                    new InvalidOperationException(
                        "Run_InitialiseStateWithExceptionInEntryAction_StateNotInitialisedExceptionThrown"));

            // Act
            try
            {
                state = state.Execute(StateStatus.Initialised);
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
        public void Run_InitialiseStateWithStatusAction_StateInitialisedStatusActionExecuted()
        {
            // Arrange
            var mockAction = new Mock<Action<State>>();

            var state = new State(1, "Pricing Workflow")
                .AddAction(StateActionType.Status, mockAction.Object);

            // Act
            state = state.Execute(StateStatus.Initialised);

            // Assert
            mockAction.Verify(a => a(state), Times.Once);
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public void Run_InitialiseStateWhenStateAlreadyInitialised_StateStatusUnchanged()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow", status: StateStatus.Initialised);

            // Act
            state = state.Execute(StateStatus.Initialised);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public void Run_InitialiseStateWithDependencyComplete_StateInitialised()
        {
            // Arrange
            var dependency = new State(0, "Dependent State", status: StateStatus.Completed);

            var state = new State(1, "Pricing Workflow")
                .AddDependency(dependency);

            // Act
            state = state.Execute(StateStatus.Initialised);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public void Run_InitialiseStateWithDependencyNotComplete_StateNotInitialised()
        {
            // Arrange
            var dependency = new State(0, "Dependent State", status: StateStatus.InProgress);

            var state = new State(1, "Pricing Workflow")
                .AddDependency(dependency);

            // Act
            state = state.Execute(StateStatus.Initialised);

            // Assert
            var logEntry = state.Log.First();
            Assert.IsTrue(logEntry.Message.StartsWith(String.Format("{0} is dependent on", state.Name)));
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public void Run_DependencyCompletedWhereDependantHasInitialiseDependantWhenCompleteSetTrue_InitialiseDependant()
        {
            // Arrange
            var dependency = new State(0, "Dependent State", status: StateStatus.InProgress);

            var state = new State(1, "Pricing Workflow")
                .AddDependency(dependency, true);

            // Act
            dependency = dependency.Execute(StateStatus.Completed);

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
        public void Run_DependencyCompletedWhereDependantHasInitialiseDependantWhenCompleteSetFalse_DependantNotInitialised()
        {
            // Arrange
            var dependency = new State(0, "Dependent State", status: StateStatus.InProgress);

            var state = new State(1, "Pricing Workflow")
                .AddDependency(dependency);

            // Act
            dependency = dependency.Execute(StateStatus.Completed);

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
        public void Run_AutoStateInitialisedAndCompleteWithoutTransition_AutoStateCompleted()
        {
            // Arrange
            var autoState = new State(1, "Close Case", type: StateType.Auto);

            // Act
            autoState = autoState.Execute(StateStatus.Initialised);

            // Assert
            Assert.AreEqual(autoState.Id, 1);
            Assert.AreEqual(autoState.Name, "Close Case");
            Assert.AreEqual(autoState.Status, StateStatus.Completed);
            Assert.IsNull(autoState.Transition);
        }

        [TestMethod]
        public void Run_AutoStateInitialisedWithTransition_AutoStateTransitionedToFinalReview()
        {
            // Arrange
            var finalState = new State(2, "Final Review");
            var overrideState = new State(3, "Override");

            var autoState = new State(1, "Override Decision", type: StateType.Auto)
                .AddTransition(finalState)
                .AddTransition(overrideState)
                .AddAction(StateActionType.Entry, s => { s.Transition = finalState; });

            // Act
            var state = autoState.Execute(StateStatus.Initialised);

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
        public void Run_AutoStateInitialisedWithTransition_AutoStateTransitionedToOverride()
        {
            // Arrange
            var finalState = new State(2, "Final");
            var overrideState = new State(3, "Override");

            var autoState = new State(1, "Override Decision", type: StateType.Auto)
                .AddTransition(finalState)
                .AddTransition(overrideState)
                .AddAction(StateActionType.Entry, s => { s.Transition = overrideState; });

            // Act
            var state = autoState.Execute(StateStatus.Initialised);

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
        public void Run_AggregateStateInitialised_AggregateStateInitialisedWithTwoSubStatesInitialised()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A", initialiseWithParent: true))
                .AddSubState(new State(3, "Pricing B"))
                .AddSubState(new State(4, "Pricing C", initialiseWithParent: true));

            // Act
            state = state.Execute(StateStatus.Initialised);

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
        public void Run_AggregateStateChangeSubStateToInProgress_SubSateAndParentSetToInProgress()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A", initialiseWithParent: true));

            state = state.Execute(StateStatus.Initialised);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            // Act
            statePricingA = statePricingA.Execute(StateStatus.InProgress);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.InProgress);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, StateStatus.InProgress);
        }

        [TestMethod]
        public void Run_AggregateStateChangeOneSubStateToComplete_SubStateCompletedAndParentSetToInProgress()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A", initialiseWithParent: true))
                .AddSubState(new State(3, "Pricing B", initialiseWithParent: true));

            state = state.Execute(StateStatus.Initialised);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            // Act
            statePricingA = statePricingA.Execute(StateStatus.Completed);

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
        public void Run_AggregateStateChangeAllSubStatesToCompleted_SubStatesCompletedAndParentCompleted()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A", initialiseWithParent: true))
                .AddSubState(new State(3, "Pricing B", initialiseWithParent: true));

            state = state.Execute(StateStatus.Initialised);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            // Act
            statePricingA = statePricingA.Execute(StateStatus.Completed);

            state = statePricingB.Execute(StateStatus.Completed);

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
        public void Run_UninitialiseState_StateReset()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            state = state.Execute(StateStatus.Initialised);

            // Act
            state = state.Execute(StateStatus.Uninitialised);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public void Run_FailStateWithoutTransition_StateFailedWithLogEntry()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            state = state.Execute(StateStatus.Failed);

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
        public void Run_FailAllSubStatesFailsParent_SubStatesFailedAndParentFailed()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A", initialiseWithParent: true))
                .AddSubState(new State(3, "Pricing B", initialiseWithParent: true));

            state = state.Execute(StateStatus.Initialised);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            // Act
            statePricingA = statePricingA.Execute(StateStatus.Failed);

            statePricingB = statePricingB.Execute(StateStatus.Failed);

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
        public void Run_FailOneSubStateAndCompleteAnother_OneSubStateFailedAndOneCompletedWithParentRemainingInProgress()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A", initialiseWithParent: true))
                .AddSubState(new State(3, "Pricing B", initialiseWithParent: true));

            state = state.Execute(StateStatus.Initialised);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            // Act
            statePricingA = statePricingA.Execute(StateStatus.Failed);

            statePricingB = statePricingB.Execute(StateStatus.Completed);

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
        public void Run_FailState_StateReset()
        {
            // Arrange
            var review = new State(2, "Review");
            var state = new State(1, "Pricing Workflow")
                .AddTransition(review);

            state = state.Execute(StateStatus.Initialised);
            review = state.Execute(review);
            review = review.Execute(StateStatus.InProgress);

            // Act
            review = review.Execute(StateStatus.Failed);

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
        public void Run_FailStateTransitionToAnother_TransitionStateInitialisedFailedStateAndAntecedentsReset()
        {
            // Arrange
            var execution = new State(3, "Execution");
            var review = new State(2, "Review")
                .AddTransition(execution);
            var state = new State(1, "Pricing Workflow")
                .AddTransition(review);

            execution.AddTransition(state);

            state = state.Execute(StateStatus.Initialised);
            review = state.Execute(review);
            execution = review.Execute(execution);

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
            state = execution.Execute(StateStatus.Failed, state);

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
        public void Run_CompleteStateWithoutTransition_StateCompleted()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            state = state.Execute(StateStatus.Completed);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Completed);
            Assert.IsNull(state.Transition);
        }

        [TestMethod]
        public void Run_CompleteStateWithTransition_StateCompletedAndTransitionStateInitialised()
        {
            // Arrange
            var review = new State(2, "Review");
            var state = new State(1, "Pricing Workflow")
                .AddTransition(review);

            state = state.Execute(StateStatus.Initialised);

            // Act
            review = state.Execute(review);

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
        public void Run_CompleteSubStateAndTransitionParent_SubStateCompletedAndParentSompletedAndTransitionStateInitialised()
        {
            // Arrange
            var review = new State(3, "Review");
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing", initialiseWithParent: true))
                .AddTransition(review);

            state = state.Execute(StateStatus.Initialised);

            var pricing = state.SubStates.First();

            // Act
            review = pricing.Execute(StateStatus.Completed);

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
        public void Run_TransitionToStateNotInTransitionList_ExceptionThrownTransitionAborted()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");
            var review = new State(2, "Review");

            state = state.Execute(StateStatus.Initialised);

            State result = null;

            // Act
            try
            {
                result = state.Execute(review);
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