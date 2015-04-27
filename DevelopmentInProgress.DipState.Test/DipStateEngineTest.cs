using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class DipStateEngineTest
    {
        private DipStateEngine dipStateEngine;

        [TestInitialize]
        public void Initialise()
        {
            dipStateEngine = new DipStateEngine();
        }

        [TestMethod]
        public void Run_InitialiseState_StateInitialised()
        {
            // Arrange
            IDipState state = new DipState(1, "Pricing Workflow");

            // Act
            state = dipStateEngine.Run(state, DipStateStatus.Initialised);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public void Run_InitialiseStateWithEntryAction_StateInitialised()
        {
            // Arrange
            var state = new DipState(1, "Pricing Workflow")
                .AddAction(DipStateActionType.Entry,
                s => Debug.WriteLine(String.Format("Test entry action for {0}", s.Name)));

            // Act
            state = dipStateEngine.Run(state, DipStateStatus.Initialised) as DipState;

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public void Run_InitialiseStateWithExceptionInEntryAction_ExceptionThrownStateNotInitialised()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Run_InitialiseStateWithDependencyComplete_StateInitialised()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Run_InitialiseStateWithDependencyNotComplete_StateNotInitialised()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Run_InitialiseAutoStateAndComplete_AutoStateCompleted()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Run_InitialiseAutoStateAndAutoTransition_AutoStateTransitioned()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Run_InitialiseAggregateStateInitialiseTwoSubStatesWithParent_AggregateStateInitialisedTwoSubStatesInitialised()
        {
            throw new NotImplementedException();
        }
    }
}