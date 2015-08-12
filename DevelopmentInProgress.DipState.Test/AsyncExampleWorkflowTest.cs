using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class AsyncExampleWorkflowTest
    {
        private State pricingWorkflow;
        private State collateData;
        private State communicationUpdate;
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
            collateData = new State(1100, "Collate Data", true);
            communicationUpdate = new State(1200, "Communication Update");
            modelling = new State(1300, "Modelling");
            modelling1 = new State(1310, "Modelling 1", true);
            modelling2 = new State(1320, "Modelling 2", true);
            modellingReview = new State(1400, "Modelling Review");
            adjustmentCheck = new State(1500, "Adjustment Check", type: StateType.Auto);
            adjustments = new State(1600, "Adjustments");
            finalReview = new State(1700, "Final Review");
            finalCommunication = new State(1800, "Final Communication", canCompleteParent: true);

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
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddTransition(modelling);

            communicationUpdate
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddDependency(collateData, true);

            modelling
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddSubState(modelling1)
                .AddSubState(modelling2)
                .AddTransition(modellingReview);

            modellingReview
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddTransition(adjustmentCheck)
                .AddTransition(modelling)
                .AddTransition(collateData);

            adjustmentCheck
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddTransition(adjustments)
                .AddTransition(finalReview);

            adjustments
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddTransition(finalReview);

            finalReview
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddTransition(finalCommunication)
                .AddTransition(modelling)
                .AddTransition(collateData);

            finalCommunication
                .AddActionAsync(StateActionType.Entry, AsyncTestMethods.TraceWrite)
                .AddActionAsync(StateActionType.Exit, AsyncTestMethods.TraceWrite)
                .AddDependency(communicationUpdate);
        }

        [TestMethod]
        public async Task CollateDataFail_ResetCollateDate()
        {
            // Arrange
            await Arrange("PricingWorkflowInitialised");

            // Act 
            await collateData.ExecuteAsync(StateStatus.Fail);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Initialise);
            Assert.AreEqual(collateData.Status, StateStatus.Uninitialise);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Uninitialise);
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
        public async Task CollateDataComplete_CompleteCollateDataAndInitialiseModelling()
        {
            // Arrange
            await Arrange("PricingWorkflowInitialised");

            // Act
            await collateData.ExecuteAsync(StateStatus.Complete);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialise);
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
        public async Task ModellingFailBothSubTasks_ResetModellingSubStates()
        {
            // Arrange
            await Arrange("CollateDataComplete");

            // Act
            await modelling1.ExecuteAsync(StateStatus.Fail);
            await modelling2.ExecuteAsync(StateStatus.Fail);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling1.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modelling2.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);
        }

        [TestMethod]
        public async Task ModellingCompleteOneSubTaskAndFailTheOther_ModellingRemainsInProgress()
        {
            // Arrange
            await Arrange("CollateDataComplete");

            // Act 
            await modelling1.ExecuteAsync(StateStatus.Complete);
            await modelling2.ExecuteAsync(StateStatus.Fail);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling.Status, StateStatus.InProgress);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);
        }

        [TestMethod]
        public async Task ModellingCompleteBothSubTasks_CompleteModellingInitialiseModellingReview()
        {
            // Arrange
            await Arrange("CollateDataComplete");

            // Act 
            await modelling1.ExecuteAsync(StateStatus.Complete);
            await modelling2.ExecuteAsync(StateStatus.Complete);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialise);
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
        public async Task ModellingReviewFail_ResetModellingReview()
        {
            // Arrange
            await Arrange("ModellingComplete");

            // Act 
            await modellingReview.ExecuteAsync(StateStatus.Fail);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialise);
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
        public async Task ModellingReviewFailToModelling_ResetModellingReviewAndInitialiseModelling()
        {
            // Arrange
            await Arrange("ModellingComplete");

            // Act 
            await modellingReview.ExecuteAsync(StateStatus.Fail, modelling);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialise);
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
        public async Task ModellingReviewFailToCollateData_ResetModellingReviewAndInitialiseCollateData()
        {
            // Arrange
            await Arrange("ModellingComplete");

            // Act 
            await modellingReview.ExecuteAsync(StateStatus.Fail, collateData);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Initialise);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Uninitialise);
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
        public async Task ModellingReviewComplete_CompleteModellingReviewAndTransitionToAdjustmentCheckAndThenTransitionToAdjustment()
        {
            // Arrange
            await Arrange("ModellingComplete");

            adjustmentCheck.AddActionAsync(StateActionType.Entry,
                AsyncTestMethods.AsyncAutoEnrtyActionTransitionToAdjustments);

            // Act 
            await modellingReview.ExecuteAsync(StateStatus.Complete, adjustmentCheck);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialise);
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
        public async Task AdjustmentComplete_CompleteAdjustmentAndTransitionToFinalReview()
        {
            // Arrange
            await Arrange("ModellingComplete");

            adjustmentCheck.AddActionAsync(StateActionType.Entry,
                AsyncTestMethods.AsyncAutoEnrtyActionTransitionToAdjustments);

            // Act 
            await modellingReview.ExecuteAsync(StateStatus.Complete, adjustmentCheck);

            await adjustments.ExecuteAsync(StateStatus.Complete);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
            Assert.AreEqual(adjustments.Status, StateStatus.Complete);
            Assert.AreEqual(finalReview.Status, StateStatus.Initialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);
        }

        [TestMethod]
        public async Task ModellingReviewComplete_CompleteModellingReviewAndTransitionToAdjustmentCheckAndThenTransitionToFinalReview()
        {
            // Arrange
            await Arrange("ModellingComplete");

            adjustmentCheck.AddActionAsync(StateActionType.Entry,
                AsyncTestMethods.AsyncAutoEnrtyActionTransitionToFinalReview);

            // Act 
            await modellingReview.ExecuteAsync(StateStatus.Complete, adjustmentCheck);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialise);
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
        public async Task FinalReviewFailToModelling_ResetFinalReviewAndInitialiseModelling()
        {
            // Arrange
            await Arrange("ModellingReviewComplete");

            // Act 
            await finalReview.ExecuteAsync(StateStatus.Fail, modelling);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialise);
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
        public async Task FinalReviewFailToCollateData_FailFinalReviewAndInitialiseCollateData()
        {
            // Arrange
            await Arrange("ModellingReviewComplete");

            // Act 
            await finalReview.ExecuteAsync(StateStatus.Fail, collateData);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Initialise);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Uninitialise);
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
        public async Task FinalReviewComplete_CompleteFinalReviewAndCommunicationUpdateNotComplete_FinalCommunicationNotInitialised()
        {
            // Arrange
            await Arrange("ModellingReviewComplete");

            // Act 
            await finalReview.ExecuteAsync(StateStatus.Complete, finalCommunication);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialise);
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
        public async Task FinalReviewCompleteAfterCompletedCommunicationUpdate_FinalReviewCompletedAndFinalCommunicationInitialised()
        {
            // Arrange
            await Arrange("ModellingReviewComplete");

            await communicationUpdate.ExecuteAsync(StateStatus.Complete);

            // Act 
            await finalReview.ExecuteAsync(StateStatus.Complete, finalCommunication);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Complete);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Initialise);
        }

        [TestMethod]
        public async Task FinalCommunicationComplete_CompleteFinalCommunicationAndCompletePricingWorkflow()
        {
            // Arrange
            await Arrange("FinalReviewComplete");

            // Act 
            await finalCommunication.ExecuteAsync(StateStatus.Complete);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Complete);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Complete);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Complete);
        }

        [TestMethod]
        public async Task PricingWorkflowCompleteThenReset_ResetPricingWorkflow()
        {
            // Arrange
            await Arrange(String.Empty);

            // Act 
            await pricingWorkflow.ResetAsync();

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Uninitialise);
            Assert.AreEqual(collateData.Status, StateStatus.Uninitialise);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modelling.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modelling1.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modelling2.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);
        }

        private async Task Arrange(string instruction)
        {
            await pricingWorkflow.ExecuteAsync(StateStatus.Initialise);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Initialise);
            Assert.AreEqual(collateData.Status, StateStatus.Initialise);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modelling.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modelling1.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modelling2.Status, StateStatus.Uninitialise);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);

            if (instruction.Equals("PricingWorkflowInitialised"))
            {
                return;
            }

            await collateData.ExecuteAsync(StateStatus.Complete);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling1.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling2.Status, StateStatus.Initialise);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);

            if (instruction.Equals("CollateDataComplete"))
            {
                return;
            }

            await modelling1.ExecuteAsync(StateStatus.Complete);
            await modelling2.ExecuteAsync(StateStatus.Complete);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Initialise);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialise);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);

            if (instruction.Equals("ModellingComplete"))
            {
                return;
            }

            adjustmentCheck.AddActionAsync(StateActionType.Entry,
                AsyncTestMethods.AsyncAutoEnrtyActionTransitionToFinalReview);

            await modellingReview.ExecuteAsync(StateStatus.Complete, adjustmentCheck);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialise);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Initialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);

            if (instruction.Equals("ModellingReviewComplete"))
            {
                return;
            }

            await communicationUpdate.ExecuteAsync(StateStatus.Complete);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Initialise);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialise);

            await finalReview.ExecuteAsync(StateStatus.Complete, finalCommunication);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Complete);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Initialise);

            if (instruction.Equals("FinalReviewComplete"))
            {
                return;
            }

            await finalCommunication.ExecuteAsync(StateStatus.Complete);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Complete);
            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Complete);
            Assert.AreEqual(modelling.Status, StateStatus.Complete);
            Assert.AreEqual(modelling1.Status, StateStatus.Complete);
            Assert.AreEqual(modelling2.Status, StateStatus.Complete);
            Assert.AreEqual(modellingReview.Status, StateStatus.Complete);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Complete);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialise);
            Assert.AreEqual(finalReview.Status, StateStatus.Complete);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Complete);
        }
    }
}
