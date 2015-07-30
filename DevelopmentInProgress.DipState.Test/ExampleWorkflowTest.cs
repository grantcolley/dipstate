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
                .AddAction(StateActionType.Entry, TraceWrite)
                .AddAction(StateActionType.Exit, TraceWrite)
                .AddTransition(modelling);

            communicationUpdate
                .AddAction(StateActionType.Entry, TraceWrite)
                .AddAction(StateActionType.Exit, TraceWrite)
                .AddDependency(collateData, true);

            modelling
                .AddAction(StateActionType.Entry, TraceWrite)
                .AddAction(StateActionType.Exit, TraceWrite)
                .AddSubState(modelling1)
                .AddSubState(modelling2)
                .AddTransition(modellingReview);

            modellingReview
                .AddAction(StateActionType.Entry, TraceWrite)
                .AddAction(StateActionType.Exit, TraceWrite)
                .AddTransition(adjustmentCheck)
                .AddTransition(modelling)
                .AddTransition(collateData);

            adjustmentCheck
                .AddAction(StateActionType.Entry, TraceWrite)
                .AddAction(StateActionType.Exit, TraceWrite)
                .AddTransition(adjustments)
                .AddTransition(finalReview);

            adjustments
                .AddAction(StateActionType.Entry, TraceWrite)
                .AddAction(StateActionType.Exit, TraceWrite)
                .AddTransition(finalReview);

            finalReview
                .AddAction(StateActionType.Entry, TraceWrite)
                .AddAction(StateActionType.Exit, TraceWrite)
                .AddTransition(finalCommunication)
                .AddTransition(modelling)
                .AddTransition(collateData);

            finalCommunication
                .AddAction(StateActionType.Entry, TraceWrite)
                .AddAction(StateActionType.Exit, TraceWrite)
                .AddDependency(communicationUpdate);
        }

        [TestMethod]
        public void CollateDataFail_ResetCollateDateAndPricingWorkflow()
        {
            // Arrange
            Arrange("PricingWorkflowInitialised");

            // Act 
            collateData.Execute(StateStatus.Failed);

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
            collateData.Execute(StateStatus.Completed);

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
        public void ModellingFailBothSubTasks_ResetModellingAndItsSubStates()
        {
            // Arrange
            Arrange("CollateDataComplete");

            // Act
            modelling1.Execute(StateStatus.Failed);
            modelling2.Execute(StateStatus.Failed);

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
            modelling1.Execute(StateStatus.Completed);
            modelling2.Execute(StateStatus.Failed);

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
            modelling1.Execute(StateStatus.Completed);
            modelling2.Execute(StateStatus.Completed);

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
            modellingReview.Execute(StateStatus.Failed);

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
        public void ModellingReviewFailToModelling_ResetModellingReviewAndInitialiseModelling()
        {
            // Arrange
            Arrange("ModellingComplete");

            // Act 
            modellingReview.Execute(StateStatus.Failed, modelling);

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
            modellingReview.Execute(StateStatus.Failed, collateData);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Initialised);
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
        public void ModellingReviewComplete_CompleteModellingReviewAndTransitionToAdjustmentCheckAndThenTransitionToAdjustment()
        {
            // Arrange
            Arrange("ModellingComplete");

            adjustmentCheck.AddAction(StateActionType.Entry, (a) =>
            {
                a.Transition = adjustments;
            });

            // Act 
            modellingReview.Execute(StateStatus.Completed, adjustmentCheck);

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

            adjustmentCheck.AddAction(StateActionType.Entry, (a) =>
            {
                a.Transition = adjustments;
            });

            // Act 
            modellingReview.Execute(StateStatus.Completed, adjustmentCheck);

            adjustments.Execute(StateStatus.Completed);

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

            adjustmentCheck.AddAction(StateActionType.Entry, (a) =>
            {
                a.Transition = finalReview;
            });

            // Act 
            modellingReview.Execute(StateStatus.Completed, adjustmentCheck);

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
            finalReview.Execute(StateStatus.Failed, modelling);

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
            finalReview.Execute(StateStatus.Failed, collateData);

            // Assert
            Assert.AreEqual(pricingWorkflow.Status, StateStatus.InProgress);
            Assert.AreEqual(collateData.Status, StateStatus.Initialised);
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
        public void FinalReviewComplete_CompleteFinalReviewAndCommunicationUpdateNotComplete_FinalCommunicationNotInitialised()
        {
            // Arrange
            Arrange("ModellingReviewComplete");

            // Act 
            finalReview.Execute(StateStatus.Completed, finalCommunication);

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

            communicationUpdate.Execute(StateStatus.Completed);

            // Act 
            finalReview.Execute(StateStatus.Completed, finalCommunication);

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
            finalCommunication.Execute(StateStatus.Completed);

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
            pricingWorkflow.Reset();

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

        private void TraceWrite(State state)
        {
            Trace.WriteLine(String.Format("{0} {1} - {2}", state.Id, state.Name, state.Status));
            foreach (var logEntry in state.Log)
            {
                Trace.WriteLine(String.Format("     {0}", logEntry.Message));
            }
        }

        private void Arrange(string instruction)
        {
            pricingWorkflow.Execute(StateStatus.Initialised);

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

            collateData.Execute(StateStatus.Completed);

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

            modelling1.Execute(StateStatus.Completed);
            modelling2.Execute(StateStatus.Completed);

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

            adjustmentCheck.AddAction(StateActionType.Entry, (a) =>
            {
                a.Transition = finalReview;
            });

            modellingReview.Execute(StateStatus.Completed, adjustmentCheck);

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

            communicationUpdate.Execute(StateStatus.Completed);

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

            finalReview.Execute(StateStatus.Completed, finalCommunication);

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

            finalCommunication.Execute(StateStatus.Completed);

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
