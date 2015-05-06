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
            adjustmentCheck = new DipState(1500, "Adjustment Check");
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
        public void TestMethod1()
        {
        }

        private void TraceWrite(IDipState state)
        {
            Trace.WriteLine(String.Format("{0} {1} - {2}", state.Id, state.Name, state.Status));
            foreach (var logEntry in state.Log)
            {
                Trace.WriteLine(String.Format("     {0}", logEntry.Message));
            }
        }
    }
}
