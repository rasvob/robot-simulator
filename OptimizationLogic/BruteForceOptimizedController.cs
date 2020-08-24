using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic
{
    public class BruteForceOptimizedController : BaseController
    {
        new private const int TimeLimit = 55;

        public BruteForceOptimizedController(ProductionState productionState, string csvProcessingTimeMatrix, string csvWarehouseInitialState, string csvHistroicalProduction, string csvFutureProductionPlan) : base(productionState, csvProcessingTimeMatrix, csvWarehouseInitialState, csvHistroicalProduction, csvFutureProductionPlan)
        {
        }

        public override bool NextStep()
        {
            if (ProductionState.FutureProductionPlan.Count == 0)
            {
                return false;
            }

            if ((ProductionState.StepCounter - 1) % 50 == 0)
            {
                ReorganizeWarehouse(300);
            }
            else
            {
                NaiveNextStep(ProductionState, StepLog);
            }

            return true;
        }

        public void NaiveNextStep(ProductionState actualProductionState, List<StepModel> logger=null)
        {
            var needed = actualProductionState.FutureProductionPlan.Dequeue();
            var current = actualProductionState.ProductionHistory.Dequeue();
            actualProductionState.ProductionHistory.Enqueue(needed);

            var nearestFreePosition = GetNearestEmptyPosition(actualProductionState);
            (int r, int c) = actualProductionState.GetWarehouseIndex(nearestFreePosition);
            actualProductionState.WarehouseState[r, c] = current;

            var nearestNeededPosition = GetNearesElementWarehousePosition(actualProductionState, needed);
            (r, c) = ProductionState.GetWarehouseIndex(nearestNeededPosition);
            actualProductionState.WarehouseState[r, c] = ItemState.Empty;

            var insertTime = actualProductionState.TimeMatrix[actualProductionState.GetTimeMatrixIndex(PositionCodes.Stacker), actualProductionState.GetTimeMatrixIndex(nearestFreePosition)];
            var moveToDifferentCellTime = actualProductionState.TimeMatrix[actualProductionState.GetTimeMatrixIndex(nearestFreePosition), actualProductionState.GetTimeMatrixIndex(nearestNeededPosition)] - 5; // TODO: Validate this calculation
            var withdrawTime = actualProductionState.TimeMatrix[actualProductionState.GetTimeMatrixIndex(nearestNeededPosition), actualProductionState.GetTimeMatrixIndex(PositionCodes.Stacker)];
            var totalTime = insertTime + moveToDifferentCellTime + withdrawTime;

            actualProductionState.ProductionStateIsOk = totalTime <= TimeLimit;
            actualProductionState.StepCounter++;

            if (logger != null)
            {
                logger.Add(new StepModel
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
            
        }

        private void ReorganizeWarehouse(int reservedTime)
        {
            ProductionState initialProductionState = (ProductionState)ProductionState.Clone();
            //List<StepModel> log = new List<StepModel>();

            //ReorganizeWarehouseRecursion(initialProductionState, reservedTime, 100);            
            RegorganizeMethod(reservedTime);
        }

        private void RegorganizeMethod(int reservedTime)
        {
            List<WarehouseReorganizationRecord> warehouseReorganizationRecords = new List<WarehouseReorganizationRecord>();

            warehouseReorganizationRecords.Add(new WarehouseReorganizationRecord
            {
                ProductionState = ProductionState,
                Swap = null,
                PreviousRecord = null,
                RemainingTime = reservedTime,
                MissingSimulationSteps = SimulateProcessing(ProductionState, 100)
            });

            int index = 0;
            while (index < warehouseReorganizationRecords.Count)
            {
                var currentRecord = warehouseReorganizationRecords[index];

                if (currentRecord.RemainingTime > 0
                    && (currentRecord.PreviousRecord == null || currentRecord.MissingSimulationSteps < currentRecord.PreviousRecord.MissingSimulationSteps)
                    )
                {
                    var availableSwaps = currentRecord.ProductionState.GetAvailableWarehouseSwaps();
                    foreach (var swap in availableSwaps)
                    {
                        ProductionState newProductionState = (ProductionState)currentRecord.ProductionState.Clone();
                        var swapTimeConsumed = newProductionState.SwapWarehouseItems(swap.Item1, swap.Item2);
                        int numberOfMissingSteps = SimulateProcessing(newProductionState, 100);

                        Console.WriteLine(numberOfMissingSteps);

                        warehouseReorganizationRecords.Add(new WarehouseReorganizationRecord
                        {
                            ProductionState = newProductionState,
                            Swap = swap,
                            PreviousRecord = currentRecord,
                            RemainingTime = currentRecord.RemainingTime - swapTimeConsumed - 100,
                            MissingSimulationSteps = numberOfMissingSteps
                        });
                    }
                }
            }
        }

        private void ReorganizeWarehouseRecursion(ProductionState productionState, int reservedTime, int numberOfMissingStepsToBeat)
        {
            if (reservedTime <= 0)
            {
                return;
            }

            var availableSwaps = productionState.GetAvailableWarehouseSwaps();
            foreach (var swap in availableSwaps)
            {
                ProductionState currentProductionState = (ProductionState)productionState.Clone();
                var timeConsumed = currentProductionState.SwapWarehouseItems(swap.Item1, swap.Item2);
                //Console.WriteLine(swap);

                int numberOfMissingSteps = SimulateProcessing(currentProductionState, 100);

                if (numberOfMissingSteps < numberOfMissingStepsToBeat)
                {
                    Console.WriteLine("Missing steps {0}", numberOfMissingSteps);
                    ReorganizeWarehouseRecursion(currentProductionState, reservedTime - 100, numberOfMissingSteps);
                }
            }
        }

        private int SimulateProcessing(ProductionState productionState, int numberOfImagenarySteps)
        {
            ProductionState localProductionState = (ProductionState)productionState.Clone();
            int counter = 0;
            while (localProductionState.FutureProductionPlan.Count < numberOfImagenarySteps)
            {
                localProductionState.FutureProductionPlan.Enqueue(localProductionState.FutureProductionPlan.ElementAt(counter++));
            }


            while (localProductionState.ProductionStateIsOk && localProductionState.FutureProductionPlan.Count > 0 && --numberOfImagenarySteps > 0)
            {
                NaiveNextStep(localProductionState);
            }
            return numberOfImagenarySteps;
        }
    }
}
