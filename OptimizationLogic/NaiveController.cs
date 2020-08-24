using OptimizationLogic.DTO;
using OptimizationLogic.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic
{
    public class NaiveController: BaseController
    {
        public NaiveController(ProductionState productionState, string csvProcessingTimeMatrix, string csvWarehouseInitialState, string csvHistroicalProduction, string csvFutureProductionPlan): base(productionState, csvProcessingTimeMatrix, csvWarehouseInitialState, csvHistroicalProduction, csvFutureProductionPlan)
        {
        }

        public NaiveController(ProductionState state, string csvProcessingTimeMatrix): base(state, csvProcessingTimeMatrix)
        {
        }

        public NaiveController(ProductionState state): base(state)
        {
        }

        public override bool NextStep()
        {
            if (ProductionState.FutureProductionPlan.Count == 0)
            {
                return false;
            }

            History.Push(ProductionState.Copy());

            var needed = ProductionState.FutureProductionPlan.Dequeue();
            var current = ProductionState.ProductionHistory.Dequeue();
            ProductionState.ProductionHistory.Enqueue(needed);

            var nearestFreePosition = GetNearestEmptyPosition(ProductionState);
            (int r, int c) = ProductionState.GetWarehouseIndex(nearestFreePosition);
            ProductionState.WarehouseState[r, c] = current;
            
            var nearestNeededPosition = GetNearesElementWarehousePosition(ProductionState, needed);
            (r, c) = ProductionState.GetWarehouseIndex(nearestNeededPosition);
            ProductionState.WarehouseState[r, c] = ItemState.Empty;

            var insertTime = ProductionState.TimeMatrix[ProductionState.GetTimeMatrixIndex(PositionCodes.Stacker), ProductionState.GetTimeMatrixIndex(nearestFreePosition)];
            var moveToDifferentCellTime = ProductionState.TimeMatrix[ProductionState.GetTimeMatrixIndex(nearestFreePosition), ProductionState.GetTimeMatrixIndex(nearestNeededPosition)] - 5; // TODO: Validate this calculation
            var withdrawTime = ProductionState.TimeMatrix[ProductionState.GetTimeMatrixIndex(nearestNeededPosition), ProductionState.GetTimeMatrixIndex(PositionCodes.Stacker)];
            var totalTime = insertTime + moveToDifferentCellTime + withdrawTime;
            ProductionState.CurrentStepTime = totalTime;
            ProductionState.TimeSpentInSimulation += totalTime;

            ProductionState.ProductionStateIsOk = ProductionState.ProductionStateIsOk && totalTime <= TimeLimit;
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
    }
}
