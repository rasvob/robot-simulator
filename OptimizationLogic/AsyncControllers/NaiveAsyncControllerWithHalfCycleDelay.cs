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
        public NaiveAsyncControllerWithHalfCycleDelay(ProductionState productionState, string csvWarehouseInitialState, string csvHistroicalProduction, string csvFutureProductionPlan) : base(productionState, csvWarehouseInitialState, csvHistroicalProduction, csvFutureProductionPlan)
        {
        }

        public NaiveAsyncControllerWithHalfCycleDelay(ProductionState state) : base(state)
        {
        }

        public NaiveAsyncControllerWithHalfCycleDelay(ProductionState state, int tactTime, int operationLimit) : base(state, tactTime, operationLimit)
        {
        }


        public double GetPreviousOuttakeTime()
        {
            if (RealTime < TimeBase + IntakeOuttakeDifference)
            {
                return TimeBase + IntakeOuttakeDifference;
            }

            int currentIntakeSteps = ProductionState.InitialFutureProductionPlanLen - ProductionState.FutureProductionPlan.Count;
            return (currentIntakeSteps-1) * IntakeClock + InitialIntakeTime + IntakeOuttakeDifference + BreakTime;
        }

        public double GetNextOuttakeTime()
        {
            return GetPreviousOuttakeTime() + IntakeClock;
        }

        public override void PutHandler()
        {
            var nearestNeededPosition = GetNearesElementWarehousePosition(ProductionState, Needed);
            var stackerRoundtripForItem = ProductionState[PositionCodes.Stacker, nearestNeededPosition] + ProductionState[nearestNeededPosition, PositionCodes.Stacker] - StackerOperationTime * 2;
            ProductionState[nearestNeededPosition] = ItemState.Empty;
            var realTimeBeforeOp = RealTime;
            var nextOuttake = GetClosestNextOuttakeTime();
            var nextIntake = GetClosestNextIntakeTime();
            RealTime += stackerRoundtripForItem;
            Current = ProductionState.ProductionHistory.Peek();
            IsReadyForBreak = false;

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

                        //double closestIntake = GetClosestNextIntakeTime();
                        double closestIntake = nextIntake;
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
                    
                    if (RealTime > GetNextOuttakeTime())
                    {
                        Delay += RealTime - nextOuttake;
                    }
                    if (RealTime <= nextIntake)
                    {
                        //RealTime = GetClosestNextIntakeTime();
                        RealTime = nextIntake;
                        step.Message += $", Skipped to intake: {RealTime}";
                    }
                    
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
                        double previousIntake = GetPreviousOuttakeTime() - IntakeOuttakeDifference;
                        if (RealTime < closestIntake & RealTime < previousIntake)
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
            IsReadyForBreak = true;

            AsyncStepModel step = new AsyncStepModel
            {
                CurrentState = AsyncControllerState.Get,
                Message = $"RealTime: {RealTime}, Current: {Current}, Took item to position: {nearestNeededPosition} with time {stackerRoundtripForItem}, Next intake in: {nextIntake}, Time before get: {realTimeBeforeOp}"
            };

            /*if (RealTime > nextIntake)
            {
                Delay += RealTime - nextIntake;
                nextIntake = GetClosestNextIntakeTime();
            }*/

            ProductionState.ProductionHistory.Dequeue();
            if (ProductionState.FutureProductionPlan.Peek() == ProductionState.ProductionHistory.Peek())
            {
                CurrentState = AsyncControllerState.Start;
                RealTime = nextIntake;
                IntakeItem = ItemState.Empty;
                step.Message += $", Going to state: {CurrentState}, Skipped to intake {nextIntake}";
                StepLog.Add(step);
                return;
            }

            double closestOuttake = GetClosestNextOuttakeTime();
            double previousOuttake = GetPreviousOuttakeTime();
            if (RealTime < closestOuttake && RealTime < previousOuttake)
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
        public override BaseController CreateNew(ProductionState state)
        {
            NaiveAsyncControllerWithHalfCycleDelay controller = new NaiveAsyncControllerWithHalfCycleDelay(state);
            controller.SetControllerTimes(this.ClockTime, this.TimeLimit);
            return controller;
        }
    }
}
