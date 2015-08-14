using System;
using System.Diagnostics;
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
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddTransition(modelling);

            communicationUpdate
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddDependency(collateData, true);

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
                .AddTransition(modelling)
                .AddTransition(collateData);

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
                .AddTransition(finalCommunication)
                .AddTransition(modelling)
                .AddTransition(collateData);

            finalCommunication
                .AddAction(StateActionType.Entry, TestMethods.TraceWrite)
                .AddAction(StateActionType.Exit, TestMethods.TraceWrite)
                .AddDependency(communicationUpdate);
        }

        [TestMethod]
        public void CollateDataFail_ResetCollateDate()
        {
            // Arrange
            Arrange("PricingWorkflowInitialised");

            // Act 
            collateData.Execute(StateStatus.Fail);

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
        public void CollateDataComplete_CompleteCollateDataAndInitialiseModelling()
        {
            // Arrange
            Arrange("PricingWorkflowInitialised");

            // Act
            collateData.Execute(StateStatus.Complete);

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
        public void ModellingFailBothSubTasks_ResetModellingSubStates()
        {
            // Arrange
            Arrange("CollateDataComplete");

            // Act
            modelling1.Execute(StateStatus.Fail);
            modelling2.Execute(StateStatus.Fail);

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
        public void ModellingCompleteOneSubTaskAndFailTheOther_ModellingRemainsInProgress()
        {
            // Arrange
            Arrange("CollateDataComplete");

            // Act 
            modelling1.Execute(StateStatus.Complete);
            modelling2.Execute(StateStatus.Fail);

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
        public void ModellingCompleteBothSubTasks_CompleteModellingInitialiseModellingReview()
        {
            // Arrange
            Arrange("CollateDataComplete");

            // Act 
            modelling1.Execute(StateStatus.Complete);
            modelling2.Execute(StateStatus.Complete);

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
        public void ModellingReviewFail_ResetModellingReview()
        {
            // Arrange
            Arrange("ModellingComplete");

            // Act 
            modellingReview.Execute(StateStatus.Fail);

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
        public void ModellingReviewFailToModelling_ResetModellingReviewAndInitialiseModelling()
        {
            // Arrange
            Arrange("ModellingComplete");

            // Act 
            modellingReview.Execute(StateStatus.Fail, modelling);

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
        public void ModellingReviewFailToCollateData_ResetModellingReviewAndInitialiseCollateData()
        {
            // Arrange
            Arrange("ModellingComplete");

            // Act 
            modellingReview.Execute(StateStatus.Fail, collateData);

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
        public void ModellingReviewComplete_CompleteModellingReviewAndTransitionToAdjustmentCheckAndThenTransitionToAdjustment()
        {
            // Arrange
            Arrange("ModellingComplete");

            adjustmentCheck.AddAction(StateActionType.Entry, (a) =>
            {
                a.Transition = adjustments;
            });

            // Act 
            modellingReview.Execute(StateStatus.Complete, adjustmentCheck);

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
        public void AdjustmentComplete_CompleteAdjustmentAndTransitionToFinalReview()
        {
            // Arrange
            Arrange("ModellingComplete");

            adjustmentCheck.AddAction(StateActionType.Entry, (a) =>
            {
                a.Transition = adjustments;
            });

            // Act 
            modellingReview.Execute(StateStatus.Complete, adjustmentCheck);

            adjustments.Execute(StateStatus.Complete);

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
        public void ModellingReviewComplete_CompleteModellingReviewAndTransitionToAdjustmentCheckAndThenTransitionToFinalReview()
        {
            // Arrange
            Arrange("ModellingComplete");

            adjustmentCheck.AddAction(StateActionType.Entry, (a) =>
            {
                a.Transition = finalReview;
            });

            // Act 
            modellingReview.Execute(StateStatus.Complete, adjustmentCheck);

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
        public void FinalReviewFailToModelling_ResetFinalReviewAndInitialiseModelling()
        {
            // Arrange
            Arrange("ModellingReviewComplete");

            // Act 
            finalReview.Execute(StateStatus.Fail, modelling);

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
        public void FinalReviewFailToCollateData_FailFinalReviewAndInitialiseCollateData()
        {
            // Arrange
            Arrange("ModellingReviewComplete");

            // Act 
            finalReview.Execute(StateStatus.Fail, collateData);

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
        public void FinalReviewComplete_CompleteFinalReviewAndCommunicationUpdateNotComplete_FinalCommunicationNotInitialised()
        {
            // Arrange
            Arrange("ModellingReviewComplete");

            // Act 
            finalReview.Execute(StateStatus.Complete, finalCommunication);

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
        public void FinalReviewCompleteAfterCompletedCommunicationUpdate_FinalReviewCompletedAndFinalCommunicationInitialised()
        {
            // Arrange
            Arrange("ModellingReviewComplete");

            communicationUpdate.Execute(StateStatus.Complete);

            // Act 
            finalReview.Execute(StateStatus.Complete, finalCommunication);

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
        public void FinalCommunicationComplete_CompleteFinalCommunicationAndCompletePricingWorkflow()
        {
            // Arrange
            Arrange("FinalReviewComplete");

            // Act 
            finalCommunication.Execute(StateStatus.Complete);

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
        public void PricingWorkflowCompleteThenReset_ResetPricingWorkflow()
        {
            // Arrange
            Arrange(String.Empty);

            // Act 
            pricingWorkflow.Reset();

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

        private void Arrange(string instruction)
        {
            pricingWorkflow.Execute(StateStatus.Initialise);

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

            collateData.Execute(StateStatus.Complete);

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

            modelling1.Execute(StateStatus.Complete);
            modelling2.Execute(StateStatus.Complete);

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

            adjustmentCheck.AddAction(StateActionType.Entry, (a) =>
            {
                a.Transition = finalReview;
            });

            modellingReview.Execute(StateStatus.Complete, adjustmentCheck);

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

            communicationUpdate.Execute(StateStatus.Complete);

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

            finalReview.Execute(StateStatus.Complete, finalCommunication);

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

            finalCommunication.Execute(StateStatus.Complete);

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
