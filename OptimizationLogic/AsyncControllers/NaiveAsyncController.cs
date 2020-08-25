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
        public double SwapChainTime { get; private set; } = 10;
        public double TimeBase { get; private set; } = 9;
        public double RealTime { get; set; } = 9;
        public ItemState IntakeItem { get; set; } = ItemState.Empty;
        public ItemState OuttakeItem { get; set; } = ItemState.Empty;
        public ItemState Needed { get; set; }
        public ItemState Current { get; set; }
        public double StackerOperationTime { get; private set; } = 2.5;
        public AsyncControllerState CurrentState { get; set; } = AsyncControllerState.Start;

        public NaiveAsyncController(ProductionState productionState, string csvProcessingTimeMatrix, string csvWarehouseInitialState, string csvHistroicalProduction, string csvFutureProductionPlan) : base(productionState, csvProcessingTimeMatrix, csvWarehouseInitialState, csvHistroicalProduction, csvFutureProductionPlan)
        {

        }

        public NaiveAsyncController(ProductionState state, string csvProcessingTimeMatrix) : base(state, csvProcessingTimeMatrix)
        {

        }

        public NaiveAsyncController(ProductionState state) : base(state)
        {

        }

        protected double GetClosestNextIntakeTime()
        {
            int currentIntakeSteps = (int)((RealTime - TimeBase) / IntakeClock);
            return TimeBase + (currentIntakeSteps + 1)* IntakeClock;
        }

        protected double GetClosestNextOuttakeTime()
        {
            int currentIntakeSteps = (int)((RealTime - TimeBase) / IntakeClock);
            return TimeBase + (currentIntakeSteps) * IntakeClock + IntakeOuttakeDifference;
        }

        //TODO:  Check times for in-take/out-take operations
        public override bool NextStep()
        {
            if (ProductionState.FutureProductionPlan.Count == 0)
            {
                CurrentState = AsyncControllerState.End;
                return false;
            }

            History.Push(ProductionState.Copy());

            switch (CurrentState)
            {
                case AsyncControllerState.Start:
                    Needed = ProductionState.FutureProductionPlan.Dequeue();
                    Current = ProductionState.ProductionHistory.Dequeue();
                    ProductionState.ProductionHistory.Enqueue(Needed);

                    OuttakeItem = ItemState.Empty;
                    IntakeItem = Current;
                    CurrentState = Needed == Current ? AsyncControllerState.SwapChain : AsyncControllerState.Put;
                    break;
                case AsyncControllerState.SwapChain:
                    OuttakeItem = IntakeItem;
                    IntakeItem = ItemState.Empty;
                    RealTime += SwapChainTime;

                    if (ProductionState.FutureProductionPlan.Count == 0)
                    {
                        CurrentState = AsyncControllerState.End;
                        return false;
                    }

                    if (ProductionState.FutureProductionPlan.Peek() == ProductionState.ProductionHistory.Peek())
                    {
                        CurrentState = AsyncControllerState.Start;
                        RealTime = GetClosestNextIntakeTime();
                        OuttakeItem = ItemState.Empty;
                        IntakeItem = ItemState.Empty;
                    }
                    else
                    {
                        CurrentState = AsyncControllerState.Put;
                    }
                    break;
                case AsyncControllerState.Put:
                    var nearestNeededPosition = GetNearesElementWarehousePosition(ProductionState, Needed);
                    var stackerToItemGetItemAndBack = ProductionState[PositionCodes.Stacker, nearestNeededPosition] + ProductionState[nearestNeededPosition, PositionCodes.Stacker] - StackerOperationTime*3;
                    ProductionState[nearestNeededPosition] = ItemState.Empty;
                    var nextOuttake = GetClosestNextOuttakeTime();
                    RealTime += stackerToItemGetItemAndBack;

                    if (OuttakeItem == ItemState.Empty && RealTime > (nextOuttake - StackerOperationTime))
                    {
                        ProductionState.ProductionStateIsOk = false;
                        CurrentState = AsyncControllerState.End;
                    }
                    else if (OuttakeItem == ItemState.Empty && RealTime < (nextOuttake - StackerOperationTime))
                    {
                        OuttakeItem = Needed;
                        RealTime += StackerOperationTime;
                        CurrentState = AsyncControllerState.Get;
                    }

                    //TODO: Pridat kod pro cekani

                    break;
                case AsyncControllerState.Get:
                    break;
                case AsyncControllerState.End:
                    return false;
            }

            //Needed = ProductionState.FutureProductionPlan.Dequeue();
            //Current = ProductionState.ProductionHistory.Dequeue();
            //ProductionState.ProductionHistory.Enqueue(Needed);

            //if (Needed == Current)
            //{
            //    RealTime += SwapChainTime;

            //    if (ProductionState.FutureProductionPlan.Count == 0)
            //    {
            //        return false;
            //    }

            //    var futureNeed = ProductionState.FutureProductionPlan.Peek();
            //    var futureGot = ProductionState.ProductionHistory.Peek();
            //    if (futureNeed == futureGot)
            //    {
            //        NextIntake += IntakeClock;
            //        NextOuttake += IntakeClock;
            //        RealTime = NextIntake;
            //        IntakeSolved = false;
            //        OuttakeSolved = false;
            //    }
            //    else
            //    {

            //    }
            //} else
            //{

            //}

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

        public override void RenewControllerState()
        {
            base.RenewControllerState();
            RealTime = 9;
            IntakeItem = ItemState.Empty;
            OuttakeItem = ItemState.Empty;
            CurrentState = AsyncControllerState.Start;
        }

        public override bool CanUndo()
        {
            return false;
        }
    }
}
