using System;
using OptimizationLogic.DTO;
using OptimizationLogic.Extensions;

namespace OptimizationLogic.AsyncControllers
{
    public class NaiveAsyncController : BaseController
    {
        
        public NaiveAsyncController(ProductionState productionState, string csvProcessingTimeMatrix, string csvWarehouseInitialState, string csvHistroicalProduction, string csvFutureProductionPlan) : base(productionState, csvProcessingTimeMatrix, csvWarehouseInitialState, csvHistroicalProduction, csvFutureProductionPlan)
        {

        }

        public NaiveAsyncController(ProductionState state, string csvProcessingTimeMatrix) : base(state, csvProcessingTimeMatrix)
        {

        }

        public NaiveAsyncController(ProductionState state) : base(state)
        {

        }

        //TODO:  
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




            StepLog.Add(new StepModel
            {
                //InsertToCell = nearestFreePosition,
                //WithdrawFromCell = nearestNeededPosition,
                //InsertType = current,
                //WithdrawType = needed,
                //InsertTime = insertTime,
                //MoveToDifferentCellTime = moveToDifferentCellTime,
                //WithdrawTime = withdrawTime
            });

            return true;
        }
    }
}
