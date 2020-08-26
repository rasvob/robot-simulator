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

        public AsyncControllerState PreviousStateForPut { get; set; }

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
                    Current = ProductionState.ProductionHistory.Peek();
                    ProductionState.ProductionHistory.Enqueue(Needed);

                    OuttakeItem = ItemState.Empty;
                    IntakeItem = Current;
                    CurrentState = Needed == Current ? AsyncControllerState.SwapChain : AsyncControllerState.Put;
                    PreviousStateForPut = AsyncControllerState.Start;
                    break;
                case AsyncControllerState.SwapChain:
                    OuttakeItem = IntakeItem;
                    IntakeItem = ItemState.Empty;
                    RealTime += SwapChainTime;
                    ProductionState.ProductionHistory.Dequeue();

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
                        PreviousStateForPut = AsyncControllerState.SwapChain;
                        Needed = ProductionState.FutureProductionPlan.Peek();
                    }
                    break;
                case AsyncControllerState.Put:
                    {
                        var nearestNeededPosition = GetNearesElementWarehousePosition(ProductionState, Needed);
                        var stackerRoundtripForItem = ProductionState[PositionCodes.Stacker, nearestNeededPosition] + ProductionState[nearestNeededPosition, PositionCodes.Stacker] - StackerOperationTime * 2;
                        ProductionState[nearestNeededPosition] = ItemState.Empty;
                        var nextOuttake = GetClosestNextOuttakeTime();
                        RealTime += stackerRoundtripForItem;
                        Current = ProductionState.ProductionHistory.Peek();
                        switch (PreviousStateForPut)
                        {
                            case AsyncControllerState.Start:
                                if (RealTime > nextOuttake)
                                {
                                    ProductionState.ProductionStateIsOk = false;
                                    CurrentState = AsyncControllerState.End;
                                }
                                else if (RealTime <= nextOuttake)
                                {
                                    OuttakeItem = Needed;
                                    CurrentState = AsyncControllerState.Get;
                                }
                                break;

                            case AsyncControllerState.SwapChain:
                                var deq = ProductionState.FutureProductionPlan.Dequeue();
                                ProductionState.ProductionHistory.Enqueue(deq);
                                CurrentState = AsyncControllerState.Get;
                                RealTime = GetClosestNextIntakeTime();
                                OuttakeItem = Needed;
                                Current = ProductionState.ProductionHistory.Peek();
                                IntakeItem = Current;
                                break;

                            case AsyncControllerState.Get:
                                if (RealTime > nextOuttake)
                                {
                                    ProductionState.ProductionStateIsOk = false;
                                    CurrentState = AsyncControllerState.End;
                                }
                                else if (RealTime <= nextOuttake)
                                {
                                    OuttakeItem = Needed;
                                    Current = ProductionState.ProductionHistory.Peek();
                                    CurrentState = AsyncControllerState.Get;
                                    var deq1 = ProductionState.FutureProductionPlan.Dequeue();
                                    ProductionState.ProductionHistory.Enqueue(deq1);
                                }
                                break;
                        }
                        break;
                    }
                case AsyncControllerState.Get:
                    {
                        var nearestNeededPosition = GetNearesElementWarehousePosition(ProductionState, ItemState.Empty);
                        var stackerRoundtripForItem = ProductionState[PositionCodes.Stacker, nearestNeededPosition] + ProductionState[nearestNeededPosition, PositionCodes.Stacker] - StackerOperationTime * 2;
                        ProductionState[nearestNeededPosition] = Current;
                        var nextIntake = GetClosestNextIntakeTime();
                        RealTime += stackerRoundtripForItem;

                        if (RealTime > nextIntake)
                        {
                            ProductionState.ProductionStateIsOk = false;
                            CurrentState = AsyncControllerState.End;
                            break;
                        }

                        ProductionState.ProductionHistory.Dequeue();
                        if (ProductionState.FutureProductionPlan.Peek() == ProductionState.ProductionHistory.Peek())
                        {
                            CurrentState = AsyncControllerState.Start;
                            RealTime = nextIntake;
                            IntakeItem = ItemState.Empty;
                            break;
                        }

                        CurrentState = AsyncControllerState.Put;
                        PreviousStateForPut = AsyncControllerState.Get;
                        Needed = ProductionState.FutureProductionPlan.Peek();
                        break;
                    }
                case AsyncControllerState.End:
                    return false;
            }

            ProductionState.StepCounter++;
            StepLog.Add(new AsyncStepModel
            {
                CurrentState = this.CurrentState,
                Message = "Test"
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
