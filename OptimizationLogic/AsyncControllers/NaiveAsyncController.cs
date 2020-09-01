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
        public double TimeBase { get => _timeBaseShift + Delay; }
        private readonly double _timeBaseShift = 9;
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

        public double GetClosestNextIntakeTime()
        {
            if (RealTime < _timeBaseShift)
            {
                return _timeBaseShift;
            }

            int currentIntakeSteps = (int)((RealTime - TimeBase) / IntakeClock);
            return TimeBase + (currentIntakeSteps + 1) * IntakeClock;
        }

        public double GetClosestNextOuttakeTime()
        {
            if (RealTime < TimeBase + IntakeOuttakeDifference)
            {
                return TimeBase + IntakeOuttakeDifference;
            }

            int currentIntakeSteps = (int)((RealTime - (TimeBase + IntakeOuttakeDifference)) / IntakeClock);
            return (currentIntakeSteps+1) * IntakeClock + TimeBase + IntakeOuttakeDifference;
        }

        public override bool NextStep()
        {
            if (ProductionState.FutureProductionPlan.Count == 0)
            {
                CurrentState = AsyncControllerState.End;
                StepLog.Add(new AsyncStepModel
                {
                    CurrentState = AsyncControllerState.End,
                    Message = $"RealTime: {RealTime}, No more items to process, Going to state: {CurrentState}"
                });
                return true;
            }

            //History.Push(ProductionState.Copy());

            switch (CurrentState)
            {
                case AsyncControllerState.Start:
                    {
                        StartHandler();
                        break;
                    }
                case AsyncControllerState.SwapChain:
                    {
                        SwapChainHandler();
                        break;
                    }
                case AsyncControllerState.Put:
                    {
                        PutHandler();
                        break;
                    }
                case AsyncControllerState.Get:
                    {
                        GetHandler();
                        break;
                    }
                case AsyncControllerState.End:
                    {
                        EndHandler();
                        return false;
                    }
            }

            ProductionState.StepCounter++;
            return true;
        }

        public override void RenewControllerState()
        {
            base.RenewControllerState();
            IntakeItem = ItemState.Empty;
            OuttakeItem = ItemState.Empty;
            CurrentState = AsyncControllerState.Start;
        }

        public override bool CanUndo()
        {
            return false;
        }

        public virtual void EndHandler()
        {
            AsyncStepModel step = new AsyncStepModel
            {
                CurrentState = AsyncControllerState.End,
                Message = $"RealTime: {RealTime}, Is state ok: {ProductionState.ProductionStateIsOk}"
            };
            StepLog.Add(step);
        }

        public virtual void SwapChainHandler()
        {
            OuttakeItem = IntakeItem;
            IntakeItem = ItemState.Empty;
            var realTimeBeforeOp = RealTime;
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
        }

        public virtual void StartHandler()
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
        }

        public virtual void PutHandler()
        {
            var nearestNeededPosition = GetNearesElementWarehousePosition(ProductionState, Needed);
            var stackerRoundtripForItem = ProductionState[PositionCodes.Stacker, nearestNeededPosition] + ProductionState[nearestNeededPosition, PositionCodes.Stacker] - StackerOperationTime * 2;
            ProductionState[nearestNeededPosition] = ItemState.Empty;
            var realTimeBeforeOp = RealTime;
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
                            RealTime = closestIntake;
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
                            RealTime = closestIntake;
                            step.Message += $", Skipped to intake: {RealTime}";
                        }
                    }
                    break;
            }
            step.Message += $", Going to state: {CurrentState}";
            StepLog.Add(step);
        }

        public virtual void GetHandler()
        {
            var nearestNeededPosition = GetNearesElementWarehousePosition(ProductionState, ItemState.Empty);
            var stackerRoundtripForItem = ProductionState[PositionCodes.Stacker, nearestNeededPosition] + ProductionState[nearestNeededPosition, PositionCodes.Stacker] - StackerOperationTime * 2;
            ProductionState[nearestNeededPosition] = Current;
            var nextIntake = GetClosestNextIntakeTime();
            var realTimeBeforeOp = RealTime;
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
                return;
            }

            ProductionState.ProductionHistory.Dequeue();
            if (ProductionState.FutureProductionPlan.Peek() == ProductionState.ProductionHistory.Peek())
            {
                CurrentState = AsyncControllerState.Start;
                RealTime = nextIntake;
                IntakeItem = ItemState.Empty;
                step.Message += $", Going to state: {CurrentState}";
                StepLog.Add(step);
                return;
            }

            double closestOuttake = GetClosestNextOuttakeTime();
            if (RealTime < closestOuttake)
            {
                RealTime = closestOuttake;
                step.Message += $", Skipped to outtake: {RealTime}";
            }

            CurrentState = AsyncControllerState.Put;
            PreviousStateForPut = AsyncControllerState.Get;
            Needed = ProductionState.FutureProductionPlan.Peek();
            step.Message += $", Going to state: {CurrentState}";
            StepLog.Add(step);
        }
    }
}
