using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class AsyncWorkflowFailStateTest
    {
        private State pricingWorkflow;
        private State collateData;
        private State initialCommunication;
        private State communicationReponse;
        private State modelling;
        private State modelling1;
        private State modelling2;
        private State modellingReview;
        private State adjustmentCheck;
        private State adjustments;
        private State finalReview;
        private State finalCommunication;

        [TestInitialize]
        public void Initialise()
        {
            pricingWorkflow = new State(1000, "Pricing Workflow");
            initialCommunication = new State(2100, "Initial Communication", true);
            communicationReponse = new State(2200, "Communication Response");
            collateData = new State(3000, "Collate Data", true);
            modelling = new State(4000, "Modelling");
            modelling1 = new State(4100, "Modelling 1", true);
            modelling2 = new State(4200, "Modelling 2", true);
            modellingReview = new State(5000, "Modelling Review");
            adjustmentCheck = new State(6000, "Adjustment Check", type: StateType.Auto);
            adjustments = new State(7000, "Adjustments");
            finalReview = new State(8000, "Final Review");
            finalCommunication = new State(9000, "Final Communication", canCompleteParent: true);

            pricingWorkflow
                .AddSubState(initialCommunication)
                .AddSubState(communicationReponse)
                .AddSubState(collateData)
                .AddSubState(modelling)
                .AddSubState(modellingReview)
                .AddSubState(adjustmentCheck)
                .AddSubState(adjustments)
                .AddSubState(finalReview)
                .AddSubState(finalCommunication);

            initialCommunication
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWriteFast)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWriteFast)
                .AddTransition(communicationReponse);

            communicationReponse
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWriteFast)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWriteFast)
                .AddTransition(modellingReview);

            collateData
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWriteFast)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWriteFast)
                .AddTransition(modelling);

            modelling
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWriteFast)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWriteFast)
                .AddSubState(modelling1)
                .AddSubState(modelling2)
                .AddTransition(modellingReview);

            modellingReview
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWriteFast)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWriteFast)
                .AddTransition(adjustmentCheck)
                .AddDependency(modelling)
                .AddDependency(communicationReponse);

            adjustmentCheck
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWriteFast)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWriteFast)
                .AddTransition(adjustments)
                .AddTransition(finalReview);

            adjustments
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWriteFast)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWriteFast)
                .AddTransition(finalReview);

            finalReview
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWriteFast)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWriteFast)
                .AddTransition(finalCommunication);

            finalCommunication
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWriteFast)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWriteFast)
                .AddTransition(initialCommunication)
                .AddTransition(communicationReponse)
                .AddTransition(collateData)
                .AddTransition(modelling)
                .AddTransition(modellingReview)
                .AddTransition(adjustmentCheck)
                .AddTransition(adjustments)
                .AddTransition(finalReview);
        }

        [TestMethod]
        public async Task FinalCommunicationFail()
        {
            // Arrange
            await Arrange(false);

            await finalCommunication.ExecuteAsync(StateStatus.Fail);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Complete);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Complete);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToFinalReview()
        {
            // Arrange
            Arrange(false);

            await finalCommunication.ExecuteAsync(StateStatus.Fail, finalReview);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Complete);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Initialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToAdjustments()
        {
            // Arrange
            Arrange(true);

            await finalCommunication.ExecuteAsync(StateStatus.Fail, adjustments);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Complete);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
            Assert.AreEqual(adjustments.Status, StateStatus.Initialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToAdjustmentCheck_TransitionToAdjustments()
        {
            // Arrange
            Arrange(true);

            await finalCommunication.ExecuteAsync(StateStatus.Fail, adjustmentCheck);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Complete);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
            Assert.AreEqual(adjustments.Status, StateStatus.Initialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToAdjustmentCheck_TransitionToFinalReview()
        {
            // Arrange
            Arrange(false);

            await finalCommunication.ExecuteAsync(StateStatus.Fail, adjustmentCheck);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Complete);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Initialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToModellingReview()
        {
            // Arrange
            Arrange(false);

            await finalCommunication.ExecuteAsync(StateStatus.Fail, modellingReview);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Complete);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Initialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToModelling()
        {
            // Arrange
            Arrange(false);

            await finalCommunication.ExecuteAsync(StateStatus.Fail, modelling);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Complete);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling1.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling2.Status, StateStatus.Initialise);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToCollateData()
        {
            // Arrange
            Arrange(false);

            await finalCommunication.ExecuteAsync(StateStatus.Fail, collateData);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Complete);
            Assert.AreEqual(collateData.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modelling1.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modelling2.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToCommunicationResponse()
        {
            // Arrange
            Arrange(false);

            await finalCommunication.ExecuteAsync(StateStatus.Fail, communicationReponse);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Initialise);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToInitialCommuniation()
        {
            // Arrange
            Arrange(false);

            await finalCommunication.ExecuteAsync(StateStatus.Fail, initialCommunication);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Initialise);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Uninitialise);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);
        }

        private async Task Arrange(bool requiresAdjustment)
        {
            await pricingWorkflow.ExecuteAsync(StateStatus.Initialise);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Initialise);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Initialise);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Uninitialise);
            Assert.AreEqual(collateData.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modelling1.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modelling2.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);

            await initialCommunication.ExecuteAsync(StateStatus.Complete);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Initialise);
            Assert.AreEqual(collateData.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modelling1.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modelling2.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);

            await collateData.ExecuteAsync(StateStatus.Complete);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Initialise);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling1.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling2.Status, StateStatus.Initialise);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);

            await modelling1.ExecuteAsync(StateStatus.Complete);
            await modelling2.ExecuteAsync(StateStatus.Complete);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Initialise);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);

            await communicationReponse.ExecuteAsync(StateStatus.Complete);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Complete);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Initialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);

            if (requiresAdjustment)
            {
                adjustmentCheck.AddActionAsync(StateActionType.Entry, TransitionToAdjustments);
            }
            else
            {
                adjustmentCheck.AddActionAsync(StateActionType.Entry, TransitionToFinalReview);
            }

            await modellingReview.ExecuteAsync(StateStatus.Complete);

            if (requiresAdjustment)
            {
                Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
                Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
                Assert.AreEqual(communicationReponse.Status, StateStatus.Complete);
                Assert.AreEqual(collateData.Status, StateStatus.Complete);
                Assert.AreEqual(modelling.Status, StateStatus.Complete);
                Assert.AreEqual(modelling1.Status, StateStatus.Complete);
                Assert.AreEqual(modelling2.Status, StateStatus.Complete);
                Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
                Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
                Assert.AreEqual(adjustments.Status, StateStatus.Initialise);
                Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
                Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);

                await adjustments.ExecuteAsync(StateStatus.Complete);

                Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
                Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
                Assert.AreEqual(communicationReponse.Status, StateStatus.Complete);
                Assert.AreEqual(collateData.Status, StateStatus.Complete);
                Assert.AreEqual(modelling.Status, StateStatus.Complete);
                Assert.AreEqual(modelling1.Status, StateStatus.Complete);
                Assert.AreEqual(modelling2.Status, StateStatus.Complete);
                Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
                Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
                Assert.AreEqual(adjustments.Status, StateStatus.Complete);
                Assert.AreEqual(finalReview.Status, StateStatus.Initialise);
                Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);

                await finalReview.ExecuteAsync(StateStatus.Complete);

                Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
                Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
                Assert.AreEqual(communicationReponse.Status, StateStatus.Complete);
                Assert.AreEqual(collateData.Status, StateStatus.Complete);
                Assert.AreEqual(modelling.Status, StateStatus.Complete);
                Assert.AreEqual(modelling1.Status, StateStatus.Complete);
                Assert.AreEqual(modelling2.Status, StateStatus.Complete);
                Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
                Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
                Assert.AreEqual(adjustments.Status, StateStatus.Complete);
                Assert.AreEqual(finalReview.Status, StateStatus.Complete);
            }
            else
            {
                Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
                Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
                Assert.AreEqual(communicationReponse.Status, StateStatus.Complete);
                Assert.AreEqual(collateData.Status, StateStatus.Complete);
                Assert.AreEqual(modelling.Status, StateStatus.Complete);
                Assert.AreEqual(modelling1.Status, StateStatus.Complete);
                Assert.AreEqual(modelling2.Status, StateStatus.Complete);
                Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
                Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
                Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
                Assert.AreEqual(finalReview.Status, StateStatus.Initialise);
                Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);

                await finalReview.ExecuteAsync(StateStatus.Complete);

                Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
                Assert.AreEqual(initialCommunication.Status, StateStatus.Complete);
                Assert.AreEqual(communicationReponse.Status, StateStatus.Complete);
                Assert.AreEqual(collateData.Status, StateStatus.Complete);
                Assert.AreEqual(modelling.Status, StateStatus.Complete);
                Assert.AreEqual(modelling1.Status, StateStatus.Complete);
                Assert.AreEqual(modelling2.Status, StateStatus.Complete);
                Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
                Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
                Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
                Assert.AreEqual(finalReview.Status, StateStatus.Complete);
            }

            Assert.AreEqual(finalCommunication.Status, StateStatus.Initialise);
        }

        public static async Task TransitionToAdjustments(State state)
        {
            var startLogEntry = new LogEntry(String.Format("Start Entry Async Action - {0}", state.Name));
            state.Log.Add(startLogEntry);

            Debug.WriteLine(startLogEntry.ToString());

            state.Transition = state.Transitions.FirstOrDefault(s => s.Name.Equals("Adjustments"));

            var endLogEntry = new LogEntry(String.Format("End Entry Async Action - {0}", state.Name));
            state.Log.Add(endLogEntry);

            Debug.WriteLine(endLogEntry.ToString());
        }

        public static async Task TransitionToFinalReview(State state)
        {
            var startLogEntry = new LogEntry(String.Format("Start Entry Async Action - {0}", state.Name));
            state.Log.Add(startLogEntry);

            Debug.WriteLine(startLogEntry.ToString());

            state.Transition = state.Transitions.FirstOrDefault(s => s.Name.Equals("Final Review"));

            var endLogEntry = new LogEntry(String.Format("End Entry Async Action - {0}", state.Name));
            state.Log.Add(endLogEntry);

            Debug.WriteLine(endLogEntry.ToString());
        }
    }
}
