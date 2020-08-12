using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic
{
    public class NaiveController
    {
        public ProductionState ProductionState { get; set; }
        public List<StepModel> StepLog { get; set; } = new List<StepModel>();

        private List<PositionCodes> SortedPositionCodes;
        private const int TimeLimit = 55;

        public NaiveController(ProductionState productionState, string csvProcessingTimeMatrix, string csvWarehouseInitialState, string csvHistroicalProduction, string csvFutureProductionPlan)
        {
            ProductionState = productionState;
            ProductionState.LoadFutureProductionPlan(csvFutureProductionPlan);
            ProductionState.LoadProductionHistory(csvHistroicalProduction);
            ProductionState.LoadTimeMatrix(csvProcessingTimeMatrix);
            ProductionState.LoadWarehouseState(csvWarehouseInitialState);

            Dictionary<PositionCodes, double> cellsTimes = new Dictionary<PositionCodes, double>();
            foreach (PositionCodes positionCode in Enum.GetValues(typeof(PositionCodes)))
            {
                if (positionCode != PositionCodes.Service && positionCode != PositionCodes.Stacker)
                {
                    cellsTimes[positionCode] = ProductionState.TimeMatrix[ProductionState.GetTimeMatrixIndex(PositionCodes.Stacker), ProductionState.GetTimeMatrixIndex(positionCode)];
                }
            }
            SortedPositionCodes = cellsTimes.OrderBy(i => i.Value).Select(x => x.Key).ToList();
        }

        // TODO: Generate random inputs
        public NaiveController(ProductionState state, string csvProcessingTimeMatrix)
        {
            ProductionState = state;
            ProductionState.LoadTimeMatrix(csvProcessingTimeMatrix);
        }

        public void NextStep()
        {
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
        }

        private PositionCodes GetNearestEmptyPosition()
        {
            return GetNearesElementWarehousePosition(ItemState.Empty);
        }

        private PositionCodes GetNearesElementWarehousePosition(ItemState itemState)
        {
            foreach (PositionCodes positionCode in SortedPositionCodes)
            {
                (int r, int c) = ProductionState.GetWarehouseIndex(positionCode);
                if (ProductionState.WarehouseState[r, c] == itemState)
                {
                    return positionCode;
                }
            }
            throw new Exception("ItemState is not in warehouse.");
        }
    }
}
