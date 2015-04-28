using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
        public void Run_StateAlreadyHasStatusSpecified_StateStatusUnchanged()
        {
            // Arrange
            IDipState state = new DipState(1, "Pricing Workflow", DipStateType.Standard, DipStateStatus.Initialised);

            // Act
            state = dipStateEngine.Run(state, DipStateStatus.Initialised);

            // Assert
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
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
            var mockAction = new Mock<Action<IDipState>>();

            var state = new DipState(1, "Pricing Workflow")
                .AddAction(DipStateActionType.Entry, mockAction.Object);

            // Act
            state = dipStateEngine.Run(state, DipStateStatus.Initialised) as DipState;

            // Assert
            mockAction.Verify(a => a(state), Times.Once);
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public void Run_InitialiseStateWithExceptionInEntryAction_ExceptionThrownStateNotInitialised()
        {
            // Arrange 
            var mockAction = new Mock<Action<IDipState>>();

            var state = new DipState(1, "Pricing Workflow")
                .AddAction(DipStateActionType.Entry, mockAction.Object);

            mockAction.Setup(a => a(state)).Throws(new InvalidOperationException("Run_InitialiseStateWithExceptionInEntryAction_ExceptionThrownStateNotInitialised"));

            // Act
            try
            {
                state = dipStateEngine.Run(state, DipStateStatus.Initialised) as DipState;
            }
            catch (InvalidOperationException ex)
            {
                if (!ex.Message.Equals("Run_InitialiseStateWithExceptionInEntryAction_ExceptionThrownStateNotInitialised"))
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
        public void Run_InitialiseStateWithDependencyComplete_StateInitialised()
        {
            // Arrange
            var dependency = new DipState(0, "Dependent State", DipStateType.Standard, DipStateStatus.Completed);

            var state = new DipState(1, "Pricing Workflow")
                .AddDependency(dependency);

            // Act
            state = dipStateEngine.Run(state, DipStateStatus.Initialised);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public void Run_InitialiseStateWithDependencyNotComplete_StateNotInitialised()
        {
            // Arrange
            var dependency = new DipState(0, "Dependent State", DipStateType.Standard, DipStateStatus.InProgress);

            var state = new DipState(1, "Pricing Workflow")
                .AddDependency(dependency);

            // Act
            state = dipStateEngine.Run(state, DipStateStatus.Initialised);

            // Assert
            var logEntry = state.Log.First();
            Assert.IsTrue(logEntry.Message.StartsWith(String.Format("{0} is dependent on", state.Name)));
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Uninitialised);
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