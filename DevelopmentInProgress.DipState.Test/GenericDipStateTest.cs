using System;
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
            Assert.AreEqual(((DipState<DipStateContextText>)state).Context.Text, "Entry Action");
        }
    }
}
