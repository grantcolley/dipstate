using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipState.Test
{
    [TestClass]
    public class ExampleWorkflowTest
    {
        private DipStateEngine dipStateEngine;
        private IDipState pricingWorkflow;
        private IDipState collateData;
        private IDipState modelling;
        private IDipState modelling1;
        private IDipState modelling2;
        private IDipState reviewModelling;
        private IDipState adjustmentCheck;
        private IDipState adjustments;
        private IDipState finalReview;

        [TestInitialize]
        public void Initialise()
        {
            dipStateEngine = new DipStateEngine();
        }

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
