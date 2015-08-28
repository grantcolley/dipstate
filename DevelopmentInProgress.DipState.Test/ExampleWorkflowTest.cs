using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class ExampleWorkflowTest
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
            collateData = new State(1100, "Collate Data");
            communicationUpdate = new State(1200, "Communication Update");
            modelling = new State(1300, "Modelling");
            modelling1 = new State(1310, "Modelling 1");
            modelling2 = new State(1320, "Modelling 2");
            modellingReview = new State(1400, "Modelling Review");
            adjustmentCheck = new State(1500, "Adjustment Check", StateType.Auto);
            adjustments = new State(1600, "Adjustments");
            finalReview = new State(1700, "Final Review");
            finalCommunication = new State(1800, "Final Communication");

            pricingWorkflow
                .AddSubState(collateData, true)
                .AddSubState(communicationUpdate)
                .AddSubState(modelling)
                .AddSubState(modellingReview)
                .AddSubState(adjustmentCheck)
                .AddSubState(adjustments, completionRequired: false)
                .AddSubState(finalReview)
                .AddSubState(finalCommunication);

            collateData
                .AddAction(StateActionType.OnEntry, TestMethods.TraceWrite)
                .AddAction(StateActionType.OnExit, TestMethods.TraceWrite)
                .AddTransition(modelling);

            communicationUpdate
                .AddAction(StateActionType.OnEntry, TestMethods.TraceWrite)
                .AddAction(StateActionType.OnExit, TestMethods.TraceWrite)
                .AddDependency(collateData, true);

            modelling
                .AddAction(StateActionType.OnEntry, TestMethods.TraceWrite)
                .AddAction(StateActionType.OnExit, TestMethods.TraceWrite)
                .AddSubState(modelling1, true)
                .AddSubState(modelling2, true)
                .AddTransition(modellingReview, true);

            modellingReview
                .AddAction(StateActionType.OnEntry, TestMethods.TraceWrite)
                .AddAction(StateActionType.OnExit, TestMethods.TraceWrite)
                .AddTransition(adjustmentCheck)
                .AddTransition(modelling)
                .AddTransition(collateData);

            adjustmentCheck
                .AddAction(StateActionType.OnEntry, TestMethods.TraceWrite)
                .AddAction(StateActionType.OnExit, TestMethods.TraceWrite)
                .AddTransition(adjustments)
                .AddTransition(finalReview);

            adjustments
                .AddAction(StateActionType.OnEntry, TestMethods.TraceWrite)
                .AddAction(StateActionType.OnExit, TestMethods.TraceWrite)
                .AddTransition(finalReview);

            finalReview
                .AddAction(StateActionType.OnEntry, TestMethods.TraceWrite)
                .AddAction(StateActionType.OnExit, TestMethods.TraceWrite)
                .AddTransition(finalCommunication)
                .AddTransition(modelling)
                .AddTransition(collateData);

            finalCommunication
                .AddAction(StateActionType.OnEntry, TestMethods.TraceWrite)
                .AddAction(StateActionType.OnExit, TestMethods.TraceWrite)
                .AddDependency(communicationUpdate, true)
                .AddDependency(finalReview);
        }

        [TestMethod]
        public void ResetCollateData_CollateDateUninitialised()
        {
            // Arrange
            Arrange("PricingWorkflowInitialised");

            // Act 
            collateData.Execute(StateExecutionType.Reset);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Uninitialised);
            Assert.AreEqual(collateData.Status, StateStatus.Uninitialised);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Uninitialised);
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
        public void CollateDataComplete_CompleteCollateDataAndInitialiseModelling()
        {
            // Arrange
            Arrange("PricingWorkflowInitialised");

            // Act
            collateData.Execute(modelling);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialised);
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
        public void ModellingFailBothSubTasks_ResetModellingSubStates()
        {
            // Arrange
            Arrange("CollateDataComplete");

            // Act
            modelling1.Execute(StateExecutionType.Reset);
            modelling2.Execute(StateExecutionType.Reset);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialised);
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
        public void ModellingCompleteOneSubTaskAndFailTheOther_ModellingRemainsInProgress()
        {
            // Arrange
            Arrange("CollateDataComplete");

            // Act 
            modelling1.Execute(StateExecutionType.Complete);
            modelling2.Execute(StateExecutionType.Reset);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialised);
            Assert.AreEqual(modelling.Status, StateStatus.InProgress);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public void ModellingCompleteBothSubTasks_CompleteModellingInitialiseModellingReview()
        {
            // Arrange
            Arrange("CollateDataComplete");

            // Act 
            modelling1.Execute(StateExecutionType.Complete);
            modelling2.Execute(StateExecutionType.Complete);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialised);
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
        public void ModellingReviewFail_ResetModellingReview()
        {
            // Arrange
            Arrange("ModellingComplete");

            // Act 
            modellingReview.Execute(StateExecutionType.Reset);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialised);
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
        public void ModellingReviewTransitionToModellingWithoutComplete_ResetModellingReviewAndInitialiseModelling()
        {
            // Arrange
            Arrange("ModellingComplete");

            // Act 
            modellingReview.Execute(modelling, true);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialised);
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
        public void ModellingReviewFailToCollateData_ResetModellingReviewAndInitialiseCollateData()
        {
            // Arrange
            Arrange("ModellingComplete");

            // Act 
            modellingReview.Execute(collateData, true);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Initialised);
            Assert.AreEqual(collateData.Status, StateStatus.Initialised);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Uninitialised);
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
        public void ModellingReviewComplete_CompleteModellingReviewAndTransitionToAdjustmentCheckAndThenTransitionToAdjustment()
        {
            // Arrange
            Arrange("ModellingComplete");

            adjustmentCheck.AddAction(StateActionType.OnEntry, (a) =>
            {
                a.Transition = adjustments;
            });

            // Act 
            modellingReview.Execute(adjustmentCheck);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialised);
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
        public void AdjustmentComplete_CompleteAdjustmentAndTransitionToFinalReview()
        {
            // Arrange
            Arrange("ModellingComplete");

            adjustmentCheck.AddAction(StateActionType.OnEntry, (a) =>
            {
                a.Transition = adjustments;
            });

            // Act 
            modellingReview.Execute(adjustmentCheck);

            adjustments.Execute(finalReview);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialised);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
            Assert.AreEqual(adjustments.Status, StateStatus.Completed);
            Assert.AreEqual(finalReview.Status, StateStatus.Initialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);
        }

        [TestMethod]
        public void ModellingReviewComplete_CompleteModellingReviewAndTransitionToAdjustmentCheckAndThenTransitionToFinalReview()
        {
            // Arrange
            Arrange("ModellingComplete");

            adjustmentCheck.AddAction(StateActionType.OnEntry, (a) =>
            {
                a.Transition = finalReview;
            });

            // Act 
            modellingReview.Execute(adjustmentCheck);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialised);
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
        public void FinalReviewFailToModelling_ResetFinalReviewAndInitialiseModelling()
        {
            // Arrange
            Arrange("ModellingReviewComplete");

            // Act 
            finalReview.Execute(modelling, true);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialised);
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
        public void FinalReviewFailToCollateData_FailFinalReviewAndInitialiseCollateData()
        {
            // Arrange
            Arrange("ModellingReviewComplete");

            // Act 
            finalReview.Execute(collateData, true);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Initialised);
            Assert.AreEqual(collateData.Status, StateStatus.Initialised);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Uninitialised);
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
        public void FinalReviewComplete_CompleteFinalReviewAndCommunicationUpdateNotComplete_FinalCommunicationNotInitialised()
        {
            // Arrange
            Arrange("ModellingReviewComplete");

            // Act 
            finalReview.Execute(finalCommunication);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialised);
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
        public void FinalReviewCompleteAfterCompletedCommunicationUpdate_FinalReviewCompletedAndFinalCommunicationInitialised()
        {
            // Arrange
            Arrange("ModellingReviewComplete");

            communicationUpdate.Execute(StateExecutionType.Complete);

            // Act 
            finalReview.Execute(finalCommunication);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Completed);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Initialised);
        }

        [TestMethod]
        public void FinalCommunicationComplete_CompleteFinalCommunicationAndCompletePricingWorkflow()
        {
            // Arrange
            Arrange("FinalReviewComplete");

            // Act 
            finalCommunication.Execute(StateExecutionType.Complete);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Completed);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Completed);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Completed);
        }

        [TestMethod]
        public void PricingWorkflowCompleteThenReset_ResetPricingWorkflow()
        {
            // Arrange
            Arrange(String.Empty);

            // Act 
            pricingWorkflow.Execute(StateExecutionType.Reset);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Uninitialised);
            Assert.AreEqual(collateData.Status, StateStatus.Uninitialised);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modelling.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modelling1.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modelling2.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);
        }

        private void Arrange(string instruction)
        {
            pricingWorkflow.Execute(StateExecutionType.Initialise);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Initialised);
            Assert.AreEqual(collateData.Status, StateStatus.Initialised);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modelling.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modelling1.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modelling2.Status, StateStatus.Uninitialised);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);

            if (instruction.Equals("PricingWorkflowInitialised"))
            {
                return;
            }

            collateData.Execute(modelling);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialised);
            Assert.AreEqual(modelling.Status, StateStatus.Initialised);
            Assert.AreEqual(modelling1.Status, StateStatus.Initialised);
            Assert.AreEqual(modelling2.Status, StateStatus.Initialised);
            Assert.AreEqual(modellingReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);

            if (instruction.Equals("CollateDataComplete"))
            {
                return;
            }

            modelling1.Execute(StateExecutionType.Complete);
            modelling2.Execute(StateExecutionType.Complete);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialised);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Initialised);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);

            if (instruction.Equals("ModellingComplete"))
            {
                return;
            }

            adjustmentCheck.AddAction(StateActionType.OnEntry, (a) =>
            {
                a.Transition = finalReview;
            });

            modellingReview.Execute(adjustmentCheck);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Initialised);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Initialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);

            if (instruction.Equals("ModellingReviewComplete"))
            {
                return;
            }

            communicationUpdate.Execute(StateExecutionType.Complete);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Initialised);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Uninitialised);

            finalReview.Execute(finalCommunication);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Completed);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Initialised);

            if (instruction.Equals("FinalReviewComplete"))
            {
                return;
            }

            finalCommunication.Execute(StateExecutionType.Complete);

            Assert.AreEqual(pricingWorkflow.Status, StateStatus.Completed);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(communicationUpdate.Status, StateStatus.Completed);
            Assert.AreEqual(modelling.Status, StateStatus.Completed);
            Assert.AreEqual(modelling1.Status, StateStatus.Completed);
            Assert.AreEqual(modelling2.Status, StateStatus.Completed);
            Assert.AreEqual(modellingReview.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentCheck.Status, StateStatus.Completed);
            Assert.AreEqual(adjustments.Status, StateStatus.Uninitialised);
            Assert.AreEqual(finalReview.Status, StateStatus.Completed);
            Assert.AreEqual(finalCommunication.Status, StateStatus.Completed);
        }
    }
}
