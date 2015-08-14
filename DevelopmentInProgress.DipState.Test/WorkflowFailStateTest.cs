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
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddTransition(communicationReponse);

            communicationReponse
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddTransition(modellingReview);

            collateData
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddTransition(modelling);

            modelling
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddSubState(modelling1)
                .AddSubState(modelling2)
                .AddTransition(modellingReview);

            modellingReview
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddTransition(adjustmentCheck)
                .AddDependency(modelling)
                .AddDependency(communicationReponse);

            adjustmentCheck
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddTransition(adjustments)
                .AddTransition(finalReview);

            adjustments
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddTransition(finalReview);

            finalReview
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddTransition(finalCommunication);

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

            finalCommunication.Execute(StateStatus.Fail);

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
        public void FinalCommunicationFailToFinalReview()
        {
            // Arrange
            Arrange(false);

            finalCommunication.Execute(StateStatus.Fail, finalReview);

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
        public void FinalCommunicationFailToAdjustments()
        {
            // Arrange
            Arrange(true);

            finalCommunication.Execute(StateStatus.Fail, adjustments);

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
        public void FinalCommunicationFailToAdjustmentCheck_TransitionToAdjustments()
        {
            // Arrange
            Arrange(true);

            finalCommunication.Execute(StateStatus.Fail, adjustmentCheck);

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
        public void FinalCommunicationFailToAdjustmentCheck_TransitionToFinalReview()
        {
            // Arrange
            Arrange(false);

            finalCommunication.Execute(StateStatus.Fail, adjustmentCheck);

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
        public void FinalCommunicationFailToModellingReview()
        {
            // Arrange
            Arrange(false);

            finalCommunication.Execute(StateStatus.Fail, modellingReview);

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
        public void FinalCommunicationFailToModelling()
        {
            // Arrange
            Arrange(false);

            finalCommunication.Execute(StateStatus.Fail, modelling);

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
        public void FinalCommunicationFailToCollateData()
        {
            // Arrange
            Arrange(false);

            finalCommunication.Execute(StateStatus.Fail, collateData);

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
        public void FinalCommunicationFailToCommunicationResponse()
        {
            // Arrange
            Arrange(false);

            finalCommunication.Execute(StateStatus.Fail, communicationReponse);

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
        public void FinalCommunicationFailToInitialCommuniation()
        {
            // Arrange
            Arrange(false);

            finalCommunication.Execute(StateStatus.Fail, initialCommunication);

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

        private void Arrange(bool requiresAdjustment)
        {
            pricingWorkflow.Execute(StateStatus.Initialise);

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

            initialCommunication.Execute(StateStatus.Complete);

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

            collateData.Execute(StateStatus.Complete);

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

            modelling1.Execute(StateStatus.Complete);
            modelling2.Execute(StateStatus.Complete);

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

            communicationReponse.Execute(StateStatus.Complete);

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

            modellingReview.Execute(StateStatus.Complete);

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

                adjustments.Execute(StateStatus.Complete);

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

                finalReview.Execute(StateStatus.Complete);

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

                finalReview.Execute(StateStatus.Complete);

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
    }
}
