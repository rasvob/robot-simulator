using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OptimizationLogic.DTO;

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

        
    }
}
