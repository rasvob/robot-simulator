using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic
{
    public class GreedyWarehouseOptimizationController : BaseController
    {
        new private const int TimeLimit = 36;
        private int MaxDepth;
        private int SelectBestCnt;
        private List<Tuple<PositionCodes, PositionCodes>> reorganizationSwaps = null;
        private int reorganizationSwapsCurrentIndex;

        public GreedyWarehouseOptimizationController(ProductionState productionState, string csvProcessingTimeMatrix, string csvWarehouseInitialState, string csvHistroicalProduction, string csvFutureProductionPlan, int maxDepth=5, int selectBestCnt=5) : base(productionState, csvProcessingTimeMatrix, csvWarehouseInitialState, csvHistroicalProduction, csvFutureProductionPlan)
        {
            MaxDepth = maxDepth;
            SelectBestCnt = selectBestCnt;
        }

        public override bool NextStep()
        {
            if (ProductionState.FutureProductionPlan.Count == 0)
            {
                return false;
            }

            if (reorganizationSwaps != null)
            {
                if (reorganizationSwaps.Count >0)
                {
                    WarehouseSwap();
                    // TODO: validate how to work with step counter
                    ProductionState.StepCounter++;
                } else
                {
                    reorganizationSwaps = null;
                    // TODO: validate how to work with step counter
                    ProductionState.StepCounter++;
                }
            }
            else if ((ProductionState.StepCounter - 1) % 50 == 0)
            {
                reorganizationSwaps = GetBestSwaps(300, MaxDepth, SelectBestCnt);
                reorganizationSwapsCurrentIndex = 0;
            }
            else
            {
                NaiveNextStep(ProductionState, StepLog);
            }

            return true;
        }

        private void WarehouseSwap()
        {
            var currentSwap = reorganizationSwaps[reorganizationSwapsCurrentIndex];
            (int r, int c) = ProductionState.GetWarehouseIndex(currentSwap.Item1);
            var itemType = ProductionState.WarehouseState[r, c];
            var swapTime = ProductionState.SwapWarehouseItems(currentSwap.Item1, currentSwap.Item2);
            double moveToDifferentCellTime;

            if (reorganizationSwapsCurrentIndex == 0)
            {
                moveToDifferentCellTime = ProductionState.TimeMatrix[
                    ProductionState.GetTimeMatrixIndex(PositionCodes.Stacker),
                    ProductionState.GetTimeMatrixIndex(currentSwap.Item1)
                    ];
            }
            else
            {
                var previousSwap = reorganizationSwaps[reorganizationSwapsCurrentIndex - 1];
                moveToDifferentCellTime = ProductionState.TimeMatrix[
                    ProductionState.GetTimeMatrixIndex(previousSwap.Item2),
                    ProductionState.GetTimeMatrixIndex(currentSwap.Item1)
                    ];
            }

            if (reorganizationSwapsCurrentIndex == reorganizationSwaps.Count-1)
            {
                var extraTime = ProductionState.TimeMatrix[
                    ProductionState.GetTimeMatrixIndex(currentSwap.Item2),
                    ProductionState.GetTimeMatrixIndex(PositionCodes.Stacker)
                    ];
                reorganizationSwaps = null;

                StepLog.Add(new WarehouseSwapStepModel
                {
                    MoveTime = moveToDifferentCellTime,
                    SwapFromCell = currentSwap.Item1,
                    SwapTime = swapTime,
                    SwapToCell = currentSwap.Item2,
                    SwapElement = itemType,
                    ExtraTime = extraTime
                });
            }
            else
            {
                StepLog.Add(new WarehouseSwapStepModel
                {
                    MoveTime = moveToDifferentCellTime,
                    SwapFromCell = currentSwap.Item1,
                    SwapTime = swapTime,
                    SwapToCell = currentSwap.Item2,
                    SwapElement = itemType,
                });
            }
            reorganizationSwapsCurrentIndex++;
        }

        public void NaiveNextStep(ProductionState actualProductionState, List<BaseStepModel> logger=null)
        {
            var needed = actualProductionState.FutureProductionPlan.Dequeue();
            var current = actualProductionState.ProductionHistory.Dequeue();
            actualProductionState.ProductionHistory.Enqueue(needed);

            var nearestFreePosition = GetNearestEmptyPosition(actualProductionState);
            (int r, int c) = actualProductionState.GetWarehouseIndex(nearestFreePosition);
            actualProductionState.WarehouseState[r, c] = current;

            var nearestNeededPosition = GetNearesElementWarehousePosition(actualProductionState, needed);
            // Try to optimize moving time
            //var nearestNeededPosition = GetNearesElementWarehousePosition(actualProductionState, nearestFreePosition, needed);
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

        private List<Tuple<PositionCodes, PositionCodes>> GetBestSwaps(int reservedTime, int maxDepth, int selectBestCnt)
        {
            Dictionary<int, List<WarehouseReorganizationRecord>> warehouseReorganizationRecordsDict = new Dictionary<int, List<WarehouseReorganizationRecord>>();

            warehouseReorganizationRecordsDict[0] = new List<WarehouseReorganizationRecord>();
            warehouseReorganizationRecordsDict[0].Add(new WarehouseReorganizationRecord
            {
                ProductionState = ProductionState,
                Swap = null,
                PreviousRecord = null,
                RemainingTime = reservedTime,
                MissingSimulationSteps = SimulateProcessing(ProductionState, 100)
            });

            for (int depthIndex = 0; depthIndex < maxDepth; depthIndex++)
            {
                //Console.WriteLine(String.Format("Processing records in depth {0} ...", depthIndex));
                warehouseReorganizationRecordsDict[depthIndex + 1] = new List<WarehouseReorganizationRecord>();
                var warehouseReorganizationRecords = warehouseReorganizationRecordsDict[depthIndex].Where(record => record.RemainingTime > 0).ToList();
                warehouseReorganizationRecords = warehouseReorganizationRecords.OrderBy(record => record.MissingSimulationSteps).ThenByDescending(record => record.RemainingTime).ToList();

                for (int topIndex = 0; topIndex < selectBestCnt && topIndex < warehouseReorganizationRecords.Count; topIndex++)
                {
                    var currentRecord = warehouseReorganizationRecords[topIndex];
                    var availableSwaps = currentRecord.ProductionState.GetAvailableWarehouseSwaps();
                    foreach (var swap in availableSwaps)
                    {
                        ProductionState newProductionState = (ProductionState)currentRecord.ProductionState.Clone();
                        var swapTimeConsumed = newProductionState.SwapWarehouseItems(swap.Item1, swap.Item2);
                        PositionCodes previousPosition;
                        if (currentRecord.PreviousRecord == null)
                        {
                            previousPosition = PositionCodes.Stacker;
                        }
                        else
                        {
                            if (currentRecord.PreviousRecord.Swap == null)
                            {
                                previousPosition = PositionCodes.Stacker;
                            }
                            else
                            {
                                previousPosition = currentRecord.PreviousRecord.Swap.Item2;
                            }                            
                        }
                        var moveTime = ProductionState.TimeMatrix[ProductionState.GetTimeMatrixIndex(previousPosition), ProductionState.GetTimeMatrixIndex(swap.Item1)];
                        var timeToStacker = ProductionState.TimeMatrix[ProductionState.GetTimeMatrixIndex(swap.Item2), ProductionState.GetTimeMatrixIndex(PositionCodes.Stacker)];
                        var timeRemaining = currentRecord.RemainingTime - moveTime - swapTimeConsumed;

                        if (timeRemaining-timeToStacker > 0)
                        {
                            int numberOfMissingSteps = SimulateProcessing(newProductionState, 100);

                            warehouseReorganizationRecordsDict[depthIndex + 1].Add(new WarehouseReorganizationRecord
                            {
                                ProductionState = newProductionState,
                                Swap = swap,
                                PreviousRecord = currentRecord,
                                RemainingTime = timeRemaining,
                                MissingSimulationSteps = numberOfMissingSteps
                            });
                        }
                        
                    }
                }
            }

            var allRecords = warehouseReorganizationRecordsDict.Values.SelectMany(x => x).ToList();
            var bestRecord = allRecords.OrderBy(record => record.MissingSimulationSteps).ThenByDescending(record => record.RemainingTime).ToList()[0];
            //Console.WriteLine(String.Format("Missing simulation steps: {0}, remaining time: {1}", bestRecord.MissingSimulationSteps, bestRecord.RemainingTime));
            //Console.WriteLine(bestRecord.PrintSwapsFromRoot());

            return bestRecord.GetSwapsFromRoot();
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
