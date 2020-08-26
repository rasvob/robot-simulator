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
        public double RealTime { get; set; } = 0;
        public ItemState IntakeItem { get; set; } = ItemState.Empty;
        public ItemState OuttakeItem { get; set; } = ItemState.Empty;
        public ItemState Needed { get; set; }
        public ItemState Current { get; set; }
        public double StackerOperationTime { get; private set; } = 2.5;
        public AsyncControllerState CurrentState { get; set; } = AsyncControllerState.Start;

        public AsyncControllerState PreviousStateForPut { get; set; }
        public double TimePadding { get; private set; } = 0.000;

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
            if (RealTime < 9)
            {
                return 9;
            }

            int currentIntakeSteps = (int)((RealTime - TimeBase) / IntakeClock);
            return TimeBase + (currentIntakeSteps + 1)* IntakeClock;
        }

        protected double GetClosestNextOuttakeTime()
        {
            if ((RealTime) < (45))
            {
                return 45;
            }

            int currentIntakeSteps = (int)((RealTime - 45) / IntakeClock);
            return (currentIntakeSteps+1) * IntakeClock + 45;
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
            double realTimeBeforeOp;
            switch (CurrentState)
            {
                case AsyncControllerState.Start:
                    {
                        Needed = ProductionState.FutureProductionPlan.Dequeue();
                        Current = ProductionState.ProductionHistory.Peek();
                        ProductionState.ProductionHistory.Enqueue(Needed);
                        OuttakeItem = ItemState.Empty;
                        IntakeItem = Current;
                        CurrentState = Needed == Current ? AsyncControllerState.SwapChain : AsyncControllerState.Put;
                        PreviousStateForPut = AsyncControllerState.Start;

                        StepLog.Add(new AsyncStepModel
                        {
                            CurrentState = AsyncControllerState.Start,
                            Message = $"RealTime: {RealTime}, Need: {Needed}, Current from history: {Current}, Going to state: {CurrentState}"
                        });
                        break;
                    }
                case AsyncControllerState.SwapChain:
                    {
                        OuttakeItem = IntakeItem;
                        IntakeItem = ItemState.Empty;
                        realTimeBeforeOp = RealTime;
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

                        StepLog.Add(new AsyncStepModel
                        {
                            CurrentState = AsyncControllerState.SwapChain,
                            Message = $"RealTime: {RealTime}, Going to state: {CurrentState}, Time before swap: {realTimeBeforeOp}"
                        });
                        break;
                    }
                case AsyncControllerState.Put:
                    {
                        var nearestNeededPosition = GetNearesElementWarehousePosition(ProductionState, Needed);
                        var stackerRoundtripForItem = ProductionState[PositionCodes.Stacker, nearestNeededPosition] + ProductionState[nearestNeededPosition, PositionCodes.Stacker] - StackerOperationTime * 2;
                        ProductionState[nearestNeededPosition] = ItemState.Empty;
                        realTimeBeforeOp = RealTime;
                        var nextOuttake = GetClosestNextOuttakeTime();
                        RealTime += stackerRoundtripForItem;
                        Current = ProductionState.ProductionHistory.Peek();

                        AsyncStepModel step = new AsyncStepModel
                        {
                            CurrentState = AsyncControllerState.Put,
                            Message = $"RealTime: {RealTime}, Need: {Needed}, Took item from position: {nearestNeededPosition} with time {stackerRoundtripForItem}, Next outtake in: {nextOuttake}, Came here from: {PreviousStateForPut}, Time before put: {realTimeBeforeOp}"
                        };

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

                                    double closestIntake = GetClosestNextIntakeTime();
                                    if (RealTime < closestIntake)
                                    {
                                        RealTime = closestIntake + TimePadding;
                                        step.Message += $", Skipped to intake: {RealTime}";
                                    }
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

                                    double closestIntake = GetClosestNextIntakeTime();
                                    if (RealTime < closestIntake)
                                    {
                                        RealTime = closestIntake + TimePadding;
                                        step.Message += $", Skipped to intake: {RealTime}";
                                    }
                                }
                                break;
                        }
                        step.Message += $", Going to state: {CurrentState}";
                        StepLog.Add(step);
                        break;
                    }
                case AsyncControllerState.Get:
                    {
                        var nearestNeededPosition = GetNearesElementWarehousePosition(ProductionState, ItemState.Empty);
                        var stackerRoundtripForItem = ProductionState[PositionCodes.Stacker, nearestNeededPosition] + ProductionState[nearestNeededPosition, PositionCodes.Stacker] - StackerOperationTime * 2;
                        ProductionState[nearestNeededPosition] = Current;
                        var nextIntake = GetClosestNextIntakeTime();
                        realTimeBeforeOp = RealTime;
                        RealTime += stackerRoundtripForItem;

                        AsyncStepModel step = new AsyncStepModel
                        {
                            CurrentState = AsyncControllerState.Get,
                            Message = $"RealTime: {RealTime}, Current: {Current}, Took item to position: {nearestNeededPosition} with time {stackerRoundtripForItem}, Next intake in: {nextIntake}, Time before put: {realTimeBeforeOp}"
                        };

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
                            step.Message += $", Going to state: {CurrentState}";
                            StepLog.Add(step);
                            break;
                        }

                        double closestOuttake = GetClosestNextOuttakeTime();
                        if (RealTime < closestOuttake)
                        {
                            RealTime = closestOuttake + TimePadding;
                            step.Message += $", Skipped to outtake: {RealTime}";
                        }

                        CurrentState = AsyncControllerState.Put;
                        PreviousStateForPut = AsyncControllerState.Get;
                        Needed = ProductionState.FutureProductionPlan.Peek();
                        step.Message += $", Going to state: {CurrentState}";
                        StepLog.Add(step);
                        break;
                    }
                case AsyncControllerState.End:
                    {
                        AsyncStepModel step = new AsyncStepModel
                        {
                            CurrentState = AsyncControllerState.End,
                            Message = $"RealTime: {RealTime}, Is state ok: {ProductionState.ProductionStateIsOk}"
                        };
                        StepLog.Add(step);
                        return false;
                    }
            }

            ProductionState.StepCounter++;
            return true;
        }

        public override void RenewControllerState()
        {
            base.RenewControllerState();
            RealTime = 0;
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
