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
            IDipState state = new DipState(1, "Pricing Workflow", DipStateType.Standard,
                status: DipStateStatus.Initialised);

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

            IDipState state = new DipState(1, "Pricing Workflow")
                .AddAction(DipStateActionType.Entry, mockAction.Object);

            // Act
            state = dipStateEngine.Run(state, DipStateStatus.Initialised);

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

            IDipState state = new DipState(1, "Pricing Workflow")
                .AddAction(DipStateActionType.Entry, mockAction.Object);

            mockAction.Setup(a => a(state)).Throws(new InvalidOperationException("Run_InitialiseStateWithExceptionInEntryAction_ExceptionThrownStateNotInitialised"));

            // Act
            try
            {
                state = dipStateEngine.Run(state, DipStateStatus.Initialised);
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
            var dependency = new DipState(0, "Dependent State", DipStateType.Standard, status: DipStateStatus.Completed);

            IDipState state = new DipState(1, "Pricing Workflow")
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
            var dependency = new DipState(0, "Dependent State", DipStateType.Standard, status: DipStateStatus.InProgress);

            IDipState state = new DipState(1, "Pricing Workflow")
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
            // Arrange
            IDipState autoState = new DipState(1, "Override Check", DipStateType.Auto);

            // Act
            autoState = dipStateEngine.Run(autoState, DipStateStatus.Initialised);

            // Assert
            Assert.AreEqual(autoState.Id, 1);
            Assert.AreEqual(autoState.Name, "Override Check");
            Assert.AreEqual(autoState.Status, DipStateStatus.Completed);
        }

        [TestMethod]
        public void Run_InitialiseAutoStateAndAutoTransition_AutoStateTransitionedToFinal()
        {
            // Arrange
            var finalState = new DipState(2, "Final");
            var overrideState = new DipState(3, "Override");

            IDipState autoState = new DipState(1, "Override Check", DipStateType.Auto)
                .AddTransition(finalState)
                .AddTransition(overrideState)
                .AddAction(DipStateActionType.Entry, s => { s.Transition = finalState; });

            // Act
            var state = dipStateEngine.Run(autoState, DipStateStatus.Initialised);

            // Assert
            Assert.AreEqual(autoState.Id, 1);
            Assert.AreEqual(autoState.Name, "Override Check");
            Assert.AreEqual(autoState.Status, DipStateStatus.Completed);

            Assert.AreEqual(state.Id, 2);
            Assert.AreEqual(state.Name, "Final");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);

            Assert.AreEqual(overrideState.Id, 3);
            Assert.AreEqual(overrideState.Name, "Override");
            Assert.AreEqual(overrideState.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public void Run_InitialiseAutoStateAndAutoTransition_AutoStateTransitionedToOverride()
        {
            // Arrange
            var finalState = new DipState(2, "Final");
            var overrideState = new DipState(3, "Override");

            IDipState autoState = new DipState(1, "Override Check", DipStateType.Auto)
                .AddTransition(finalState)
                .AddTransition(overrideState)
                .AddAction(DipStateActionType.Entry, s => { s.Transition = overrideState; });

            // Act
            var state = dipStateEngine.Run(autoState, DipStateStatus.Initialised);

            // Assert
            Assert.AreEqual(autoState.Id, 1);
            Assert.AreEqual(autoState.Name, "Override Check");
            Assert.AreEqual(autoState.Status, DipStateStatus.Completed);

            Assert.AreEqual(finalState.Id, 2);
            Assert.AreEqual(finalState.Name, "Final");
            Assert.AreEqual(finalState.Status, DipStateStatus.Uninitialised);

            Assert.AreEqual(state.Id, 3);
            Assert.AreEqual(state.Name, "Override");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public void Run_InitialiseAggregateStateInitialiseTwoSubStatesWithParent_AggregateStateInitialisedTwoSubStatesInitialised()
        {
            // Arrange
            IDipState state = new DipState(1, "Pricing Workflow")
                .AddSubState(new DipState(2, "Pricing A", initialiseWithParent: true))
                .AddSubState(new DipState(3, "Pricing B"))
                .AddSubState(new DipState(4, "Pricing C", initialiseWithParent: true));

            // Act
            state = dipStateEngine.Run(state, DipStateStatus.Initialised);

            // Assert
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Id, 1);
            Assert.AreEqual(state.Name, "Pricing Workflow");
            Assert.AreEqual(state.Status, DipStateStatus.Initialised);

            var pricingA = state.SubStates.First(pa => pa.Name.Equals("Pricing A"));
            Assert.AreEqual(pricingA.Id, 2);
            Assert.AreEqual(pricingA.Name, "Pricing A");
            Assert.AreEqual(pricingA.Status, DipStateStatus.Initialised);

            var pricingB = state.SubStates.First(pa => pa.Name.Equals("Pricing B"));
            Assert.AreEqual(pricingB.Id, 3);
            Assert.AreEqual(pricingB.Name, "Pricing B");
            Assert.AreEqual(pricingB.Status, DipStateStatus.Uninitialised);

            var pricingC = state.SubStates.First(pa => pa.Name.Equals("Pricing C"));
            Assert.AreEqual(pricingC.Id, 4);
            Assert.AreEqual(pricingC.Name, "Pricing C");
            Assert.AreEqual(pricingC.Status, DipStateStatus.Initialised);
        }
    }
}