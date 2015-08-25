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
            initialCommunication = new State(2100, "Initial Communication");
            communicationReponse = new State(2200, "Communication Response");
            collateData = new State(3000, "Collate Data");
            modelling = new State(4000, "Modelling");
            modelling1 = new State(4100, "Modelling 1");
            modelling2 = new State(4200, "Modelling 2");
            modellingReview = new State(5000, "Modelling Review");
            adjustmentCheck = new State(6000, "Adjustment Check", type: StateType.Auto);
            adjustments = new State(7000, "Adjustments");
            finalReview = new State(8000, "Final Review");
            finalCommunication = new State(9000, "Final Communication");

            pricingWorkflow
                .AddSubState(initialCommunication, true)
                .AddSubState(communicationReponse)
                .AddSubState(collateData, true)
                .AddSubState(modelling)
                .AddSubState(modellingReview)
                .AddSubState(adjustmentCheck)
                .AddSubState(adjustments, completionRequired: false)
                .AddSubState(finalReview)
                .AddSubState(finalCommunication);

            initialCommunication
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddTransition(communicationReponse, true);

            communicationReponse
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddTransition(modellingReview, true);

            collateData
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddTransition(modelling, true);

            modelling
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddSubState(modelling1, true)
                .AddSubState(modelling2, true)
                .AddTransition(modellingReview, true);

            modellingReview
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddTransition(adjustmentCheck, true)
                .AddDependency(modelling, true)
                .AddDependency(communicationReponse, true);

            adjustmentCheck
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddTransition(adjustments)
                .AddTransition(finalReview);

            adjustments
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddTransition(finalReview, true);

            finalReview
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddTransition(finalCommunication, true);

            finalCommunication
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
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
            Arrange(false);

            await finalCommunication.ExecuteAsync(StateExecutionType.Reset);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Completed);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Completed);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToFinalReview()
        {
            // Arrange
            Arrange(false);

            await finalCommunication.ExecuteAsync(finalReview, true);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Completed);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Initialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToAdjustments()
        {
            // Arrange
            Arrange(true);

            await finalCommunication.ExecuteAsync(adjustments, true);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Completed);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
            Assert.AreEqual(adjustments.Status, StateStatus.Initialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToAdjustmentCheck_TransitionToAdjustments()
        {
            // Arrange
            Arrange(true);

            await finalCommunication.ExecuteAsync(adjustmentCheck, true);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Completed);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
            Assert.AreEqual(adjustments.Status, StateStatus.Initialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToAdjustmentCheck_TransitionToFinalReview()
        {
            // Arrange
            Arrange(false);

            await finalCommunication.ExecuteAsync(adjustmentCheck, true);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Completed);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Initialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToModellingReview()
        {
            // Arrange
            Arrange(false);

            await finalCommunication.ExecuteAsync(modellingReview, true);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Completed);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Initialised);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToModelling()
        {
            // Arrange
            Arrange(false);

            await finalCommunication.ExecuteAsync(modelling, true);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Completed);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Initialised);
            Assert.AreEqual(modelling1.Status, StateStatus.Initialised);
            Assert.AreEqual(modelling2.Status, StateStatus.Initialised);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToCollateData()
        {
            // Arrange
            Arrange(false);

            await finalCommunication.ExecuteAsync(collateData, true);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Completed);
            Assert.AreEqual(collateData.Status, StateStatus.Initialised);
            Assert.AreEqual(modelling.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modelling1.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modelling2.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToCommunicationResponse()
        {
            // Arrange
            Arrange(false);

            await finalCommunication.ExecuteAsync(communicationReponse, true);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Initialised);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task FinalCommunicationFailToInitialCommuniation()
        {
            // Arrange
            Arrange(false);

            await finalCommunication.ExecuteAsync(initialCommunication, true);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Initialised);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Uninitialised);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);
        }

        private async Task Arrange(bool requiresAdjustment)
        {
            await pricingWorkflow.ExecuteAsync(StateExecutionType.Initialise);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Initialised);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Initialised);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Uninitialised);
            Assert.AreEqual(collateData.Status, StateStatus.Initialised);
            Assert.AreEqual(modelling.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modelling1.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modelling2.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);

            await initialCommunication.ExecuteAsync(StateExecutionType.Complete);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Initialised);
            Assert.AreEqual(collateData.Status, StateStatus.Initialised);
            Assert.AreEqual(modelling.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modelling1.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modelling2.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);

            await collateData.ExecuteAsync(StateExecutionType.Complete);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Initialised);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Initialised);
            Assert.AreEqual(modelling1.Status, StateStatus.Initialised);
            Assert.AreEqual(modelling2.Status, StateStatus.Initialised);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);

            await modelling1.ExecuteAsync(StateExecutionType.Complete);
            await modelling2.ExecuteAsync(StateExecutionType.Complete);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Initialised);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);

            await communicationReponse.ExecuteAsync(StateExecutionType.Complete);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
            Assert.AreEqual(communicationReponse.Status, StateStatus.Completed);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Initialised);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);

            if (requiresAdjustment)
            {
                adjustmentCheck.AddActionAsync(StateActionType.Entry, TransitionToAdjustments);
            }
            else
            {
                adjustmentCheck.AddActionAsync(StateActionType.Entry, TransitionToFinalReview);
            }

            await modellingReview.ExecuteAsync(StateExecutionType.Complete);

            if (requiresAdjustment)
            {
                Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
                Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
                Assert.AreEqual(communicationReponse.Status, StateStatus.Completed);
                Assert.AreEqual(collateData.Status, StateStatus.Completed);
                Assert.AreEqual(modelling.Status, StateStatus.Completed);
                Assert.AreEqual(modelling1.Status, StateStatus.Completed);
                Assert.AreEqual(modelling2.Status, StateStatus.Completed);
                Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
                Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
                Assert.AreEqual(adjustments.Status, StateStatus.Initialised);
                Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
                Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);

                await adjustments.ExecuteAsync(StateExecutionType.Complete);

                Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
                Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
                Assert.AreEqual(communicationReponse.Status, StateStatus.Completed);
                Assert.AreEqual(collateData.Status, StateStatus.Completed);
                Assert.AreEqual(modelling.Status, StateStatus.Completed);
                Assert.AreEqual(modelling1.Status, StateStatus.Completed);
                Assert.AreEqual(modelling2.Status, StateStatus.Completed);
                Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
                Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
                Assert.AreEqual(adjustments.Status, StateStatus.Completed);
                Assert.AreEqual(finalReview.Status, StateStatus.Initialised);
                Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);

                await finalReview.ExecuteAsync(StateExecutionType.Complete);

                Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
                Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
                Assert.AreEqual(communicationReponse.Status, StateStatus.Completed);
                Assert.AreEqual(collateData.Status, StateStatus.Completed);
                Assert.AreEqual(modelling.Status, StateStatus.Completed);
                Assert.AreEqual(modelling1.Status, StateStatus.Completed);
                Assert.AreEqual(modelling2.Status, StateStatus.Completed);
                Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
                Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
                Assert.AreEqual(adjustments.Status, StateStatus.Completed);
                Assert.AreEqual(finalReview.Status, StateStatus.Completed);
            }
            else
            {
                Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
                Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
                Assert.AreEqual(communicationReponse.Status, StateStatus.Completed);
                Assert.AreEqual(collateData.Status, StateStatus.Completed);
                Assert.AreEqual(modelling.Status, StateStatus.Completed);
                Assert.AreEqual(modelling1.Status, StateStatus.Completed);
                Assert.AreEqual(modelling2.Status, StateStatus.Completed);
                Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
                Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
                Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
                Assert.AreEqual(finalReview.Status, StateStatus.Initialised);
                Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);

                await finalReview.ExecuteAsync(StateExecutionType.Complete);

                Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
                Assert.AreEqual(initialCommunication.Status, StateStatus.Completed);
                Assert.AreEqual(communicationReponse.Status, StateStatus.Completed);
                Assert.AreEqual(collateData.Status, StateStatus.Completed);
                Assert.AreEqual(modelling.Status, StateStatus.Completed);
                Assert.AreEqual(modelling1.Status, StateStatus.Completed);
                Assert.AreEqual(modelling2.Status, StateStatus.Completed);
                Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
                Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
                Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
                Assert.AreEqual(finalReview.Status, StateStatus.Completed);
            }

            Assert.AreEqual(finalCommunication.Status, StateStatus.Initialised);
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
