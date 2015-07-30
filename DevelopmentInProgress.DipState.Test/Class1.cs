//using System;
//using System.Diagnostics;
//using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace DevelopmentInProgress.DipState.Test
//{
//    [TestClass]
//    public class Class1
//    {
//        [TestMethod]
//        public void Test()
//        {
//            var remediationWorkflow = new State.DipState(1, "Remediation Workflow", type: StateType.Root);
//            var communication = new Communication() { Id = 2, Name = "Communication", InitialiseWithParent = true };
//            var collateData = new CollateData() { Id = 3, Name = "Collate Date", InitialiseWithParent = true };
//            var adjustmentDecision = new AdjustmentDecision() { Id = 4, Name = "Adjustment Decision", Type = StateType.Auto };
//            var adjustment = new Adjustment() { Id = 5, Name = "Adjustment" };
//            var autoTransitionRedressReview = new AutoTransitionRedressReview() { Id = 6, Name = "Auto Transition Redress Review", Type = StateType.Auto };
//            var redressReview = new RedressReview() { Id = 6, Name = "Redress Review" };
//            var payment = new Payment() { Id = 7, Name = "Payment", CanCompleteParent = true };

//            remediationWorkflow
//                .AddSubState(communication)
//                .AddSubState(collateData)
//                .AddSubState(adjustmentDecision)
//                .AddSubState(adjustment)
//                .AddSubState(autoTransitionRedressReview)
//                .AddSubState(redressReview)
//                .AddSubState(payment);

//            communication.AddDependant(redressReview, true);

//            collateData
//                .AddTransition(adjustmentDecision);

//            adjustmentDecision
//                .AddTransition(adjustment)
//                .AddTransition(autoTransitionRedressReview)
//                .AddAction(StateActionType.Entry, (s =>
//                {
//                    var collData = s.Antecedent as CollateData;
//                    if (collData.RedressAmount == null
//                        || collateData.RedressAmount.Value < 100)
//                    {
//                        // If the calculated redress amount is less
//                        // than 100 transition to adjustment.
//                        s.Transition = s.Transitions[0];
//                    }
//                    else
//                    {
//                        // If the calculated redress amount is greater 
//                        // or equal to 100 transition to redress review.
//                        s.Transition = s.Transitions[1];
//                    }
//                }));

//            adjustment.AddTransition(autoTransitionRedressReview);

//            autoTransitionRedressReview
//                .AddTransition(redressReview);
//            //.AddDependant(redressReview);

//            redressReview
//                .AddTransition(payment)
//                .AddTransition(collateData)
//                .AddDependency(communication);
//        }
//    }
//}
