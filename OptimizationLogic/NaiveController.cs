//#define HISTORY
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
        public NaiveController(ProductionState productionState, string csvWarehouseInitialState, string csvHistroicalProduction, string csvFutureProductionPlan): base(productionState, csvWarehouseInitialState, csvHistroicalProduction, csvFutureProductionPlan)
        {
        }

        public NaiveController(ProductionState state): base(state)
        {
        }

        public NaiveController(ProductionState state, int tactTime, int operationLimit) : base(state, tactTime, operationLimit)
        {
        }

        public override bool NextStep()
        {
            if (ProductionState.FutureProductionPlan.Count == 0)
            {
                return false;
            }

#if HISTORY
            History.Push(ProductionState.Copy());
#endif

            var needed = ProductionState.FutureProductionPlan.Dequeue();
            var current = ProductionState.ProductionHistory.Dequeue();
            ProductionState.ProductionHistory.Enqueue(needed);

            var nearestFreePosition = GetNearestEmptyPosition(ProductionState);
            (int r, int c) = ProductionState.GetWarehouseIndex(nearestFreePosition);
            ProductionState.WarehouseState[r, c] = current;
            
            var nearestNeededPosition = GetNearesElementWarehousePosition(ProductionState, needed);
            (r, c) = ProductionState.GetWarehouseIndex(nearestNeededPosition);
            ProductionState.WarehouseState[r, c] = ItemState.Empty;

            var insertTime = ProductionState[PositionCodes.Stacker, nearestFreePosition];
            var moveToDifferentCellTime = ProductionState[nearestFreePosition, nearestNeededPosition] - 5; // TODO: Validate this calculation
            moveToDifferentCellTime = moveToDifferentCellTime < 0 ? 0 : moveToDifferentCellTime; 
            var withdrawTime = ProductionState[nearestNeededPosition, PositionCodes.Stacker];
            var totalTime = insertTime + moveToDifferentCellTime + withdrawTime;
            ProductionState.CurrentStepTime = totalTime;
            ProductionState.TimeSpentInSimulation += totalTime;

            ProductionState.ProductionStateIsOk = ProductionState.ProductionStateIsOk && totalTime <= TimeLimit;
            RealTime += ClockTime;
            if (totalTime > TimeLimit)
            {
                Delay += totalTime - TimeLimit;
                RealTime += totalTime - TimeLimit;
            }            
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

        public override BaseController CreateNew(ProductionState state)
        {
            NaiveController controller = new NaiveController(state);
            controller.SetControllerTimes(this.ClockTime, this.TimeLimit);
            return controller;
        }
    }
}
