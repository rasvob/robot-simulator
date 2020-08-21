using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.StateGenerating
{
    public class ProductionStateGenerator
    {
        
        public ProductionStateGenerator(ProductionHistoryGenerator productionHistoryGenerator, FutureProductionPlanGenerator futureProductionPlanGenerator, double[,] timeMatrix, double mqbDistanceWeight = 0.0, double mebDistanceWeight = 0.0, double uniformProbabilityWeight = 1.0, bool startWithMqb = true)
        {
            ProductionHistoryGenerator = productionHistoryGenerator;
            FutureProductionPlanGenerator = futureProductionPlanGenerator;
            MqbDistanceWeight = mqbDistanceWeight;
            MebDistanceWeight = mebDistanceWeight;
            UniformProbabilityWeight = uniformProbabilityWeight;
            TimeMatrix = timeMatrix;
            RandomGenerator = !RandomSeed.HasValue ? new Random() : new Random(RandomSeed.Value);
            StartWithMqb = startWithMqb;
        }

        public ProductionHistoryGenerator ProductionHistoryGenerator { get; set; }
        public FutureProductionPlanGenerator FutureProductionPlanGenerator { get; set; }
        public int? RandomSeed { get; set; }
        public Random RandomGenerator { get; set; }
        public bool StartWithMqb { get; private set; }
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
            var validPositions = ((PositionCodes[])Enum.GetValues(typeof(PositionCodes))).Where(t => t != PositionCodes.Service && t != PositionCodes.Stacker).Select(t => state.GetWarehouseIndex(t)).ToList();
            var timeMatrixMaximum = state.TimeMatrix.Cast<double>().Max();
            var timeAdvantageForCell = validPositions.Select(t => (t.row, t.col, state.TimeMatrix[t.row, t.col] / timeMatrixMaximum)).ToList();
            int numberOfFreeSlots = MaximumFreeSlotsInWarehouse - state.ProductionHistory.Count;
            int numberOfMqbItems = MaximumOfMqbItems - state.ProductionHistory.Count(t => t == ItemState.MQB);
            int numberOfMebItems = MaximumOfMebItems - state.ProductionHistory.Count(t => t == ItemState.MEB);

            (var item, int limit) = StartWithMqb ? (ItemState.MQB, numberOfMqbItems): (ItemState.MEB, numberOfMebItems);

            for (int i = 0; i < limit; i++)
            {
                double uniformProb = 1.0 / validPositions.Count;

            }

        }
    }
}
