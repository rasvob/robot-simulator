using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OptimizationLogic.DAL;
using OptimizationLogic.DTO;
using OptimizationLogic.StateGenerating;

namespace OptimizationUnitTests
{
    [TestClass]
    public class ProductionStateUnitTest
    {
        [DataTestMethod]
        [DataRow(PositionCodes.Stacker, 2, 0)]
        [DataRow(PositionCodes.Service, 2, 11)]
        [DataRow(PositionCodes._2A, 3, 1)]
        [DataRow(PositionCodes._5B, 0, 2)]
        public void TestGetWarehouseIndex(PositionCodes code, int expectedRow, int expectedCol)
        {
            var state = new ProductionState(12, 4);
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

        [DataTestMethod]
        [DataRow(-10, -10, 0, 8)]
        public void TestProductionStateGenerator(double mqbDistanceWeight, double mebDistanceWeight, double uniformProbabilityWeight, int expected)
        {
            var historyGenerator = new ProductionHistoryGenerator(0.5)
            {
                ProbabilityOfStartingInMqbState = 1.0,
                SequenceLength = 64,
            };

            var futureGenerator = new FutureProductionPlanGenerator(0.5)
            {
                ProbabilityOfStartingInMqbState = 1.0,
                SequenceLength = 100
            };

            var files = Directory.GetFiles("Input", "*");
            var loaderFiles = Enumerable.Repeat(0, 1).Select(t =>
            {
                return new ProductionScenarioPaths()
                {
                    FutureProductionListCsv = files.FirstOrDefault(s => s.Contains("Future")),
                    HistoricalProductionListCsv = files.FirstOrDefault(s => s.Contains("Historical")),
                    WarehouseInitialStateCsv = files.FirstOrDefault(s => s.Contains("Warehouse"))
                };
            })
            .ToList();
            var loader = new ProductionStateLoader(loaderFiles, "Input/ProcessingTimeMatrix.csv");

            var prodStateGenerator = new ProductionStateGenerator(historyGenerator, futureGenerator, mqbDistanceWeight, mebDistanceWeight, uniformProbabilityWeight);
            var state = prodStateGenerator.GenerateProductionState();
            Assert.AreEqual(expected, state.WarehouseState.Cast<ItemState>().Count(t => t == ItemState.Empty));
        }
    }
}
