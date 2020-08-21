using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OptimizationLogic.DTO;
using OptimizationLogic.StateGenerating;

namespace OptimizationUnitTests
{
    [TestClass]
    public class ProductionStateUnitTest
    {
        [DataTestMethod]
        [DataRow(PositionCodes.Stacker, 0)]
        [DataRow(PositionCodes.Service, 44)]
        [DataRow(PositionCodes._2A, 3)]
        public void TestGetTimeMatrixIndex(PositionCodes code, int expected)
        {
            var state = new ProductionState();
            var res = state.GetTimeMatrixIndex(code);
            Assert.AreEqual(expected, res);
        }

        [DataTestMethod]
        [DataRow(PositionCodes.Stacker, 2, 0)]
        [DataRow(PositionCodes.Service, 2, 11)]
        [DataRow(PositionCodes._2A, 3, 1)]
        [DataRow(PositionCodes._5B, 0, 2)]
        public void TestGetWarehouseIndex(PositionCodes code, int expectedRow, int expectedCol)
        {
            var state = new ProductionState();
            var res = state.GetWarehouseIndex(code);
            Assert.AreEqual((expectedRow, expectedCol), res);
        }

        [DataTestMethod]
        [DataRow(0.0, 100)]
        [DataRow(1.0, 50)]
        public void TestFutureProductionPlanGenerator(double proba, int expected)
        {
            var generator = new FutureProductionPlanGenerator(proba)
            {
                ProbabilityOfStartingInMqbState = 1.0,
                SequenceLength = 100
            };

            var sequence = generator.GenerateSequence();
            Assert.AreEqual(expected, sequence.Count(t => t == ItemState.MQB));
        }

        [DataTestMethod]
        [DataRow(0.0, 64)]
        [DataRow(1.0, 32)]
        public void TestProductionHistoryGenerator(double proba, int expected)
        {
            var generator = new ProductionHistoryGenerator(proba)
            {
                ProbabilityOfStartingInMqbState = 1.0,
                SequenceLength = 64,
            };

            var sequence = generator.GenerateSequence();
            Assert.AreEqual(expected, sequence.Count(t => t == ItemState.MQB));
        }
    }
}
