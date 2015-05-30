using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class GenericDipStateTest
    {
        private DipStateEngine dipStateEngine;

        [TestInitialize]
        public void Initialise()
        {
            dipStateEngine = new DipStateEngine();
        }

        [TestMethod]
        public void CreateNewState_AssignContext_StateCreatedContextSet()
        {
            // Arrange
            var testContext = new DipStateContextText() { Text = "Pricing Workflow Context" };
            var state = new DipState<DipStateContextText>(1, "Pricing Workflow");

            // Act
            state.Context = testContext;

            // Assert
            Assert.IsNotNull(state.Context);
            Assert.IsInstanceOfType(state.Context, typeof(DipStateContextText));
            Assert.IsTrue(state.Context.Text.Equals("Pricing Workflow Context"));
        }

        [TestMethod]
        public void CreateNewState_PassContextIntoConstructor_StateCreatedContextSet()
        {
            // Arrange
            var testContext = new DipStateContextText() { Text = "Pricing Workflow Context" };

            // Act
            var state = new DipState<DipStateContextText>(testContext, 1, "Pricing Workflow");

            // Assert
            Assert.IsNotNull(state.Context);
            Assert.IsInstanceOfType(state.Context, typeof(DipStateContextText));
            Assert.IsTrue(state.Context.Text.Equals("Pricing Workflow Context"));
        }

        [TestMethod]
        public void Run_InitialiseStateWithEntryAction_StateInitialisedEntryActionExecuted()
        {
            // Arrange
            var entryAction = new Action<DipState>(a =>
            {
                var contextClass = a as DipState<DipStateContextText>;
                contextClass.Context.Text = "Entry Action";
            });

            var state = new DipState<DipStateContextText>(
                new DipStateContextText() {Text = "Uninitialised"}, 1, "Pricing Workflow")
                .AddAction(DipStateActionType.Entry, entryAction);

            // Act
            state = dipStateEngine.Run(state, DipStateStatus.Initialised);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
            Assert.IsInstanceOfType(state.Context, typeof(DipStateContextText));
            Assert.AreEqual(((DipState<DipStateContextText>)state).Context.Text, "Entry Action");
        }

        [TestMethod]
        public void AddSubState_SubStateAddedAndSubStateParentSet()
        {
            // Arrange
            var textContext = new DipStateContextText() {Text = "Pricing Workflow Context"};
            var numberContext = new DipStateContextNumber() {Number = 123};

            var subState = new DipState<DipStateContextText>(textContext, 2, "Data Capture");

            var state = new DipState<DipStateContextNumber>(numberContext, 1, "Pricing Workflow")
                .AddSubState(subState);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.IsTrue(state.SubStates.Count.Equals(1));
            Assert.AreEqual(state.SubStates.First(), subState);
            //Assert.IsInstanceOfType(state.Context, typeof(DipStateContextNumber));
            
            Assert.AreEqual(subState.Id, 2);
            Assert.AreEqual(subState.Name, "Data Capture");
            Assert.AreEqual(subState.Parent, state);
            Assert.IsInstanceOfType(subState.Context, typeof(DipStateContextText));
            Assert.IsTrue(subState.Context.Text.Equals("Pricing Workflow Context"));
        }
    }
}
