using OptimizationLogic.DTO;
using OptimizationLogic.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.StateGenerating
{
    public class RestrictiveProductionStateGenerator
    {
        public RestrictivePlanGenerator RestrictivePlanGenerator { get; set; }
        public Random RandomGenerator { get => RestrictivePlanGenerator.RandomGenerator; }
        public double DominantDistanceWeight { get; set; } = 1.0;
        public double NonDominantDistanceWeight { get; set; } = 1.0;
        public double UniformProbabilityWeight { get; set; } = 1.0;
        public int MaximumOfDominantItems { get; set; }
        public int MaximumOfNonDominantItems { get; set; }
        public int MaximumFreeSlotsInWarehouse { get => WarehouseRows * WarehouseCols - ProductionState.ForbiddedSpotsCount; }
        public int WarehouseRows { get; set; }
        public int WarehouseCols { get; set; }
        public ItemState DominantItem { get => RestrictivePlanGenerator.DominantItem; }
        public ItemState NonDominantItem { get => RestrictivePlanGenerator.NonDominantItem; }

        public int FutureProductionQueueLength { get; set; }
        public int PastProductionQueueLength { get; set; }

        public RestrictiveProductionStateGenerator(RestrictivePlanGenerator restrictivePlanGenerator, int maximumOfDominantItems, int maximumOfNonDominantItems, int warehouseRows, int warehouseCols, int futureProductionQueueLength, int pastProductionQueueLength)
        {
            RestrictivePlanGenerator = restrictivePlanGenerator;
            MaximumOfDominantItems = maximumOfDominantItems;
            MaximumOfNonDominantItems = maximumOfNonDominantItems;
            WarehouseRows = warehouseRows;
            WarehouseCols = warehouseCols;
            FutureProductionQueueLength = futureProductionQueueLength;
            PastProductionQueueLength = pastProductionQueueLength;
        }

        public ProductionState GenerateProductionState()
        {
            var res = new ProductionState(WarehouseCols, WarehouseRows);
            var sequence = RestrictivePlanGenerator.GenerateSequence(FutureProductionQueueLength + PastProductionQueueLength);
            res.FutureProductionPlan = new Queue<ItemState>(sequence.Take(FutureProductionQueueLength));
            res.ProductionHistory = new Queue<ItemState>(sequence.Skip(FutureProductionQueueLength));
            GenerateWarehouseState(res);
            return res;
        }

        private void GenerateWarehouseState(ProductionState state)
        {
            double DistanceFunc(double time, double maxTime) => (1 + (-time / maxTime));
            double UniformFunc(int validCount) => (1.0 / validCount);
            Dictionary<(int row, int col), double> ComputeTimeAdvantageForCells(List<(int row, int col)> validPositions, double maximum) => validPositions.ToDictionary(k => ((k.row, k.col)), t => DistanceFunc(state.GetDistanceFromStacker(t.row, t.col), maximum));
            var validPositions = ((PositionCodes[])Enum.GetValues(typeof(PositionCodes)))
                   .FilterPositions(state.WarehouseRows, state.WarehouseColls)
                   .Where(t => t != PositionCodes.Service && t != PositionCodes.Stacker)
                   .Select(t => state.GetWarehouseIndex(t))
                   .ToList();

            var timeMatrixMaximum = state.TimeDictionary[PositionCodes.Stacker].Select(t => t.Value).Max();
            var timeAdvantageForCell = ComputeTimeAdvantageForCells(validPositions, timeMatrixMaximum);
            int numberOfDominantItems = MaximumOfDominantItems - state.ProductionHistory.Count(t => t == DominantItem);
            int numberOfNonDominantItems = MaximumOfNonDominantItems - state.ProductionHistory.Count(t => t == NonDominantItem);
            int numberOfFreeSlots = MaximumFreeSlotsInWarehouse - numberOfDominantItems - numberOfNonDominantItems;

            var randomPermutationOfItems = Enumerable.Repeat(NonDominantItem, numberOfNonDominantItems)
                .Concat(Enumerable.Repeat(DominantItem, numberOfDominantItems))
                .OrderBy(_ => RandomGenerator.Next())
                .ToList();

            foreach (var item in randomPermutationOfItems)
            {
                double uniformProb = UniformFunc(validPositions.Count);
                double weight = item == NonDominantItem ? NonDominantDistanceWeight : DominantDistanceWeight;
                var validProbs = validPositions.Select(t => UniformProbabilityWeight * uniformProb + timeAdvantageForCell[(t.row, t.col)] * weight).Softmax();
                //var validProbs = validPositions.Select(t => UniformProbabilityWeight * uniformProb).Softmax();
                var cumsum = validProbs.CumulativeSum().Select((value, index) => (index, value));
                var rndDouble = RandomGenerator.NextDouble();
                var positionIndex = cumsum.FirstOrDefault(t => rndDouble < t.value).index;
                var position = validPositions[positionIndex];
                state.WarehouseState[position.row, position.col] = item;
                validPositions.RemoveAt(positionIndex);
            }
        }
    }
}
