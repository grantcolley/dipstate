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

            var remediationWorkflowRoot = new State(100, "Remediation Workflow",
                type: StateType.Root);

            var communication = new State(200, "Communication",
                                                initialiseWithParent: true);

            var letterSent = new State(210, "Letter Sent", initialiseWithParent: true)
                .AddActionAsync(StateActionType.Entry, GenerateLetterAsync)
                .AddActionAsync(StateActionType.Status, SaveStatusAsync)
                .AddActionAsync(StateActionType.Exit, NotifyDispatchAsync)
                .AddCanCompletePredicateAsync(LetterCheckedAsync);

            var responseRecieved = new State(220, "Response Received", canCompleteParent: true)
                .AddActionAsync(StateActionType.Status, SaveStatusAsync);

            var collateData = new State(300, "Collate Data", true)
                .AddCanCompletePredicateAsync(ValidateDataAsync);

            var adjustmentDecision = new State(400, "Adjustment Decision",
                type: StateType.Auto);

            var adjustment = new State(500, "Adjustment");

            var autoTransitionToRedressReview
                = new State(600, "Auto Transition To Redress Review",
                                    type: StateType.Auto);

            var redressReview = new State(700, "Redress Review");

            var payment = new State(800, "Payment", canCompleteParent: true);


            // Assemble the remediation workflow

            redressReview
                .AddTransition(payment)
                .AddTransition(collateData)
                .AddDependency(communication)
                .AddDependency(autoTransitionToRedressReview);

            autoTransitionToRedressReview
                .AddTransition(redressReview);

            adjustment.AddTransition(autoTransitionToRedressReview);

            adjustmentDecision
                .AddTransition(adjustment)
                .AddTransition(autoTransitionToRedressReview)
                .AddActionAsync(StateActionType.Entry, AdjustmentRequiredCheckAsync);

            collateData
                .AddTransition(adjustmentDecision);

            letterSent.AddTransition(responseRecieved);

            communication.AddDependant(redressReview, true)
                .AddSubState(letterSent)
                .AddSubState(responseRecieved)
                .AddTransition(redressReview);

            remediationWorkflowRoot
                .AddSubState(communication)
                .AddSubState(collateData)
                .AddSubState(adjustmentDecision)
                .AddSubState(adjustment)
                .AddSubState(autoTransitionToRedressReview)
                .AddSubState(redressReview)
                .AddSubState(payment);

            var result = await remediationWorkflowRoot.ExecuteAsync(StateStatus.Initialise);

            Assert.IsTrue(result.Name.Equals("Remediation Workflow"));
            Assert.AreEqual(remediationWorkflowRoot.Status, StateStatus.Initialise);
            Assert.AreEqual(communication.Status, StateStatus.Initialise);
            Assert.AreEqual(letterSent.Status, StateStatus.Initialise);
            Assert.AreEqual(collateData.Status, StateStatus.Initialise);

            result = await letterSent.ExecuteAsync(responseRecieved);
            
            Assert.IsTrue(result.Name.Equals("Response Received"));
            Assert.AreEqual(letterSent.Status, StateStatus.Complete);
            Assert.AreEqual(responseRecieved.Status, StateStatus.Initialise);
            Assert.IsTrue(responseRecieved.Antecedent.Equals(letterSent));

            // When a state has only one transition state then setting 
            // it to Complete will automatically transition to it.
            // Note: if there is more than one transition state then 
            // simply setting it to complete will thrown an exception.
            result = await collateData.ExecuteAsync(StateStatus.Complete);

            Assert.AreEqual(collateData.Status, StateStatus.Complete);
            Assert.AreEqual(adjustmentDecision.Status, StateStatus.Complete);
            Assert.IsTrue(adjustmentDecision.Antecedent.Equals(collateData));

            if (result.Equals(adjustment))
            {
                Assert.AreEqual(adjustment.Status, StateStatus.Initialise);
                Assert.IsTrue(adjustment.Antecedent.Equals(adjustmentDecision));

                result = await result.ExecuteAsync(StateStatus.Complete);

                Assert.AreEqual(adjustment.Status, StateStatus.Complete);

                Assert.AreEqual(autoTransitionToRedressReview.Status, StateStatus.Complete);
                Assert.IsTrue(autoTransitionToRedressReview.Antecedent.Equals(adjustment));
            }
            else
            {
                Assert.AreEqual(autoTransitionToRedressReview.Status, StateStatus.Complete);
                Assert.IsTrue(autoTransitionToRedressReview.Antecedent.Equals(adjustmentDecision));
            }

            Assert.IsTrue(result.Equals(redressReview));
            Assert.AreEqual(redressReview.Status, StateStatus.Uninitialise);
            Assert.IsTrue(redressReview.Antecedent.Equals(autoTransitionToRedressReview));

            result = await responseRecieved.ExecuteAsync(StateStatus.Complete);

            Assert.AreEqual(responseRecieved.Status, StateStatus.Complete);
            Assert.AreEqual(communication.Status, StateStatus.Complete);

            Assert.IsTrue(result.Equals(redressReview));
            Assert.AreEqual(redressReview.Status, StateStatus.Initialise);
            Assert.IsTrue(redressReview.Antecedent.Equals(communication));

            result = await redressReview.ExecuteAsync(payment);

            Assert.AreEqual(redressReview.Status, StateStatus.Complete);

            Assert.IsTrue(result.Equals(payment));
            Assert.AreEqual(payment.Status, StateStatus.Initialise);
            Assert.IsTrue(payment.Antecedent.Equals(redressReview));

            result = await payment.ExecuteAsync(StateStatus.Complete);

            Assert.AreEqual(payment.Status, StateStatus.Complete);

            Assert.IsTrue(result.Equals(remediationWorkflowRoot));
            Assert.AreEqual(remediationWorkflowRoot.Status, StateStatus.Complete);
        }

        public static async Task<bool> LetterCheckedAsync(State context)
        {
            await RunAsync(context, "LetterCheckedAsync");
            return true;
        }

        public static async Task<bool> ValidateDataAsync(State context)
        {
            await RunAsync(context, "ValidateDataAsync");
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

        private static async Task AdjustmentRequiredCheckAsync(State context)
        {
            // Determine at runtime whether to transition 
            // to Adjustment or AutoTransitionToReview

            await RunAsync(context, "AdjustmentRequiredCheckAsync");

            // This test randomly selectes the transition state.
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
