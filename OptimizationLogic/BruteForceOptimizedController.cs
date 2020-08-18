using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic
{
    public class BruteForceOptimizedController : NaiveController
    {
        private const int TimeLimit = 55;

        public BruteForceOptimizedController(ProductionState productionState, string csvProcessingTimeMatrix, string csvWarehouseInitialState, string csvHistroicalProduction, string csvFutureProductionPlan) : base(productionState, csvProcessingTimeMatrix, csvWarehouseInitialState, csvHistroicalProduction, csvFutureProductionPlan)
        {
        }

        public override bool NextStep()
        {
            if (ProductionState.FutureProductionPlan.Count == 0)
            {
                return false;
            }
            
            if ((ProductionState.StepCounter-1) % 50 == 0)
            {
                ReorganizeWarehouse(300);
            }

            var needed = ProductionState.FutureProductionPlan.Dequeue();
            var current = ProductionState.ProductionHistory.Dequeue();
            ProductionState.ProductionHistory.Enqueue(needed);

            var nearestFreePosition = GetNearestEmptyPosition();
            (int r, int c) = ProductionState.GetWarehouseIndex(nearestFreePosition);
            ProductionState.WarehouseState[r, c] = current;

            var nearestNeededPosition = GetNearesElementWarehousePosition(needed);
            (r, c) = ProductionState.GetWarehouseIndex(nearestNeededPosition);
            ProductionState.WarehouseState[r, c] = ItemState.Empty;

            var insertTime = ProductionState.TimeMatrix[ProductionState.GetTimeMatrixIndex(PositionCodes.Stacker), ProductionState.GetTimeMatrixIndex(nearestFreePosition)];
            var moveToDifferentCellTime = ProductionState.TimeMatrix[ProductionState.GetTimeMatrixIndex(nearestFreePosition), ProductionState.GetTimeMatrixIndex(nearestNeededPosition)] - 5; // TODO: Validate this calculation
            var withdrawTime = ProductionState.TimeMatrix[ProductionState.GetTimeMatrixIndex(nearestNeededPosition), ProductionState.GetTimeMatrixIndex(PositionCodes.Stacker)];
            var totalTime = insertTime + moveToDifferentCellTime + withdrawTime;

            ProductionState.ProductionStateIsOk = totalTime <= TimeLimit;
            ProductionState.StepCounter++;

            StepLog.Add(new StepModel
            {
                InsertToCell = nearestFreePosition,
                WithdrawFromCell = nearestNeededPosition,
                InsertType = current,
                WithdrawType = needed,
                InsertTime = insertTime,
                MoveToDifferentCellTime = moveToDifferentCellTime,
                WithdrawTime = withdrawTime
            });

            return true;
        }

        private void ReorganizeWarehouse (int reservedTime)
        {
            ProductionState initialProductionState = (ProductionState)ProductionState.Clone(); 

        }
    }
}
