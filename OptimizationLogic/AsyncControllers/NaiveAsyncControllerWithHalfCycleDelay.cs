using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.AsyncControllers
{
    public class NaiveAsyncControllerWithHalfCycleDelay: NaiveAsyncController
    {
        public NaiveAsyncControllerWithHalfCycleDelay(ProductionState productionState, string csvProcessingTimeMatrix, string csvWarehouseInitialState, string csvHistroicalProduction, string csvFutureProductionPlan) : base(productionState, csvProcessingTimeMatrix, csvWarehouseInitialState, csvHistroicalProduction, csvFutureProductionPlan)
        {
        }

        public NaiveAsyncControllerWithHalfCycleDelay(ProductionState state, string csvProcessingTimeMatrix) : base(state, csvProcessingTimeMatrix)
        {
        }

        public NaiveAsyncControllerWithHalfCycleDelay(ProductionState state) : base(state)
        {
        }

        public override void PutHandler()
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
                        Delay += RealTime - nextOuttake;
                        CurrentState = AsyncControllerState.Get;
                        nextOuttake = GetClosestNextOuttakeTime();
                    }

                    if (RealTime <= nextOuttake)
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
                        Delay += RealTime - nextOuttake;
                        CurrentState = AsyncControllerState.Get;
                        nextOuttake = GetClosestNextOuttakeTime();
                    }

                    if (RealTime <= nextOuttake)
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

        public override void GetHandler()
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
                Delay += RealTime - nextIntake;
                nextIntake = GetClosestNextIntakeTime();
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
