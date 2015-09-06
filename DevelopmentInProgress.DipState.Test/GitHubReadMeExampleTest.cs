using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class GitHubReadMeExampleTest
    {
        [TestMethod]
        public async Task RemediationWorkflowTest()
        {
            // Create the remediation workflow states

            var remediationWorkflowRoot 
                = new State(100, "Remediation Workflow", StateType.Root);

            var communication = new State(200, "Communication");

            var letterSent = new State(210, "Letter Sent")
                .AddActionAsync(StateActionType.OnEntry, GenerateLetterAsync)
                .AddActionAsync(StateActionType.OnStatusChanged, SaveStatusAsync)
                .AddActionAsync(StateActionType.OnExit, NotifyDispatchAsync)
                .AddCanInitialisePredicateAsync(CanInitialiseLetterSentAsync)
                .AddCanChangeStatusPredicateAsync(CanChangeLetterSentStatusAsync)
                .AddCanCompletePredicateAsync(CanCompleteLetterSentAsync)
                .AddCanResetPredicateAsync(CanResetLetterSentAsync);

            var responseReceived = new State(220, "Response Received");

            var collateData = new State(300, "Collate Data");

            var adjustmentDecision 
                = new State(400, "Adjustment Decision", StateType.Auto);

            var adjustment = new State(500, "Adjustment");

            var autoTransitionToRedressReview
                = new State(600, "Auto Transition To Redress Review", StateType.Auto);

            var redressReview = new State(700, "Redress Review");

            var payment = new State(800, "Payment");


            // Assemble the remediation workflow

            redressReview
                .AddTransition(payment, true)
                .AddTransition(collateData)
                .AddDependency(communication, true)
                .AddDependency(autoTransitionToRedressReview, true)
                .AddActionAsync(StateActionType.OnEntry, CalculateFinalRedressAmountAsync);

            autoTransitionToRedressReview
                .AddDependant(redressReview)
                .AddTransition(redressReview, true);

            adjustment.AddTransition(autoTransitionToRedressReview, true);

            adjustmentDecision
                .AddTransition(adjustment)
                .AddTransition(autoTransitionToRedressReview)
                .AddActionAsync(StateActionType.OnEntry, ConditionalTransitionDecisionAsync);

            collateData
                .AddTransition(adjustmentDecision, true);

            letterSent.AddTransition(responseReceived, true);

            communication
                .AddSubState(letterSent, true)
                .AddSubState(responseReceived)
                .AddDependant(redressReview)
                .AddTransition(redressReview, true);

            remediationWorkflowRoot
                .AddSubState(communication, true)
                .AddSubState(collateData, true)
                .AddSubState(adjustmentDecision)
                .AddSubState(adjustment, completionRequired: false)
                .AddSubState(autoTransitionToRedressReview)
                .AddSubState(redressReview)
                .AddSubState(payment);

            // Initialised the workflow

            var result = await remediationWorkflowRoot.ExecuteAsync(StateExecutionType.Initialise);

            Assert.IsTrue(result.Equals(remediationWorkflowRoot));

            Assert.AreEqual(remediationWorkflowRoot.Status, StateStatus.Initialised);
            Assert.AreEqual(communication.Status, StateStatus.Initialised);
            Assert.AreEqual(letterSent.Status, StateStatus.Initialised);
            Assert.AreEqual(responseReceived.Status, StateStatus.Uninitialised);
            Assert.AreEqual(collateData.Status, StateStatus.Initialised);
            Assert.AreEqual(adjustmentDecision.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustment.Status, StateStatus.Uninitialised);
            Assert.AreEqual(autoTransitionToRedressReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(redressReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(payment.Status, StateStatus.Uninitialised);

            result = await letterSent.ExecuteAsync(StateExecutionType.InProgress);

            Assert.IsTrue(result.Equals(letterSent));
            Assert.IsNull(letterSent.Antecedent);

            Assert.AreEqual(remediationWorkflowRoot.Status, StateStatus.InProgress);
            Assert.AreEqual(communication.Status, StateStatus.InProgress);
            Assert.AreEqual(letterSent.Status, StateStatus.InProgress);
            Assert.AreEqual(responseReceived.Status, StateStatus.Uninitialised);
            Assert.AreEqual(collateData.Status, StateStatus.Initialised);
            Assert.AreEqual(adjustmentDecision.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustment.Status, StateStatus.Uninitialised);
            Assert.AreEqual(autoTransitionToRedressReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(redressReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(payment.Status, StateStatus.Uninitialised);

            result = await letterSent.ExecuteAsync(StateExecutionType.Reset);

            Assert.IsTrue(result.Equals(letterSent));

            Assert.AreEqual(remediationWorkflowRoot.Status, StateStatus.Initialised);
            Assert.AreEqual(communication.Status, StateStatus.Uninitialised);
            Assert.AreEqual(letterSent.Status, StateStatus.Uninitialised);
            Assert.AreEqual(responseReceived.Status, StateStatus.Uninitialised);
            Assert.AreEqual(collateData.Status, StateStatus.Initialised);
            Assert.AreEqual(adjustmentDecision.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustment.Status, StateStatus.Uninitialised);
            Assert.AreEqual(autoTransitionToRedressReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(redressReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(payment.Status, StateStatus.Uninitialised);

            result = await letterSent.ExecuteAsync(StateExecutionType.Initialise);

            Assert.IsTrue(result.Equals(letterSent));

            Assert.AreEqual(remediationWorkflowRoot.Status, StateStatus.Initialised);
            Assert.AreEqual(communication.Status, StateStatus.Initialised);
            Assert.AreEqual(letterSent.Status, StateStatus.Initialised);
            Assert.AreEqual(responseReceived.Status, StateStatus.Uninitialised);
            Assert.AreEqual(collateData.Status, StateStatus.Initialised);
            Assert.AreEqual(adjustmentDecision.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustment.Status, StateStatus.Uninitialised);
            Assert.AreEqual(autoTransitionToRedressReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(redressReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(payment.Status, StateStatus.Uninitialised);

            result = await letterSent.ExecuteAsync(responseReceived);

            Assert.IsTrue(result.Equals(responseReceived));
            Assert.IsTrue(responseReceived.Antecedent.Equals(letterSent));

            Assert.AreEqual(remediationWorkflowRoot.Status, StateStatus.InProgress);
            Assert.AreEqual(communication.Status, StateStatus.InProgress);
            Assert.AreEqual(letterSent.Status, StateStatus.Completed);
            Assert.AreEqual(responseReceived.Status, StateStatus.Initialised);
            Assert.AreEqual(collateData.Status, StateStatus.Initialised);
            Assert.AreEqual(adjustmentDecision.Status, StateStatus.Uninitialised);
            Assert.AreEqual(adjustment.Status, StateStatus.Uninitialised);
            Assert.AreEqual(autoTransitionToRedressReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(redressReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(payment.Status, StateStatus.Uninitialised);

            result = await collateData.ExecuteAsync(StateExecutionType.Complete);

            Assert.IsTrue(adjustmentDecision.Antecedent.Equals(collateData));

            Assert.AreEqual(remediationWorkflowRoot.Status, StateStatus.InProgress);
            Assert.AreEqual(communication.Status, StateStatus.InProgress);
            Assert.AreEqual(letterSent.Status, StateStatus.Completed);
            Assert.AreEqual(responseReceived.Status, StateStatus.Initialised);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentDecision.Status, StateStatus.Completed);
            Assert.AreEqual(redressReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(payment.Status, StateStatus.Uninitialised);

            if (result.Equals(adjustment))
            {
                Assert.AreEqual(adjustment.Status, StateStatus.Initialised);
                Assert.IsTrue(adjustment.Antecedent.Equals(adjustmentDecision));

                result = await result.ExecuteAsync(StateExecutionType.Complete);

                Assert.AreEqual(adjustment.Status, StateStatus.Completed);

                Assert.AreEqual(autoTransitionToRedressReview.Status, StateStatus.Completed);
                Assert.IsTrue(autoTransitionToRedressReview.Antecedent.Equals(adjustment));
            }
            else
            {
                Assert.AreEqual(autoTransitionToRedressReview.Status, StateStatus.Completed);
                Assert.IsTrue(autoTransitionToRedressReview.Antecedent.Equals(adjustmentDecision));
            }

            Assert.IsTrue(result.Equals(redressReview));
            Assert.IsTrue(redressReview.Antecedent.Equals(autoTransitionToRedressReview));

            Assert.AreEqual(remediationWorkflowRoot.Status, StateStatus.InProgress);
            Assert.AreEqual(communication.Status, StateStatus.InProgress);
            Assert.AreEqual(letterSent.Status, StateStatus.Completed);
            Assert.AreEqual(responseReceived.Status, StateStatus.Initialised);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentDecision.Status, StateStatus.Completed);
            Assert.AreEqual(redressReview.Status, StateStatus.Uninitialised);
            Assert.AreEqual(payment.Status, StateStatus.Uninitialised);

            result = await responseReceived.ExecuteAsync(StateExecutionType.Complete);

            Assert.IsTrue(result.Equals(redressReview));
            Assert.IsTrue(redressReview.Antecedent.Equals(communication));

            Assert.AreEqual(remediationWorkflowRoot.Status, StateStatus.InProgress);
            Assert.AreEqual(communication.Status, StateStatus.Completed);
            Assert.AreEqual(letterSent.Status, StateStatus.Completed);
            Assert.AreEqual(responseReceived.Status, StateStatus.Completed);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentDecision.Status, StateStatus.Completed);
            Assert.AreEqual(redressReview.Status, StateStatus.Initialised);
            Assert.AreEqual(payment.Status, StateStatus.Uninitialised);

            result = await redressReview.ExecuteAsync(payment);

            Assert.IsTrue(result.Equals(payment));
            Assert.IsTrue(payment.Antecedent.Equals(redressReview));

            Assert.AreEqual(remediationWorkflowRoot.Status, StateStatus.InProgress);
            Assert.AreEqual(communication.Status, StateStatus.Completed);
            Assert.AreEqual(letterSent.Status, StateStatus.Completed);
            Assert.AreEqual(responseReceived.Status, StateStatus.Completed);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentDecision.Status, StateStatus.Completed);
            Assert.AreEqual(redressReview.Status, StateStatus.Completed);
            Assert.AreEqual(payment.Status, StateStatus.Initialised);

            result = await payment.ExecuteAsync(StateExecutionType.Complete);

            Assert.IsTrue(result.Equals(remediationWorkflowRoot));

            Assert.AreEqual(remediationWorkflowRoot.Status, StateStatus.Completed);
            Assert.AreEqual(communication.Status, StateStatus.Completed);
            Assert.AreEqual(letterSent.Status, StateStatus.Completed);
            Assert.AreEqual(responseReceived.Status, StateStatus.Completed);
            Assert.AreEqual(collateData.Status, StateStatus.Completed);
            Assert.AreEqual(adjustmentDecision.Status, StateStatus.Completed);
            Assert.AreEqual(redressReview.Status, StateStatus.Completed);
            Assert.AreEqual(payment.Status, StateStatus.Completed);
        }

        public static async Task<bool> CanInitialiseLetterSentAsync(State context)
        {
            await RunAsync(context, "CanInitialiseLetterSentAsync");
            return true;
        }

        public static async Task<bool> CanChangeLetterSentStatusAsync(State context)
        {
            await RunAsync(context, "CanChangeLetterSentStatusAsync");
            return true;
        }

        public static async Task<bool> CanCompleteLetterSentAsync(State context)
        {
            await RunAsync(context, "CanCompleteLetterSentAsync");
            return true;
        }

        public static async Task<bool> CanResetLetterSentAsync(State context)
        {
            await RunAsync(context, "CanResetLetterSentAsync");
            return true;
        }

        private static async Task SaveStatusAsync(State context)
        {
            await RunAsync(context, "SaveStatusAsync");
        }

        private static async Task NotifyDispatchAsync(State context)
        {
            await RunAsync(context, "NotifyDispatchAsync");
        }

        public static async Task GenerateLetterAsync(State context)
        {
            await RunAsync(context, "GenerateLetterAsync");
        }

        private static async Task CalculateFinalRedressAmountAsync(State context)
        {
            await RunAsync(context, "CalculateFinalRedressAmountAsync");
        }

        private static async Task ConditionalTransitionDecisionAsync(State context)
        {
            // Determine at runtime whether to transition 
            // to Adjustment or AutoTransitionToReview

            await RunAsync(context, "AdjustmentRequiredCheckAsync");

            // This test randomly selects the transition state.
            var random = new Random();
            context.Transition = context.Transitions[random.Next(0, 2)];
        }

        private static async Task RunAsync(State context, string methodName)
        {
            var startLogEntry = new LogEntry(String.Format("Start {0} - {1}", methodName, context.Name));
            context.Log.Add(startLogEntry);

            Debug.WriteLine(startLogEntry.ToString());

            await Task.Run(async delegate
            {
                var random = new Random();
                int milliseconds = random.Next(3) * 1000;
                await Task.Delay(milliseconds);
            });

            var endLogEntry = new LogEntry(String.Format("End {0} - {1}", methodName, context.Name));
            context.Log.Add(endLogEntry);

            Debug.WriteLine(endLogEntry.ToString());
        }
    }
}
