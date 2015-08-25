using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class WorkflowFailStateTest
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
        public void FinalCommunicationFail()
        {
            // Arrange
            Arrange(false);

            finalCommunication.Execute(StateExecutionType.Reset);

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
        public void FinalCommunicationFailToFinalReview()
        {
            // Arrange
            Arrange(false);

            finalCommunication.Execute(finalReview, true);

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
        public void FinalCommunicationFailToAdjustments()
        {
            // Arrange
            Arrange(true);

            finalCommunication.Execute(adjustments, true);

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
        public void FinalCommunicationFailToAdjustmentCheck_TransitionToAdjustments()
        {
            // Arrange
            Arrange(true);

            finalCommunication.Execute(adjustmentCheck, true);

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
        public void FinalCommunicationFailToAdjustmentCheck_TransitionToFinalReview()
        {
            // Arrange
            Arrange(false);

            finalCommunication.Execute(adjustmentCheck, true);

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
        public void FinalCommunicationFailToModellingReview()
        {
            // Arrange
            Arrange(false);

            finalCommunication.Execute(modellingReview, true);

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
        public void FinalCommunicationFailToModelling()
        {
            // Arrange
            Arrange(false);

            finalCommunication.Execute(modelling, true);

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
        public void FinalCommunicationFailToCollateData()
        {
            // Arrange
            Arrange(false);

            finalCommunication.Execute(collateData, true);

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
        public void FinalCommunicationFailToCommunicationResponse()
        {
            // Arrange
            Arrange(false);

            finalCommunication.Execute(communicationReponse, true);

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
        public void FinalCommunicationFailToInitialCommuniation()
        {
            // Arrange
            Arrange(false);

            finalCommunication.Execute(initialCommunication, true);

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

        private void Arrange(bool requiresAdjustment)
        {
            pricingWorkflow.Execute(StateExecutionType.Initialise);

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

            initialCommunication.Execute(StateExecutionType.Complete);

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

            collateData.Execute(StateExecutionType.Complete);

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

            modelling1.Execute(StateExecutionType.Complete);
            modelling2.Execute(StateExecutionType.Complete);

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

            communicationReponse.Execute(StateExecutionType.Complete);

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
                adjustmentCheck.AddAction(StateActionType.Entry, (a) =>
                {
                    a.Transition = adjustments;
                });
            }
            else
            {
                adjustmentCheck.AddAction(StateActionType.Entry, (a) =>
                {
                    a.Transition = finalReview;
                });
            }

            modellingReview.Execute(StateExecutionType.Complete);

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

                adjustments.Execute(StateExecutionType.Complete);

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

                finalReview.Execute(StateExecutionType.Complete);

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

                finalReview.Execute(StateExecutionType.Complete);

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
    }
}
