using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class StateTest
    {
        [TestMethod]
        public void CanInitialise_NoPredicate_CanInitialiseReturnsTrueByDefault()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            var result = state.CanInitialise();

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanInitialise_PredicateReturnsTrue_CanInitialiseReturnsTrue()
        {
            // Arrange
            var mockPredicate = new Mock<Predicate<State>>();
            mockPredicate.SetReturnsDefault(true);

            var state = new State(1, "Pricing Workflow")
                .AddCanInitialisePredicate(mockPredicate.Object);

            // Act
            var result = state.CanInitialise();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanInitialise_PredicateReturnsFalse_CanInitialiseReturnsFalse()
        {
            // Arrange
            var mockPredicate = new Mock<Predicate<State>>();
            mockPredicate.SetReturnsDefault(false);

            var state = new State(1, "Pricing Workflow")
                .AddCanInitialisePredicate(mockPredicate.Object);

            // Act
            var result = state.CanInitialise();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanChangeStatus_NoPredicate_CanChangeStatusReturnsTrueByDefault()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            var result = state.CanChangeStatus();

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanChangeStatus_PredicateReturnsTrue_CanChangeStatusReturnsTrue()
        {
            // Arrange
            var mockPredicate = new Mock<Predicate<State>>();
            mockPredicate.SetReturnsDefault(true);

            var state = new State(1, "Pricing Workflow")
                .AddCanChangeStatusPredicate(mockPredicate.Object);

            // Act
            var result = state.CanChangeStatus();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanChangeStatus_PredicateReturnsFalse_CanChangeStatusReturnsFalse()
        {
            // Arrange
            var mockPredicate = new Mock<Predicate<State>>();
            mockPredicate.SetReturnsDefault(false);

            var state = new State(1, "Pricing Workflow")
                .AddCanChangeStatusPredicate(mockPredicate.Object);

            // Act
            var result = state.CanChangeStatus();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanComplete_NoPredicate_CanCompleteReturnsTrueByDefault()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            var result = state.CanComplete();

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanComplete_PredicateReturnsTrue_CanCompleteReturnsTrue()
        {
            // Arrange
            var mockPredicate = new Mock<Predicate<State>>();
            mockPredicate.SetReturnsDefault(true);

            var state = new State(1, "Pricing Workflow")
                .AddCanCompletePredicate(mockPredicate.Object);

            // Act
            var result = state.CanComplete();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanComplete_PredicateReturnsFalse_CanCompleteReturnsFalse()
        {
            // Arrange
            var mockPredicate = new Mock<Predicate<State>>();
            mockPredicate.SetReturnsDefault(false); 

            var state = new State(1, "Pricing Workflow")
                .AddCanCompletePredicate(mockPredicate.Object);

            // Act
            var result = state.CanComplete();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanReset_NoPredicate_CanResetReturnsTrueByDefault()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            var result = state.CanReset();

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanReset_PredicateReturnsTrue_CanResetReturnsTrue()
        {
            // Arrange
            var mockPredicate = new Mock<Predicate<State>>();
            mockPredicate.SetReturnsDefault(true);

            var state = new State(1, "Pricing Workflow")
                .AddCanResetPredicate(mockPredicate.Object);

            // Act
            var result = state.CanReset();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanReset_PredicateReturnsFalse_CanResetReturnsFalse()
        {
            // Arrange
            var mockPredicate = new Mock<Predicate<State>>();
            mockPredicate.SetReturnsDefault(false);

            var state = new State(1, "Pricing Workflow")
                .AddCanResetPredicate(mockPredicate.Object);

            // Act
            var result = state.CanReset();

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AddSubState_SubStateAddedAndSubStateParentSet()
        {
            // Arrange
            var subState = new State(2, "Data Capture");

            var state = new State(1, "Pricing Workflow")
                .AddSubState(subState);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.SubStates.Count.Equals(1));
            Assert.AreEqual(state.SubStates.First(), subState);
            Assert.AreEqual(subState.Id, 2);
            Assert.AreEqual(subState.Name, "Data Capture");
            Assert.AreEqual(subState.Parent, state);
        }

        [TestMethod]
        public void AddTransition_TransitionAdded()
        {
            // Arrange
            var review = new State(2, "Review");

            var state = new State(1, "Pricing Workflow")
                .AddTransition(review);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.Transitions.Count.Equals(1));
            Assert.AreEqual(state.Transitions.First(), review);
            Assert.AreEqual(review.Id, 2);
            Assert.AreEqual(review.Name, "Review");
        }

        [TestMethod]
        public void AddAction_AddEntryAction_EntryActionAdded()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddAction(StateActionType.OnEntry, s => Debug.Write(s.Name));

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.Actions.Count().Equals(1));
            Assert.IsTrue(state.Actions.Count(a => !a.IsActionAsync 
                && a.ActionType.Equals(StateActionType.OnEntry)).Equals(1));
        }

        [TestMethod]
        public void AddAction_AddExitAction_ExitActionAdded()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddAction(StateActionType.OnExit, s => Debug.Write(s.Name));

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.Actions.Count().Equals(1));
            Assert.IsTrue(state.Actions.Count(a => !a.IsActionAsync
                && a.ActionType.Equals(StateActionType.OnExit)).Equals(1));
        }

        [TestMethod]
        public void AddDependency_AddDependantAndDependency_DependentDependencyReferenced()
        {
            // Arrange
            var a = new State(1, "A");

            var b = new State(2, "B")
                .AddDependency(a);

            a.AddDependant(b);

            // Assert
            Assert.AreEqual(a.Id, 1);
            Assert.AreEqual(a.Name, "A");
            Assert.IsTrue(a.Dependants.Count.Equals(1));
            Assert.AreEqual(a.Dependants.First().Dependant, b);

            Assert.AreEqual(b.Id, 2);
            Assert.AreEqual(b.Name, "B");
            Assert.IsTrue(b.Dependencies.Count.Equals(1));
            Assert.AreEqual(b.Dependencies.First(), a);
        }

        [TestMethod]
        public void AddDependant_AddDependantAndDependency_DependentDependencyReferenced()
        {
            // Arrange
            var a = new State(1, "A");

            var b = new State(2, "B");

            a.AddDependant(b);

            b.AddDependency(a);

            // Assert
            Assert.AreEqual(a.Id, 1);
            Assert.AreEqual(a.Name, "A");
            Assert.IsTrue(a.Dependants.Count.Equals(1));
            Assert.AreEqual(a.Dependants.First().Dependant, b);

            Assert.AreEqual(b.Id, 2);
            Assert.AreEqual(b.Name, "B");
            Assert.IsTrue(b.Dependencies.Count.Equals(1));
            Assert.AreEqual(b.Dependencies.First(), a);
        }

        [TestMethod]
        public void AddDependant_WithInitialiseDependantWhenCompleteSetTrue_DependentAndDependcyReferencesSet()
        {
            // Arrange
            var dependant = new State(0, "Dependant");

            var state = new State(1, "Pricing Workflow")
                .AddDependant(dependant, true);

            // Assert
            Assert.AreEqual(dependant.Id, 0);
            Assert.AreEqual(dependant.Name, "Dependant");
            Assert.IsTrue(dependant.Dependencies.Count.Equals(1));
            Assert.AreEqual(dependant.Dependencies.First(), state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.Dependants.Count.Equals(1));
            Assert.AreEqual(state.Dependants.First().Dependant, dependant);
        }

        [TestMethod]
        public void AddDependant_WithInitialiseDependantWhenCompleteSetFalse_DependentAndDependcyReferencesSet()
        {
            // Arrange
            var dependant = new State(2, "Dependant");

            var state = new State(1, "Pricing Workflow")
                .AddDependant(dependant);

            // Assert
            Assert.AreEqual(dependant.Id, 2);
            Assert.AreEqual(dependant.Name, "Dependant");
            Assert.IsTrue(dependant.Dependencies.Count.Equals(1));
            Assert.AreEqual(dependant.Dependencies.First(), state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.Dependants.Count.Equals(1));
            Assert.AreEqual(state.Dependants.First().Dependant, dependant);
        }

        [TestMethod]
        public void AddDependency_WithInitialiseWhenDependencyCompletedSetTrue_DependentAndDependcyReferencesSet()
        {
            // Arrange
            var dependency = new State(0, "Dependency");

            var state = new State(1, "Pricing Workflow")
                .AddDependency(dependency, true);

            // Assert
            Assert.AreEqual(dependency.Id, 0);
            Assert.AreEqual(dependency.Name, "Dependency");
            Assert.IsTrue(dependency.Dependants.Count.Equals(1));
            Assert.AreEqual(dependency.Dependants.First().Dependant, state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.Dependencies.Count.Equals(1));
            Assert.AreEqual(state.Dependencies.First(), dependency);
        }

        [TestMethod]
        public void AddDependency_WithInitialiseWhenDependencyCompletedSetFalse_DependentAndDependcyReferencesSet()
        {
            // Arrange
            var dependency = new State(0, "Dependency");

            var state = new State(1, "Pricing Workflow")
                .AddDependency(dependency);

            // Assert
            Assert.AreEqual(dependency.Id, 0);
            Assert.AreEqual(dependency.Name, "Dependency");
            Assert.IsTrue(dependency.Dependants.Count.Equals(1));
            Assert.AreEqual(dependency.Dependants.First().Dependant, state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.Dependencies.Count.Equals(1));
            Assert.AreEqual(state.Dependencies.First(), dependency);
        }

        [TestMethod]
        public void GetRoot_FindRootStateFromNestedSubState_ReturnRoot()
        {
            // Arrange
            var subState1 = new State(2, "Sub State 1");
            var subState2 = new State(3, "Sub State 2");
            var subState3 = new State(6, "Sub State 3");

            var subState2_1 = new State(4, "Sub State 2_1");
            var subState2_2 = new State(5, "Sub State 2_2");

            subState2
                .AddSubState(subState2_1)
                .AddSubState(subState2_2);

            var rootState = new State(1, "Root")
                .AddSubState(subState1)
                .AddSubState(subState2)
                .AddSubState(subState3);

            // Act
            var root = subState2_2.GetRoot();

            // Assert
            Assert.IsTrue(root.Name.Equals("Root"));
        }

        [TestMethod]
        public void GetRoot_FindRootStateFromRootState_ReturnRoot()
        {
            // Arrange
            var subState1 = new State(2, "Sub State 1");
            var SubState2 = new State(3, "Sub State 2");
            var SubState3 = new State(6, "Sub State 3");

            var SubState2_1 = new State(4, "Sub State 2_1");
            var SubState2_2 = new State(5, "Sub State 2_2");

            SubState2
                .AddSubState(SubState2_1)
                .AddSubState(SubState2_2);

            var rootState = new State(1, "Root")
                .AddSubState(subState1)
                .AddSubState(SubState2)
                .AddSubState(SubState3);

            // Act
            var root = rootState.GetRoot();

            // Assert
            Assert.IsTrue(root.Name.Equals("Root"));
        }

        [TestMethod]
        public void FlattenHierarchy_FlattenWorkflowHierarchyFromNestedSubState_ReturnFLattenedList()
        {
            // Arrange
            var subState1 = new State(2, "Sub State 1");
            var SubState2 = new State(3, "Sub State 2");
            var SubState3 = new State(6, "Sub State 3");

            var SubState2_1 = new State(4, "Sub State 2_1");
            var SubState2_2 = new State(5, "Sub State 2_2");

            SubState2
                .AddSubState(SubState2_1)
                .AddSubState(SubState2_2);

            var rootState = new State(1, "Root")
                .AddSubState(subState1)
                .AddSubState(SubState2)
                .AddSubState(SubState3);

            // Act
            var states = SubState2.Flatten();

            // Assert
            Assert.IsTrue(states.Count.Equals(6));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Root")));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Sub State 1")));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Sub State 2")));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Sub State 2_1")));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Sub State 2_2")));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Sub State 3")));
        }

        [TestMethod]
        public void FlattenHierarchy_FlattenWorkflowHierarchyFromRoot_ReturnFLattenedList()
        {
            // Arrange
            var subState1 = new State(2, "Sub State 1");
            var SubState2 = new State(3, "Sub State 2");
            var SubState3 = new State(6, "Sub State 3");

            var SubState2_1 = new State(4, "Sub State 2_1");
            var SubState2_2 = new State(5, "Sub State 2_2");

            SubState2
                .AddSubState(SubState2_1)
                .AddSubState(SubState2_2);

            var rootState = new State(1, "Root")
                .AddSubState(subState1)
                .AddSubState(SubState2)
                .AddSubState(SubState3);

            // Act
            var states = rootState.Flatten();

            // Assert
            Assert.IsTrue(states.Count.Equals(6));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Root")));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Sub State 1")));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Sub State 2")));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Sub State 2_1")));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Sub State 2_2")));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Sub State 3")));
        }

        [TestMethod]
        public void FlattenHierarchy_FlattenWorkflowHierarchyFromLowestLevel_ReturnFLattenedList()
        {
            // Arrange
            var subState1 = new State(2, "Sub State 1");
            var SubState2 = new State(3, "Sub State 2");
            var SubState3 = new State(6, "Sub State 3");

            var SubState2_1 = new State(4, "Sub State 2_1");
            var SubState2_2 = new State(5, "Sub State 2_2");

            SubState2
                .AddSubState(SubState2_1)
                .AddSubState(SubState2_2);

            var rootState = new State(1, "Root")
                .AddSubState(subState1)
                .AddSubState(SubState2)
                .AddSubState(SubState3);

            // Act
            var states = SubState2_2.Flatten();

            // Assert
            Assert.IsTrue(states.Count.Equals(6));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Root")));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Sub State 1")));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Sub State 2")));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Sub State 2_1")));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Sub State 2_2")));
            Assert.IsNotNull(states.FirstOrDefault(s => s.Name.Equals("Sub State 3")));
        }

        [TestMethod]
        public void Execute_InitialiseState_StateInitialised()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            state = state.Execute(StateExecutionType.Initialise);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
            Assert.IsTrue(state.IsDirty);

            var logEntry = state.Log.First();
            Assert.IsTrue(logEntry.Message.StartsWith(String.Format("{0} - {1}", state.Name, StateStatus.Initialised)));
        }

        [TestMethod]
        public void Execute_InitialiseStateWhenStateAlreadyInitialised_StateStatusUnchanged()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow", status: StateStatus.Initialised);

            // Act
            state = state.Execute(StateExecutionType.Initialise);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public void Execute_InitialisedStateWithSubStates_StateInitialisedSubstateInitialised()
        {
            // Arrange
            var pricingWorkflow = new State(1, "Pricing Workflow");
            var dataCapture = new State(2, "Data Capture");
            var modelling = new State(3, "Modelling");

            pricingWorkflow
                .AddSubState(dataCapture, initialiseWithParent: true)
                .AddSubState(modelling);

            // Act
            pricingWorkflow = pricingWorkflow.Execute(StateExecutionType.Initialise);

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
        public void Execute_InitialiseStateWithCanInitialiseFalse_StateUninitialised()
        {
            // Arrange
            var mockPredicate = new Mock<Predicate<State>>();
            mockPredicate.SetReturnsDefault(false);

            var state = new State(1, "Pricing Workflow")
                .AddCanInitialisePredicate(mockPredicate.Object);

            // Act
            state = state.Execute(StateExecutionType.Initialise);

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
        public void Execute_InitialiseStateWithCanInitialiseTrue_StateInitialised()
        {
            // Arrange
            var mockPredicate = new Mock<Predicate<State>>();
            mockPredicate.SetReturnsDefault(true);

            var state = new State(1, "Pricing Workflow")
                .AddCanInitialisePredicate(mockPredicate.Object);

            // Act
            state = state.Execute(StateExecutionType.Initialise);

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
        public void Execute_InitialiseStateWithActions_StateInitialisedActionsExecuted()
        {
            // Arrange
            var mockEntryAction = new Mock<Action<State>>();
            var mockStatusAction = new Mock<Action<State>>();

            var state = new State(1, "Pricing Workflow")
                .AddAction(StateActionType.OnEntry, mockEntryAction.Object)
                .AddAction(StateActionType.OnStatusChanged, mockStatusAction.Object);

            // Act
            state = state.Execute(StateExecutionType.Initialise);

            // Assert
            mockEntryAction.Verify(a => a(state), Times.Once);
            mockStatusAction.Verify(a => a(state), Times.Once);
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public void Execute_InitialiseStateWithExceptionInEntryAction_StateNotInitialisedExceptionThrown()
        {
            // Arrange 
            var mockAction = new Mock<Action<State>>();

            var state = new State(1, "Pricing Workflow")
                .AddAction(StateActionType.OnEntry, mockAction.Object);

            mockAction.Setup(a => a(state))
                .Throws(
                    new InvalidOperationException(
                        "Run_InitialiseStateWithExceptionInEntryAction_StateNotInitialisedExceptionThrown"));

            // Act
            try
            {
                state = state.Execute(StateExecutionType.Initialise);
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
        public void Execute_InitialiseStateWithDependencyComplete_StateInitialised()
        {
            // Arrange
            var dependency = new State(0, "Dependent State", status: StateStatus.Completed);

            var state = new State(1, "Pricing Workflow")
                .AddDependency(dependency);

            // Act
            state = state.Execute(StateExecutionType.Initialise);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public void Execute_InitialiseStateWithDependencyNotComplete_StateNotInitialised()
        {
            // Arrange
            var dependency = new State(0, "Dependent State", status: StateStatus.InProgress);

            var state = new State(1, "Pricing Workflow")
                .AddDependency(dependency);

            // Act
            state = state.Execute(StateExecutionType.Initialise);

            // Assert
            var logEntry = state.Log.First();
            Assert.IsTrue(logEntry.Message.StartsWith(String.Format("{0} is dependent on", state.Name)));
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public void Execute_CompletedDependencyAndInitialiseDependent_DependencyCompletedDependantInitialised()
        {
            // Arrange
            var dependency = new State(0, "Dependency State", status: StateStatus.InProgress);

            var state = new State(1, "Pricing Workflow")
                .AddDependency(dependency, true);

            // Act
            dependency = dependency.Execute(StateExecutionType.Complete);

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
        public void Execute_CompletedDependency_DependencyCompletedDependantInitialised()
        {
            // Arrange
            var dependency = new State(0, "Dependency State", status: StateStatus.InProgress);

            var state = new State(1, "Pricing Workflow")
                .AddDependency(dependency);

            // Act
            dependency = dependency.Execute(StateExecutionType.Complete);

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
        public void Execute_AutoStateInitialisedAndCompleteWithoutTransition_AutoStateCompleted()
        {
            // Arrange
            var mockEntryAction = new Mock<Action<State>>();
            var mockStatusAction = new Mock<Action<State>>();

            var autoState = new State(1, "Close Case", StateType.Auto)
                .AddAction(StateActionType.OnEntry, mockEntryAction.Object)
                .AddAction(StateActionType.OnStatusChanged, mockStatusAction.Object);

            // Act
            autoState = autoState.Execute(StateExecutionType.Initialise);

            // Assert
            mockEntryAction.Verify(a => a(autoState), Times.Once);
            mockStatusAction.Verify(a => a(autoState), Times.Exactly(2));
            Assert.AreEqual(autoState.Id, 1);
            Assert.AreEqual(autoState.Name, "Close Case");
            Assert.AreEqual(autoState.Status, StateStatus.Completed);
            Assert.IsNull(autoState.Transition);
        }

        [TestMethod]
        public void Execute_AutoStateInitialisedWithTransition_AutoStateTransitionedToFinalReview()
        {
            // Arrange
            var mockEntryAction = new Mock<Action<State>>();
            var mockStatusAction = new Mock<Action<State>>();

            var finalState = new State(2, "Final Review")
                .AddAction(StateActionType.OnEntry, mockEntryAction.Object);

            var overrideState = new State(3, "Override");

            var autoState = new State(1, "Override Decision", StateType.Auto)
                .AddTransition(finalState)
                .AddTransition(overrideState)
                .AddAction(StateActionType.OnEntry, s => { s.Transition = finalState; })
                .AddAction(StateActionType.OnStatusChanged, mockStatusAction.Object);

            // Act
            var state = autoState.Execute(StateExecutionType.Initialise);

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
        public void Execute_AutoStateInitialisedWithTransition_AutoStateTransitionedToOverride()
        {
            // Arrange
            var mockEntryAction = new Mock<Action<State>>();
            var mockStatusAction = new Mock<Action<State>>();

            var finalState = new State(2, "Final");

            var overrideState = new State(3, "Override")
                .AddAction(StateActionType.OnEntry, mockEntryAction.Object);

            var autoState = new State(1, "Override Decision", type: StateType.Auto)
                .AddTransition(finalState)
                .AddTransition(overrideState)
                .AddAction(StateActionType.OnEntry, s => { s.Transition = overrideState; })
                .AddAction(StateActionType.OnStatusChanged, mockStatusAction.Object);

            // Act
            var state = autoState.Execute(StateExecutionType.Initialise);

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
        public void Execute_InitialiseAggregateState_AggregateStateInitialisedWithTwoSubStatesInitialised()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A"), true)
                .AddSubState(new State(3, "Pricing B"))
                .AddSubState(new State(4, "Pricing C"), true);

            // Act
            state = state.Execute(StateExecutionType.Initialise);

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
        public void Execute_ChangeStatusForSubStateToInProgress_SubSateAndParentSetToInProgress()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A"), true);

            state = state.Execute(StateExecutionType.Initialise);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            // Act
            statePricingA = statePricingA.Execute(StateExecutionType.InProgress);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.InProgress);

            Assert.AreEqual(statePricingA.Id, 2);
            Assert.AreEqual(statePricingA.Name, "Pricing A");
            Assert.AreEqual(statePricingA.Status, StateStatus.InProgress);
        }

        [TestMethod]
        public void Execute_ChangeStatusSubStateToComplete_SubStateCompletedAndParentRemainsInProgress()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A"), true)
                .AddSubState(new State(3, "Pricing B"), true);

            state = state.Execute(StateExecutionType.Initialise);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            // Act
            statePricingA = statePricingA.Execute(StateExecutionType.Complete);

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
        public void Execute_CompleteAllSubStates_SubStatesCompletedAndParentCompleted()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A"), true)
                .AddSubState(new State(3, "Pricing B"), true);

            state = state.Execute(StateExecutionType.Initialise);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            // Act
            statePricingA = statePricingA.Execute(StateExecutionType.Complete);

            state = statePricingB.Execute(StateExecutionType.Complete);

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
        public void Execute_CompleteAllSubStatesWithCompletion_SubStatesCompletedAndParentCompleted()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A"), true, true)
                .AddSubState(new State(3, "Pricing B"), true, false)
                .AddSubState(new State(4, "Pricing C"), true, true);

            state = state.Execute(StateExecutionType.Initialise);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            var statePricingC = state.SubStates.Single(s => s.Name.Equals("Pricing C"));

            // Act
            statePricingA = statePricingA.Execute(StateExecutionType.Complete);

            statePricingB = statePricingB.Execute(StateExecutionType.Initialise);

            state = statePricingC.Execute(StateExecutionType.Complete);

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
        public void Execute_TransitionState_StateCompleted()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow");

            // Act
            state = state.Execute(StateExecutionType.Complete);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Completed);
            Assert.IsNull(state.Transition);
        }

        [TestMethod]
        [ExpectedException(typeof(StateException))]
        public void Execute_TransitionStateToStateNotOnTransitionList_StateExceptionRaised()
        {
            // Arrange
            var review = new State(2, "Review");
            var state = new State(1, "Pricing Workflow");

            state = state.Execute(StateExecutionType.Initialise);

            // Act
            review = state.Execute(review);
        }

        [TestMethod]
        public void Execute_TransitionStateToStateNotOnTransitionList_StateFailedToCompleteOrTransition()
        {
            // Arrange
            var review = new State(2, "Review");
            var state = new State(1, "Pricing Workflow");

            state = state.Execute(StateExecutionType.Initialise);

            try
            {
                // Act
                review = state.Execute(review);
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
                Assert.IsTrue(ex.Messages.Contains(logEntry.Message));
            }

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
            Assert.IsNull(state.Transition);
        }

        [TestMethod]
        public void Execute_TransitionState_StateCompletedAndTransitionStateInitialised()
        {
            // Arrange
            var review = new State(2, "Review");
            var state = new State(1, "Pricing Workflow")
                .AddTransition(review);

            state = state.Execute(StateExecutionType.Initialise);

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
        public void Execute_TransitionSubStateAndTransitionParent_SubStateCompletedAndParentCompletedAndTransitionStateInitialised()
        {
            // Arrange
            var review = new State(3, "Review");
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing"), true)
                .AddTransition(review, true);

            state = state.Execute(StateExecutionType.Initialise);

            var pricing = state.SubStates.First();

            // Act
            review = pricing.Execute(StateExecutionType.Complete);

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
        public void Execute_TransitionStateCanCompleteFalse_StateNotTransitioned()
        {
            // Arrange
            var mockPredicate = new Mock<Predicate<State>>();
            mockPredicate.SetReturnsDefault(false); 

            var transitionState = new State(0, "Transition State");
            var state = new State(1, "State")
                .AddCanCompletePredicate(mockPredicate.Object)
                .AddTransition(transitionState);

            state = state.Execute(StateExecutionType.Initialise);

            // Act
            var result = state.Execute(transitionState);

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
        public void Execute_TransitionStateToAntecedentWithoutCompleteTrue_AntecedentResetAndThenInitialised()
        {
            // Arrange
            var pricingWorkflow = new State(1, "Pricing Workflow");
            var review = new State(2, "Review");
            var outcome = new State(3, "Outcome");

            pricingWorkflow.AddTransition(review);
            review.AddTransition(outcome);
            outcome.AddTransition(review);

            pricingWorkflow.Execute(StateExecutionType.Initialise);
            review = pricingWorkflow.Execute(review);
            outcome = review.Execute(outcome);

            // Act
            outcome.Execute(review, true);

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
        public void Execute_TransitionStateToAntecedentWithoutCompleteFalse_AntecedentResetAndThenInitialised()
        {
            // Arrange
            var pricingWorkflow = new State(1, "Pricing Workflow");
            var review = new State(2, "Review");
            var outcome = new State(3, "Outcome");

            pricingWorkflow.AddTransition(review);
            review.AddTransition(outcome);
            outcome.AddTransition(review);

            pricingWorkflow.Execute(StateExecutionType.Initialise);
            review = pricingWorkflow.Execute(review);
            outcome = review.Execute(outcome);

            // Act
            outcome.Execute(review);

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
        public void Execute_ChangeStatusCanChangeStatusReturnsTrue_StatusChanged()
        {
            // Arrange
            var mockPredicate = new Mock<Predicate<State>>();
            mockPredicate.SetReturnsDefault(true);

            var state = new State(1, "Pricing Workflow")
                .AddCanChangeStatusPredicate(mockPredicate.Object);

            state.Execute(StateExecutionType.Initialise);

            // Act
            state.Execute(StateExecutionType.InProgress);

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.InProgress);
        }

        [TestMethod]
        public void Execute_ChangeStatusCanChangeStatusReturnsFalse_StatusUnchanged()
        {
            // Arrange
            var mockPredicate = new Mock<Predicate<State>>();
            mockPredicate.SetReturnsDefault(false);

            var state = new State(1, "Pricing Workflow")
                .AddCanChangeStatusPredicate(mockPredicate.Object);

            state.Execute(StateExecutionType.Initialise);

            // Act
            state.Execute(StateExecutionType.InProgress);

            // Assert
            mockPredicate.Verify(p => p(state), Times.Once);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public void Execute_ResetStateCanResetFalse_StateNotReset()
        {
            // Arrange
            var mockPredicate = new Mock<Predicate<State>>();
            mockPredicate.SetReturnsDefault(false);

            var outcome = new State(3, "Outcome");

            var review = new State(2, "Review")
                .AddTransition(outcome)
                .AddCanResetPredicate(mockPredicate.Object);

            var pricingWorkflow = new State(1, "Pricing Workflow")
                .AddTransition(review);

            pricingWorkflow.Execute(StateExecutionType.Initialise);
            review = pricingWorkflow.Execute(review);
            outcome = review.Execute(outcome);

            // Act
            review.Execute(StateExecutionType.Reset);

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
        public void Execute_ResetState_StateResetAndTransitions()
        {
            // Arrange
            var outcome = new State(3, "Outcome");

            var review = new State(2, "Review")
                .AddTransition(outcome);

            var pricingWorkflow = new State(1, "Pricing Workflow")
                .AddTransition(review);

            pricingWorkflow.Execute(StateExecutionType.Initialise);
            review = pricingWorkflow.Execute(review);
            outcome = review.Execute(outcome);

            // Act
            review.Execute(StateExecutionType.Reset);

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
        public void Execute_ResetState_StateResetAndDependenciesReset()
        {
            // Arrange
            var outcome = new State(3, "Outcome");

            var review = new State(2, "Review")
                .AddDependant(outcome, true);

            var pricingWorkflow = new State(1, "Pricing Workflow")
                .AddTransition(review);

            pricingWorkflow.Execute(StateExecutionType.Initialise);
            review = pricingWorkflow.Execute(review);
            review.Execute(StateExecutionType.Complete);

            // Act
            review.Execute(StateExecutionType.Reset);

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
        public void Execute_ResetAllSubStatesResetsParent_ParentAndSubStatesReset()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A"), true)
                .AddSubState(new State(3, "Pricing B"), true);

            state = state.Execute(StateExecutionType.Initialise);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            // Act
            statePricingA = statePricingA.Execute(StateExecutionType.Reset);

            statePricingB = statePricingB.Execute(StateExecutionType.Reset);

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
        public void Execute_ResetParentResetsSubStates_ParentAndSubStatesReset()
        {
            // Arrange
            var state = new State(1, "Pricing Workflow")
                .AddSubState(new State(2, "Pricing A"), true)
                .AddSubState(new State(3, "Pricing B"), true)
                .AddSubState(new State(4, "Pricing C"));

            state = state.Execute(StateExecutionType.Initialise);

            var statePricingA = state.SubStates.Single(s => s.Name.Equals("Pricing A"));

            var statePricingB = state.SubStates.Single(s => s.Name.Equals("Pricing B"));

            var statePricingC = state.SubStates.Single(s => s.Name.Equals("Pricing C"));

            // Act
            state.Execute(StateExecutionType.Reset);

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
        public void Execute_ResetParentSubStateCanResetFalse_ParentAndSubStateNotReset()
        {
            // Arrange
            var mockPredicate = new Mock<Predicate<State>>();
            mockPredicate.SetReturnsDefault(false);

            var statePricingA = new State(2, "Pricing A")
                .AddCanResetPredicate(mockPredicate.Object);

            var statePricingB = new State(3, "Pricing B");

            var state = new State(1, "Pricing Workflow")
                .AddSubState(statePricingA, true)
                .AddSubState(statePricingB, true);

            state = state.Execute(StateExecutionType.Initialise);

            // Act
            state.Execute(StateExecutionType.Reset);

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
    }
}