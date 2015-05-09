using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class ExampleWorkflowTest
    {
        private DipStateEngine dipStateEngine;
        private IDipState pricingWorkflow;
        private IDipState collateData;
        private IDipState communicationUpdate;
        private IDipState modelling;
        private IDipState modelling1;
        private IDipState modelling2;
        private IDipState modellingReview;
        private IDipState adjustmentCheck;
        private IDipState adjustments;
        private IDipState finalReview;
        private IDipState finalCommunication;

        [TestInitialize]
        public void Initialise()
        {
            dipStateEngine = new DipStateEngine();

            pricingWorkflow = new DipState(1000, "Pricing Workflow");
            collateData = new DipState(1100, "Collate Date", initialiseWithParent: true);
            communicationUpdate = new DipState(1200, "Communication Update");
            modelling = new DipState(1300, "Modelling");
            modelling1 = new DipState(1310, "Modelling 1", initialiseWithParent: true);
            modelling2 = new DipState(1320, "Modelling 2", initialiseWithParent: true);
            modellingReview = new DipState(1400, "Modelling Review");
            adjustmentCheck = new DipState(1500, "Adjustment Check", DipStateType.Auto);
            adjustments = new DipState(1600, "Adjustments");
            finalReview = new DipState(1700, "Final Review");
            finalCommunication = new DipState(1800, "Final Communication");

            pricingWorkflow
                .AddSubState(collateData)
                .AddSubState(communicationUpdate)
                .AddSubState(modelling)
                .AddSubState(modellingReview)
                .AddSubState(adjustmentCheck)
                .AddSubState(adjustments)
                .AddSubState(finalReview)
                .AddTransition(finalCommunication);

            collateData
                .AddAction(DipStateActionType.Entry, TraceWrite)
                .AddAction(DipStateActionType.Exit, TraceWrite)
                .AddTransition(modelling);

            communicationUpdate
                .AddAction(DipStateActionType.Entry, TraceWrite)
                .AddAction(DipStateActionType.Exit, TraceWrite)
                .AddDependency(collateData);

            modelling
                .AddAction(DipStateActionType.Entry, TraceWrite)
                .AddAction(DipStateActionType.Exit, TraceWrite)
                .AddSubState(modelling1)
                .AddSubState(modelling2)
                .AddTransition(modellingReview);

            modellingReview
                .AddAction(DipStateActionType.Entry, TraceWrite)
                .AddAction(DipStateActionType.Exit, TraceWrite)
                .AddTransition(adjustmentCheck)
                .AddTransition(modelling)
                .AddTransition(collateData);

            adjustmentCheck
                .AddAction(DipStateActionType.Entry, TraceWrite)
                .AddAction(DipStateActionType.Exit, TraceWrite)
                .AddTransition(adjustments)
                .AddTransition(finalReview);

            adjustments
                .AddAction(DipStateActionType.Entry, TraceWrite)
                .AddAction(DipStateActionType.Exit, TraceWrite)
                .AddTransition(finalReview);

            finalReview
                .AddAction(DipStateActionType.Entry, TraceWrite)
                .AddAction(DipStateActionType.Exit, TraceWrite)
                .AddTransition(finalCommunication)
                .AddTransition(modelling)
                .AddTransition(collateData);

            finalCommunication
                .AddAction(DipStateActionType.Entry, TraceWrite)
                .AddAction(DipStateActionType.Exit, TraceWrite)
                .AddDependency(communicationUpdate);
        }

        [TestMethod]
        public void CollateDataFail_ResetCollateDateAndPricingWorkflow()
        {
            // Arrange
            Arrange("PricingWorkflowInitialised");

            // Act 
            dipStateEngine.Run(collateData, DipStateStatus.Failed);

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
        public void CollateDataComplete_CompleteCollateDataAndInitialiseModelling()
        {
            // Arrange
            Arrange("PricingWorkflowInitialised");

            // Act
            dipStateEngine.Run(collateData, DipStateStatus.Completed);

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
        public void ModellingFailBothSubTasks_ResetModellingAndItsSubStates()
        {
            // Arrange
            Arrange("CollateDataComplete");

            // Act
            dipStateEngine.Run(modelling1, DipStateStatus.Failed);
            dipStateEngine.Run(modelling2, DipStateStatus.Failed);

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
        public void ModellingCompleteOneSubTaskAndFailTheOther_ModellingRemainsInProgress()
        {
            // Arrange
            Arrange("CollateDataComplete");

            // Act 
            dipStateEngine.Run(modelling1, DipStateStatus.Completed);
            dipStateEngine.Run(modelling2, DipStateStatus.Failed);

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
        public void ModellingCompleteBothSubTasks_CompleteModellingInitialiseModellingReview()
        {
            // Arrange
            Arrange("CollateDataComplete");

            // Act 
            dipStateEngine.Run(modelling1, DipStateStatus.Completed);
            dipStateEngine.Run(modelling2, DipStateStatus.Completed);

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
        public void ModellingReviewFail_ResetModellingReview()
        {
            // Arrange
            Arrange("ModellingComplete");

            // Act 
            dipStateEngine.Run(modellingReview, DipStateStatus.Failed);

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
        public void ModellingReviewFailToModelling_ResetModellingReviewAndInitialiseModelling()
        {
            // Arrange
            Arrange("ModellingComplete");

            // Act 
            dipStateEngine.Run(modellingReview, DipStateStatus.Failed, modelling);

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
        public void ModellingReviewFailToCollateData_ResetModellingReviewAndInitialiseCollateData()
        {
            // Arrange
            Arrange("ModellingComplete");

            // Act 
            dipStateEngine.Run(modellingReview, DipStateStatus.Failed, collateData);

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
        public void ModellingReviewComplete_CompleteModellingReviewAndTransitionToAdjustmentCheckAndThenTransitionToAdjustment()
        {
            // Arrange
            Arrange("ModellingComplete");

            adjustmentCheck.AddAction(DipStateActionType.Entry, (a) =>
            {
                a.Transition = adjustments;
            });

            // Act 
            dipStateEngine.Run(modellingReview, DipStateStatus.Completed, adjustmentCheck);

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
        public void AdjustmentComplete_CompleteAdjustmentAndTransitionToFinalReview()
        {
            // Arrange
            Arrange("ModellingComplete");

            adjustmentCheck.AddAction(DipStateActionType.Entry, (a) =>
            {
                a.Transition = adjustments;
            });

            // Act 
            dipStateEngine.Run(modellingReview, DipStateStatus.Completed, adjustmentCheck);

            dipStateEngine.Run(adjustments, DipStateStatus.Completed);

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
        public void ModellingReviewComplete_CompleteModellingReviewAndTransitionToAdjustmentCheckAndThenTransitionToFinalReview()
        {
            // Arrange
            Arrange("ModellingComplete");

            adjustmentCheck.AddAction(DipStateActionType.Entry, (a) =>
            {
                a.Transition = finalReview;
            });

            // Act 
            dipStateEngine.Run(modellingReview, DipStateStatus.Completed, adjustmentCheck);

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
        public void FinalReviewFailToModelling_ResetFinalReviewAndInitialiseModelling()
        {
            // Arrange
            adjustmentCheck.AddAction(DipStateActionType.Entry, (a) =>
            {
                a.Transition = finalReview;
            });

            Arrange("ModellingReviewComplete");

            // Act 
            dipStateEngine.Run(finalReview, DipStateStatus.Failed, modelling);

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
        public void FinalReviewFailToCollateData_FailFinalReviewAndInitialiseCollateData()
        {
            // Arrange
            Arrange("ModellingReviewComplete");

            // Act 

            // Assert
            throw new NotImplementedException();
        }

        [TestMethod]
        public void FinalReviewComplete_CompleteFinalReviewAndCommunicationUpdateNotComplete_FinalCommunicationNotInitialised()
        {
            // Arrange
            Arrange("FinalReviewComplete");

            // Act 

            // Assert
            throw new NotImplementedException();
        }

        [TestMethod]
        public void CommunicationUpdateComplete_CompleteCommunicationUpdateAndFinalCommunicationInitialised()
        {
            // Arrange
            Arrange("FinalReviewComplete");

            // Act 

            // Assert
            throw new NotImplementedException();
        }

        [TestMethod]
        public void FinalReviewComplete_CompleteFinalReviewAndFinalCommunicationInitialised()
        {
            // Arrange
            Arrange("FinalReviewComplete");

            // Act 

            // Assert
            throw new NotImplementedException();
        }

        [TestMethod]
        public void FinalCommunicationComplete_CompleteFinalCommunicationAndCompletePricingWorkflow()
        {
            // Arrange
            Arrange(String.Empty);

            // Act 

            // Assert
            throw new NotImplementedException();
        }

        [TestMethod]
        public void PricingWorkflowCompleteThenReset_ResetPricingWorkflow()
        {
            // Arrange
            Arrange(String.Empty);

            // Act 

            // Assert
            throw new NotImplementedException();
        }

        private void TraceWrite(IDipState state)
        {
            Trace.WriteLine(String.Format("{0} {1} - {2}", state.Id, state.Name, state.Status));
            foreach (var logEntry in state.Log)
            {
                Trace.WriteLine(String.Format("     {0}", logEntry.Message));
            }
        }

        private void Arrange(string instruction)
        {
            dipStateEngine.Run(pricingWorkflow, DipStateStatus.Initialised);

            if (instruction.Equals("PricingWorkflowInitialised"))
            {
                return;
            }

            dipStateEngine.Run(collateData, DipStateStatus.Completed);

            if (instruction.Equals("CollateDataComplete"))
            {
                return;
            }

            dipStateEngine.Run(modelling1, DipStateStatus.Completed);

            dipStateEngine.Run(modelling2, DipStateStatus.Completed);

            if (instruction.Equals("ModellingComplete"))
            {
                return;
            }

            dipStateEngine.Run(modellingReview, DipStateStatus.Completed);

            if (instruction.Equals("ModellingReviewComplete"))
            {
                return;
            }

            dipStateEngine.Run(finalReview, DipStateStatus.Completed);

            if (instruction.Equals("FinalReviewComplete"))
            {
                return;
            }

            dipStateEngine.Run(finalCommunication, DipStateStatus.Completed);
        }
    }
}
