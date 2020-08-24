using System;
using System.Threading.Tasks;
using OptimizationLogic.DTO;
using OptimizationLogic.Extensions;

namespace OptimizationLogic.AsyncControllers
{
    public class NaiveAsyncController : BaseController
    {
        public int IntakeClock { get; private set; } = 55;
        public int IntakeOuttakeDifference { get; private set; } = 36;

        public double SwapChainTime { get; set; } = 10;

        public double TimeBase { get; set; } = 9;

        public double RealTime { get; set; } = 9;

        public TaskCompletionSource<bool> CanContinue { get; set; } = new TaskCompletionSource<bool>(false);

        public NaiveAsyncController(ProductionState productionState, string csvProcessingTimeMatrix, string csvWarehouseInitialState, string csvHistroicalProduction, string csvFutureProductionPlan) : base(productionState, csvProcessingTimeMatrix, csvWarehouseInitialState, csvHistroicalProduction, csvFutureProductionPlan)
        {

        }

        public NaiveAsyncController(ProductionState state, string csvProcessingTimeMatrix) : base(state, csvProcessingTimeMatrix)
        {

        }

        public NaiveAsyncController(ProductionState state) : base(state)
        {

        }

        //TODO:  Check times for in-take/out-take operations
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

            double nextIntake = TimeBase + IntakeClock*ProductionState.StepCounter;
            double nextOuttake = nextIntake + IntakeOuttakeDifference;

            if (needed == current)
            {
                RealTime += SwapChainTime;
            } else
            {
                double workTime = nextOuttake - RealTime;
            }

            ProductionState.StepCounter++;
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
