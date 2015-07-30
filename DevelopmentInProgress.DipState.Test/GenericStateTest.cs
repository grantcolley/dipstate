using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class GenericStateTest
    {
        [TestMethod]
        public void CreateNewState_AssignContext_StateCreatedContextSet()
        {
            // Arrange
            var testContext = new ContextText() { Text = "Pricing Workflow Context" };
            var state = new State<ContextText>(1, "Pricing Workflow");

            // Act
            state.Context = testContext;

            // Assert
            Assert.IsNotNull(state.Context);
            Assert.IsInstanceOfType(state.Context, typeof(ContextText));
            Assert.IsTrue(state.Context.Text.Equals("Pricing Workflow Context"));
        }

        [TestMethod]
        public void CreateNewState_PassContextIntoConstructor_StateCreatedContextSet()
        {
            // Arrange
            var testContext = new ContextText() { Text = "Pricing Workflow Context" };

            // Act
            var state = new State<ContextText>(testContext, 1, "Pricing Workflow");

            // Assert
            Assert.IsNotNull(state.Context);
            Assert.IsInstanceOfType(state.Context, typeof(ContextText));
            Assert.IsTrue(state.Context.Text.Equals("Pricing Workflow Context"));
        }

        [TestMethod]
        public void Run_InitialiseStateWithEntryAction_StateInitialisedEntryActionExecuted()
        {
            // Arrange
            var entryAction = new Action<State>(a =>
            {
                var contextClass = a as State<ContextText>;
                contextClass.Context.Text = "Entry Action";
            });

            var state = new State<ContextText>(
                new ContextText() {Text = "Uninitialise"}, 1, "Pricing Workflow")
                .AddAction(StateActionType.Entry, entryAction);

            // Act
            state = state.Execute(StateStatus.Initialise);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, StateStatus.Initialise);
            Assert.AreEqual(((State<ContextText>)state).Context.Text, "Entry Action");
        }

        [TestMethod]
        public void AddSubState_SubStateAddedAndSubStateParentSet()
        {
            // Arrange
            var textContext = new ContextText() {Text = "Pricing Workflow Context"};
            var numberContext = new ContextNumber() {Number = 123};

            var subState = new State<ContextText>(textContext, 2, "Data Capture");

            var state = new State<ContextNumber>(numberContext, 1, "Pricing Workflow")
                .AddSubState(subState);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.SubStates.Count.Equals(1));
            Assert.AreEqual(state.SubStates.First(), subState);
            
            Assert.AreEqual(subState.Id, 2);
            Assert.AreEqual(subState.Name, "Data Capture");
            Assert.AreEqual(subState.Parent, state);
            Assert.IsInstanceOfType(subState.Context, typeof(ContextText));
            Assert.IsTrue(subState.Context.Text.Equals("Pricing Workflow Context"));
        }
    }
}
