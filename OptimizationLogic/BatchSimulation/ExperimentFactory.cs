using OptimizationLogic.DTO;
using OptimizationLogic.StateGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.BatchSimulation
{
    public class ExperimentFactory
    {
        public int WarehouseColumns { get; set; }
        public int WarehouseRows { get; set; }
        public int NumberOfFreePositions { get; set; }
        public int NumberOfItemsInPastProductionQueue { get; set; }
        public int NumberOfItemsInFutureProductionQueue { get; set; }
        public bool IsMqbDominant { get; set; }
        public int TimeLimit { get; set; }
        public int ClockTime { get; set; }
        public int MaximumNonDominantItemsInARow { get; set; }
        public int BatchSize { get; set; }
        public bool UseReorganization { get; set; }
        public double ProbabiityOfNonDominantItemAsNextOne { get; set; }
        public double DominantDistanceWeight { get; set; }
        public double NonDominantDistanceWeight { get; set; }
        public double UniformProbabilityWeight { get; set; }
        public bool UseFixedCoefficient { get; set; }
        public bool SequencesAreDeterministic { get; set; }

        public ItemState DominantItem { get => IsMqbDominant ? ItemState.MQB : ItemState.MEB; }
        public ItemState NonDominantItem { get => !IsMqbDominant ? ItemState.MQB : ItemState.MEB; }

        public int NumberOfDominantItems { get => ProductionState.ComputeDominantTypeItemsCount(NumberOfItemsInPastProductionQueue); }
        public int NumberOfNonDominantItems { get => ProductionState.ComputeNonDominantTypeItemsCount(NumberOfItemsInPastProductionQueue, MaximumNonDominantItemsInARow); }

        private readonly int _randomSeed = 13;

        private IEnumerable<GeneratorCoefficients> SampleCoefficients()
        {
            return UseFixedCoefficient switch {
                true => Enumerable.Repeat(new GeneratorCoefficients { DominantDistanceWeight = DominantDistanceWeight, NonDominantDistanceWeight = NonDominantDistanceWeight, ProbabiityOfNonDominantItemAsNextOne = ProbabiityOfNonDominantItemAsNextOne, UniformProbabilityWeight = UniformProbabilityWeight }, BatchSize),
                false => Enumerable.Repeat(0, BatchSize).Select(t => {  }),
            };
        }

        public ExperimentConfig CreateExperimentConfig()
        {
            Random sequenceRandom = SequencesAreDeterministic ? new Random(_randomSeed) : new Random();
            Random weightRandom = SequencesAreDeterministic ? new Random(_randomSeed) : new Random();
            var res = new ExperimentConfig { ClockTime = ClockTime, TimeLimit = TimeLimit, UseReorganization = UseReorganization };
            var sequenceGenerator = new RestrictivePlanGenerator(DominantItem, NonDominantItem, MaximumNonDominantItemsInARow > 0 ? MaximumNonDominantItemsInARow : NumberOfItemsInPastProductionQueue, sequenceRandom);
            var productionStateGenerator = new RestrictiveProductionStateGenerator(sequenceGenerator, NumberOfDominantItems, NumberOfNonDominantItems, WarehouseRows, WarehouseColumns, NumberOfItemsInFutureProductionQueue, NumberOfItemsInPastProductionQueue);



            return res;
        }

        internal class GeneratorCoefficients
        {
            public double ProbabiityOfNonDominantItemAsNextOne { get; set; }
            public double DominantDistanceWeight { get; set; }
            public double NonDominantDistanceWeight { get; set; }
            public double UniformProbabilityWeight { get; set; }
        }
    }
}
