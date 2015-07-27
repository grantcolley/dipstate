using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class AsyncExampleWorkflowTest
    {
        private DipState pricingWorkflow;
        private DipState collateData;
        private DipState communicationUpdate;
        private DipState modelling;
        private DipState modelling1;
        private DipState modelling2;
        private DipState modellingReview;
        private DipState adjustmentCheck;
        private DipState adjustments;
        private DipState finalReview;
        private DipState finalCommunication;

        [TestInitialize]
        public void Initialise()
        {
            pricingWorkflow = new DipState(1000, "Pricing Workflow");
            collateData = new DipState(1100, "Collate Data", true);
            communicationUpdate = new DipState(1200, "Communication Update");
            modelling = new DipState(1300, "Modelling");
            modelling1 = new DipState(1310, "Modelling 1", true);
            modelling2 = new DipState(1320, "Modelling 2", true);
            modellingReview = new DipState(1400, "Modelling Review");
            adjustmentCheck = new DipState(1500, "Adjustment Check", type: DipStateType.Auto);
            adjustments = new DipState(1600, "Adjustments");
            finalReview = new DipState(1700, "Final Review");
            finalCommunication = new DipState(1800, "Final Communication", canCompleteParent: true);

            pricingWorkflow
                .AddSubState(collateData)
                .AddSubState(communicationUpdate)
                .AddSubState(modelling)
                .AddSubState(modellingReview)
                .AddSubState(adjustmentCheck)
                .AddSubState(adjustments)
                .AddSubState(finalReview)
                .AddSubState(finalCommunication);

            collateData
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddTransition(modelling);

            communicationUpdate
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddDependency(collateData, true);

            modelling
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddSubState(modelling1)
                .AddSubState(modelling2)
                .AddTransition(modellingReview);

            modellingReview
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddTransition(adjustmentCheck)
                .AddTransition(modelling)
                .AddTransition(collateData);

            adjustmentCheck
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddTransition(adjustments)
                .AddTransition(finalReview);

            adjustments
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddTransition(finalReview);

            finalReview
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddTransition(finalCommunication)
                .AddTransition(modelling)
                .AddTransition(collateData);

            finalCommunication
                .AddActionAsync(DipStateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(DipStateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddDependency(communicationUpdate);
        }

        [TestMethod]
        public async Task CollateDataFail_ResetCollateDateAndPricingWorkflow()
        {
            // Arrange
            await Arrange("PricingWorkflowInitialised");

            // Act 
            await collateData.ExecuteAsync(DipStateStatus.Failed);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(collateData.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task CollateDataComplete_CompleteCollateDataAndInitialiseModelling()
        {
            // Arrange
            await Arrange("PricingWorkflowInitialised");

            // Act
            await collateData.ExecuteAsync(DipStateStatus.Completed);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task ModellingFailBothSubTasks_ResetModellingAndItsSubStates()
        {
            // Arrange
            await Arrange("CollateDataComplete");

            // Act
            await modelling1.ExecuteAsync(DipStateStatus.Failed);
            await modelling2.ExecuteAsync(DipStateStatus.Failed);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task ModellingCompleteOneSubTaskAndFailTheOther_ModellingRemainsInProgress()
        {
            // Arrange
            await Arrange("CollateDataComplete");

            // Act 
            await modelling1.ExecuteAsync(DipStateStatus.Completed);
            await modelling2.ExecuteAsync(DipStateStatus.Failed);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.InProgress);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task ModellingCompleteBothSubTasks_CompleteModellingInitialiseModellingReview()
        {
            // Arrange
            await Arrange("CollateDataComplete");

            // Act 
            await modelling1.ExecuteAsync(DipStateStatus.Completed);
            await modelling2.ExecuteAsync(DipStateStatus.Completed);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Initialised);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task ModellingReviewFail_ResetModellingReview()
        {
            // Arrange
            await Arrange("ModellingComplete");

            // Act 
            await modellingReview.ExecuteAsync(DipStateStatus.Failed);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task ModellingReviewFailToModelling_ResetModellingReviewAndInitialiseModelling()
        {
            // Arrange
            await Arrange("ModellingComplete");

            // Act 
            await modellingReview.ExecuteAsync(DipStateStatus.Failed, modelling);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task ModellingReviewFailToCollateData_ResetModellingReviewAndInitialiseCollateData()
        {
            // Arrange
            await Arrange("ModellingComplete");

            // Act 
            await modellingReview.ExecuteAsync(DipStateStatus.Failed, collateData);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Initialised);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task ModellingReviewComplete_CompleteModellingReviewAndTransitionToAdjustmentCheckAndThenTransitionToAdjustment()
        {
            // Arrange
            await Arrange("ModellingComplete");

            adjustmentCheck.AddActionAsync(DipStateActionType.Entry,
                AsyncTestMethods.AsyncAutoEnrtyActionTransitionToAdjustments);

            // Act 
            await modellingReview.ExecuteAsync(DipStateStatus.Completed, adjustmentCheck);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Initialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task AdjustmentComplete_CompleteAdjustmentAndTransitionToFinalReview()
        {
            // Arrange
            await Arrange("ModellingComplete");

            adjustmentCheck.AddActionAsync(DipStateActionType.Entry,
                AsyncTestMethods.AsyncAutoEnrtyActionTransitionToAdjustments);

            // Act 
            await modellingReview.ExecuteAsync(DipStateStatus.Completed, adjustmentCheck);

            await adjustments.ExecuteAsync(DipStateStatus.Completed);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Completed);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Initialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task ModellingReviewComplete_CompleteModellingReviewAndTransitionToAdjustmentCheckAndThenTransitionToFinalReview()
        {
            // Arrange
            await Arrange("ModellingComplete");

            adjustmentCheck.AddActionAsync(DipStateActionType.Entry,
                AsyncTestMethods.AsyncAutoEnrtyActionTransitionToFinalReview);

            // Act 
            await modellingReview.ExecuteAsync(DipStateStatus.Completed, adjustmentCheck);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Initialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task FinalReviewFailToModelling_ResetFinalReviewAndInitialiseModelling()
        {
            // Arrange
            await Arrange("ModellingReviewComplete");

            // Act 
            await finalReview.ExecuteAsync(DipStateStatus.Failed, modelling);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task FinalReviewFailToCollateData_FailFinalReviewAndInitialiseCollateData()
        {
            // Arrange
            await Arrange("ModellingReviewComplete");

            // Act 
            await finalReview.ExecuteAsync(DipStateStatus.Failed, collateData);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Initialised);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task FinalReviewComplete_CompleteFinalReviewAndCommunicationUpdateNotComplete_FinalCommunicationNotInitialised()
        {
            // Arrange
            await Arrange("ModellingReviewComplete");

            // Act 
            await finalReview.ExecuteAsync(DipStateStatus.Completed, finalCommunication);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Completed);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);
        }

        [TestMethod]
        public async Task FinalReviewCompleteAfterCompletedCommunicationUpdate_FinalReviewCompletedAndFinalCommunicationInitialised()
        {
            // Arrange
            await Arrange("ModellingReviewComplete");

            await communicationUpdate.ExecuteAsync(DipStateStatus.Completed);

            // Act 
            await finalReview.ExecuteAsync(DipStateStatus.Completed, finalCommunication);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Completed);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Initialised);
        }

        [TestMethod]
        public async Task FinalCommunicationComplete_CompleteFinalCommunicationAndCompletePricingWorkflow()
        {
            // Arrange
            await Arrange("FinalReviewComplete");

            // Act 
            await finalCommunication.ExecuteAsync(DipStateStatus.Completed);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.Completed);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Completed);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Completed);
        }

        [TestMethod]
        public async Task PricingWorkflowCompleteThenReset_ResetPricingWorkflow()
        {
            // Arrange
            await Arrange(String.Empty);

            // Act 
            pricingWorkflow.Reset();

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(collateData.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);
        }

        private async Task Arrange(string instruction)
        {
            await pricingWorkflow.ExecuteAsync(DipStateStatus.Initialised);

            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.Initialised);
            Assert.AreEqual(collateData.Status, DipStateStatus.Initialised);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);

            if (instruction.Equals("PricingWorkflowInitialised"))
            {
                return;
            }

            await collateData.ExecuteAsync(DipStateStatus.Completed);

            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);

            if (instruction.Equals("CollateDataComplete"))
            {
                return;
            }

            await modelling1.ExecuteAsync(DipStateStatus.Completed);
            await modelling2.ExecuteAsync(DipStateStatus.Completed);

            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Initialised);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);

            if (instruction.Equals("ModellingComplete"))
            {
                return;
            }

            adjustmentCheck.AddActionAsync(DipStateActionType.Entry,
                AsyncTestMethods.AsyncAutoEnrtyActionTransitionToFinalReview);

            await modellingReview.ExecuteAsync(DipStateStatus.Completed, adjustmentCheck);

            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Initialised);
            Assert.AreEqual(modelling.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Initialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);

            if (instruction.Equals("ModellingReviewComplete"))
            {
                return;
            }

            await communicationUpdate.ExecuteAsync(DipStateStatus.Completed);

            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Initialised);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Uninitialised);

            await finalReview.ExecuteAsync(DipStateStatus.Completed, finalCommunication);

            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.InProgress);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Completed);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Initialised);

            if (instruction.Equals("FinalReviewComplete"))
            {
                return;
            }

            await finalCommunication.ExecuteAsync(DipStateStatus.Completed);

            Assert.AreEqual(pricingWorkflow.Status, DipStateStatus.Completed);
            Assert.AreEqual(collateData.Status, DipStateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling1.Status, DipStateStatus.Completed);
            Assert.AreEqual(modelling2.Status, DipStateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, DipStateStatus.Completed);
            Assert.AreEqual(adjustments.Status, DipStateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, DipStateStatus.Completed);
            Assert.AreEqual(finalCommunication.Status, DipStateStatus.Completed);
        }
    }
}
