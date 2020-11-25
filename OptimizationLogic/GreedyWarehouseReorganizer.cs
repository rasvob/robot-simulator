using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OptimizationLogic
{
    public class GreedyWarehouseReorganizer
    {
        public int MaxDepth { get; }
        public int SelectBestCnt { get; }
        public event EventHandler<ProgressEventArgs> ProgressTriggered;
        public BaseController Controller { get; set; }

        public GreedyWarehouseReorganizer(BaseController controller, int maxDepth = 5, int selectBestCnt = 1)
        {
            MaxDepth = maxDepth;
            SelectBestCnt = selectBestCnt;
            Controller = controller;
        }

        public void ReorganizeWarehouse(ProductionState productionState, List<BaseStepModel> logger, double reservedTime)
        {
            var bestSwaps = GetBestSwaps(productionState, reservedTime);
            var previousPosition = PositionCodes.Stacker;

            if (bestSwaps.Count == 0)
            {
                logger.Add(new BaseStepModel()
                {
                    Message = "No better warehouse item organization was found during the break"
                });
            }

            for (int i = 0; i < bestSwaps.Count; i++)
            {
                WarehouseSwap(productionState, bestSwaps[i], previousPosition, logger, isLastSwap: i == bestSwaps.Count - 1);
                previousPosition = bestSwaps[i].Item2;
            }
        }

        private void WarehouseSwap(ProductionState productionState, Tuple<PositionCodes, PositionCodes> currentSwap, PositionCodes previousPosition, List<BaseStepModel> logger, bool isLastSwap = false)
        {
            (int r, int c) = productionState.GetWarehouseIndex(currentSwap.Item1);
            var itemType = productionState.WarehouseState[r, c];
            var swapTime = productionState.SwapWarehouseItems(currentSwap.Item1, currentSwap.Item2);
            double moveToDifferentCellTime;

            moveToDifferentCellTime = productionState[previousPosition, currentSwap.Item1];

            if (isLastSwap)
            {
                var extraTime = productionState[currentSwap.Item2, PositionCodes.Stacker];

                logger.Add(new WarehouseSwapStepModel
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
                logger.Add(new WarehouseSwapStepModel
                {
                    MoveTime = moveToDifferentCellTime,
                    SwapFromCell = currentSwap.Item1,
                    SwapTime = swapTime,
                    SwapToCell = currentSwap.Item2,
                    SwapElement = itemType,
                });
            }
        }

        private List<Tuple<PositionCodes, PositionCodes>> GetBestSwaps(ProductionState productionState, double reservedTime)
        {
            Dictionary<int, List<WarehouseReorganizationRecord>> warehouseReorganizationRecordsDict = new Dictionary<int, List<WarehouseReorganizationRecord>>();

            warehouseReorganizationRecordsDict[0] = new List<WarehouseReorganizationRecord>();
            warehouseReorganizationRecordsDict[0].Add(new WarehouseReorganizationRecord
            {
                ProductionState = productionState,
                Swap = null,
                PreviousRecord = null,
                RemainingTime = reservedTime,
                MissingSimulationSteps = SimulateProcessing(productionState, 100)
            });

            if (warehouseReorganizationRecordsDict[0][0].MissingSimulationSteps > 0)
            {
                for (int depthIndex = 0; depthIndex < MaxDepth; depthIndex++)
                {
                    ProgressTriggered?.Invoke(this, new ProgressEventArgs() { State = ProgressState.Update, CurrentValue = depthIndex });
                    warehouseReorganizationRecordsDict[depthIndex + 1] = new List<WarehouseReorganizationRecord>();
                    var warehouseReorganizationRecords = warehouseReorganizationRecordsDict[depthIndex].Where(record => record.RemainingTime > 0).ToList();
                    warehouseReorganizationRecordsDict[depthIndex] = warehouseReorganizationRecords.OrderBy(record => record.MissingSimulationSteps).ThenByDescending(record => record.RemainingTime).Take(SelectBestCnt).ToList();

                    for (int topIndex = 0; topIndex < SelectBestCnt && topIndex < warehouseReorganizationRecordsDict[depthIndex].Count; topIndex++)
                    {
                        var currentRecord = warehouseReorganizationRecordsDict[depthIndex][topIndex];
                        var availableSwaps = currentRecord.ProductionState.GetAvailableWarehouseSwaps();
                        WarehouseReorganizationRecord[] reorganizationRecordArray = new WarehouseReorganizationRecord[availableSwaps.Count];

                        var result = Parallel.For(0, availableSwaps.Count, (i) =>
                        {
                            ProductionState newProductionState = (ProductionState)currentRecord.ProductionState.Clone();
                            var swap = availableSwaps[i];
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
                            var moveTime = productionState[previousPosition, swap.Item1];
                            var timeToStacker = productionState[swap.Item2, PositionCodes.Stacker];
                            var timeRemaining = currentRecord.RemainingTime - moveTime - swapTimeConsumed;

                            if (timeRemaining - timeToStacker > 0)
                            {
                                int numberOfMissingSteps = SimulateProcessing(newProductionState, 100);
                                reorganizationRecordArray[i] = new WarehouseReorganizationRecord
                                {
                                    ProductionState = newProductionState,
                                    Swap = swap,
                                    PreviousRecord = currentRecord,
                                    RemainingTime = timeRemaining,
                                    MissingSimulationSteps = numberOfMissingSteps
                                };
                            }
                        });
                        foreach (var item in reorganizationRecordArray)
                        {
                            warehouseReorganizationRecordsDict[depthIndex + 1].Add(item);
                        }
                    }
                }
            }

            var allRecords = warehouseReorganizationRecordsDict.Values.SelectMany(x => x).ToList();
            var bestRecord = allRecords.OrderBy(record => record.MissingSimulationSteps).ThenByDescending(record => record.RemainingTime).ToList()[0];
            return bestRecord.GetSwapsFromRoot();
        }

        private int SimulateProcessing(ProductionState productionState, int numberOfImagenarySteps)
        {
            ProductionState localProductionState = (ProductionState)productionState.Clone();
            BaseController localController = Controller.CreateNew(localProductionState);
            int counter = 0;
            while (localProductionState.FutureProductionPlan.Count < numberOfImagenarySteps)
            {
                localProductionState.FutureProductionPlan.Enqueue(localProductionState.FutureProductionPlan.ElementAt(counter++));
            }
            while (localController.NextStep() && localController.ProductionState.ProductionStateIsOk && --numberOfImagenarySteps > 0) { }
            return numberOfImagenarySteps;
        }
    }
}
