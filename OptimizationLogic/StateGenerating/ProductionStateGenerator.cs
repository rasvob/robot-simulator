using OptimizationLogic.DTO;
using OptimizationLogic.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.StateGenerating
{
    public class ProductionStateGenerator
    {
        public ProductionStateGenerator(ProductionHistoryGenerator productionHistoryGenerator, FutureProductionPlanGenerator futureProductionPlanGenerator, double[,] timeMatrix, double mqbDistanceWeight = 0.0, double mebDistanceWeight = 0.0, double uniformProbabilityWeight = 1.0)
        {
            ProductionHistoryGenerator = productionHistoryGenerator;
            FutureProductionPlanGenerator = futureProductionPlanGenerator;
            MqbDistanceWeight = mqbDistanceWeight;
            MebDistanceWeight = mebDistanceWeight;
            UniformProbabilityWeight = uniformProbabilityWeight;
            TimeMatrix = timeMatrix;
            RandomGenerator = !RandomSeed.HasValue ? new Random() : new Random(RandomSeed.Value);
        }

        public ProductionHistoryGenerator ProductionHistoryGenerator { get; set; }
        public FutureProductionPlanGenerator FutureProductionPlanGenerator { get; set; }
        public int? RandomSeed { get; set; }
        public Random RandomGenerator { get; set; }
        public double MqbDistanceWeight { get; set; }
        public double MebDistanceWeight { get; set; }
        public double UniformProbabilityWeight { get; set; }
        public double[,] TimeMatrix { get; set; }

        public int MaximumOfMqbItems { get; private set; } = 65;
        public int MaximumOfMebItems { get; private set; } = 35;
        public int MaximumFreeSlotsInWarehouse { get; private set; } = 44;

        public ProductionState GenerateProductionState()
        {
            var res = new ProductionState() { TimeMatrix = TimeMatrix };
            res.FutureProductionPlan = new Queue<ItemState>(FutureProductionPlanGenerator.GenerateSequence());
            res.ProductionHistory = new Queue<ItemState>(ProductionHistoryGenerator.GenerateSequence());
            GenerateWarehouseState(res);
            return res;
        }

        private void GenerateWarehouseState(ProductionState state)
        {
            double DistanceFunc(double time, double maxTime) => (1 + (-time/maxTime));
            double UniformFunc(int validCount) => (1.0 / validCount);
            Dictionary<(int row, int col), double> ComputeTimeAdvantageForCells(List<(int row, int col)> validPositions, double maximum) => validPositions.ToDictionary(k => ((k.row, k.col)), t => DistanceFunc(state.TimeMatrix[t.row, t.col], maximum));
            List<double> ComputeSoftmax(IEnumerable<double> exponents) {
                var exp = exponents.Select(Math.Exp);
                double sum = exp.Sum();
                return exp.Select(t => t / sum).ToList();
            }


            var validPositions = ((PositionCodes[])Enum.GetValues(typeof(PositionCodes)))
                   .Where(t => t != PositionCodes.Service && t != PositionCodes.Stacker)
                   .Select(t => state.GetWarehouseIndex(t))
                   .ToList();

            var timeMatrixMaximum = state.TimeMatrix.Cast<double>().Max();
            var timeAdvantageForCell = ComputeTimeAdvantageForCells(validPositions, timeMatrixMaximum);
            int numberOfMqbItems = MaximumOfMqbItems - state.ProductionHistory.Count(t => t == ItemState.MQB);
            int numberOfMebItems = MaximumOfMebItems - state.ProductionHistory.Count(t => t == ItemState.MEB);
            int numberOfFreeSlots = MaximumFreeSlotsInWarehouse - numberOfMqbItems - numberOfMebItems;

            var randomPermutationOfItems = Enumerable.Repeat(ItemState.MEB, numberOfMebItems)
                .Concat(Enumerable.Repeat(ItemState.MQB, numberOfMqbItems))
                .OrderBy(_ => RandomGenerator.Next())
                .ToList();

            foreach(var item in randomPermutationOfItems)
            {
                double uniformProb = UniformFunc(validPositions.Count);
                double weight = item == ItemState.MEB ? MebDistanceWeight : MqbDistanceWeight;
                var validProbs = validPositions.Select(t => UniformProbabilityWeight * uniformProb + timeAdvantageForCell[(t.row, t.col)] * weight).Softmax();
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
