using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class DipStateTest
    {
        [TestMethod]
        public void Status_ChangeStatus_StatusChangedAndIsDirtySetTrueAndLogEntry()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow");

            // Act
            state = state.Execute(DipStateStatus.Initialised);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
            Assert.IsTrue(state.IsDirty);

            var logEntry = state.Log.First();
            Assert.IsTrue(logEntry.Message.StartsWith(String.Format("{0} - {1}", state.Name, DipStateStatus.Initialised)));
        }

        [TestMethod]
        public void CanComplete_NoCanCompletePredicate_CanCompleteReturnsTrue()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow");

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.CanComplete());
        }

        [TestMethod]
        public void CanComplete_PredicateReturnsTrue_CanCompleteReturnsTrue()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow", canComplete: s => true);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.CanComplete());
        }

        [TestMethod]
        public void CanComplete_PredicateReturnsFalse_CanCompleteReturnsFalse()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow", canComplete: s => false);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsFalse(state.CanComplete());
        }

        [TestMethod]
        public void Reset_StateIsReset()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow");
            state = state.Execute(DipStateStatus.Initialised);

            // Act
            state.Reset(true);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Uninitialised);
            Assert.IsFalse(state.IsDirty);
            Assert.IsTrue(state.Log.Count.Equals(0));
        }

        [TestMethod]
        public void AddSubState_SubStateAddedAndSubStateParentSet()
        {
            // Arrange
            var subState = new DipState(2, "Data Capture");

            var state = new DipState(1, "Pricing Workflow")
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
            var review = new DipState(2, "Review");

            var state = new DipState(1, "Pricing Workflow")
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
            var state = new DipState(1, "Pricing Workflow")
                .AddAction(DipStateActionType.Entry, dipState => Debug.Write(dipState.Name));

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.Actions.Count().Equals(1));
            Assert.IsTrue(state.Actions.Count(a => a.ActionType.Equals(DipStateActionType.Entry)).Equals(1));
        }

        [TestMethod]
        public void AddAction_AddExitAction_ExitActionAdded()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow")
                .AddAction(DipStateActionType.Exit, dipState => Debug.Write(dipState.Name));

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.Actions.Count().Equals(1));
            Assert.IsTrue(state.Actions.Count(a => a.ActionType.Equals(DipStateActionType.Exit)).Equals(1));
        }

        [TestMethod]
        public void AddDependant_WithInitialiseDependantWhenCompleteSetTrue_DependentAndDependcyReferencesSet()
        {
            // Arrange
            var dependant = new DipState(0, "Dependant");

            var state = new DipState(1, "Pricing Workflow")
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
            var dependant = new DipState(2, "Dependant");

            var state = new DipState(1, "Pricing Workflow")
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
            var dependency = new DipState(0, "Dependency");

            var state = new DipState(1, "Pricing Workflow")
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
            var dependency = new DipState(0, "Dependency");

            var state = new DipState(1, "Pricing Workflow")
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
            var subState1 = new DipState(2, "Sub State 1");
            var subState2 = new DipState(3, "Sub State 2");
            var subState3 = new DipState(6, "Sub State 3");

            var subState2_1 = new DipState(4, "Sub State 2_1");
            var subState2_2 = new DipState(5, "Sub State 2_2");

            subState2
                .AddSubState(subState2_1)
                .AddSubState(subState2_2);

            var rootState = new DipState(1, "Root")
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
            var subState1 = new DipState(2, "Sub State 1");
            var SubState2 = new DipState(3, "Sub State 2");
            var SubState3 = new DipState(6, "Sub State 3");

            var SubState2_1 = new DipState(4, "Sub State 2_1");
            var SubState2_2 = new DipState(5, "Sub State 2_2");

            SubState2
                .AddSubState(SubState2_1)
                .AddSubState(SubState2_2);

            var rootState = new DipState(1, "Root")
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
            var subState1 = new DipState(2, "Sub State 1");
            var SubState2 = new DipState(3, "Sub State 2");
            var SubState3 = new DipState(6, "Sub State 3");

            var SubState2_1 = new DipState(4, "Sub State 2_1");
            var SubState2_2 = new DipState(5, "Sub State 2_2");

            SubState2
                .AddSubState(SubState2_1)
                .AddSubState(SubState2_2);

            var rootState = new DipState(1, "Root")
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
            var subState1 = new DipState(2, "Sub State 1");
            var SubState2 = new DipState(3, "Sub State 2");
            var SubState3 = new DipState(6, "Sub State 3");

            var SubState2_1 = new DipState(4, "Sub State 2_1");
            var SubState2_2 = new DipState(5, "Sub State 2_2");

            SubState2
                .AddSubState(SubState2_1)
                .AddSubState(SubState2_2);

            var rootState = new DipState(1, "Root")
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
            var subState1 = new DipState(2, "Sub State 1");
            var SubState2 = new DipState(3, "Sub State 2");
            var SubState3 = new DipState(6, "Sub State 3");

            var SubState2_1 = new DipState(4, "Sub State 2_1");
            var SubState2_2 = new DipState(5, "Sub State 2_2");

            SubState2
                .AddSubState(SubState2_1)
                .AddSubState(SubState2_2);

            var rootState = new DipState(1, "Root")
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
    }
}