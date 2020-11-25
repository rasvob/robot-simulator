using OptimizationLogic.DTO;
using OptimizationLogic.StateGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private readonly int _randomWeightLowerBound = -5;
        private readonly int _randomWeightHigherBound = 6;
        private readonly int _randomWeightHigherBoundUniform = 5;

        public CancellationToken CancellationToken { get; set; }

        private IEnumerable<GeneratorCoefficients> SampleCoefficients(Random random)
        {
            return UseFixedCoefficient switch {
                true => Enumerable.Repeat(new GeneratorCoefficients { DominantDistanceWeight = DominantDistanceWeight, NonDominantDistanceWeight = NonDominantDistanceWeight, ProbabiityOfNonDominantItemAsNextOne = ProbabiityOfNonDominantItemAsNextOne, UniformProbabilityWeight = UniformProbabilityWeight }, BatchSize),
                false => Enumerable.Repeat(0, BatchSize).Select(_ => new GeneratorCoefficients { UniformProbabilityWeight =  random.Next(0, _randomWeightHigherBoundUniform), ProbabiityOfNonDominantItemAsNextOne = random.NextDouble(), DominantDistanceWeight = random.Next(_randomWeightLowerBound, _randomWeightHigherBound), NonDominantDistanceWeight = random.Next(_randomWeightLowerBound, _randomWeightHigherBound) }),
            };
        }

        public ExperimentConfig CreateExperimentConfig()
        {
            Random sequenceRandom = SequencesAreDeterministic ? new Random(_randomSeed) : new Random();
            Random weightRandom = SequencesAreDeterministic ? new Random(_randomSeed) : new Random();
            var res = new ExperimentConfig { ClockTime = ClockTime, TimeLimit = TimeLimit, UseReorganization = UseReorganization };
            var sequenceGenerator = new RestrictivePlanGenerator(DominantItem, NonDominantItem, MaximumNonDominantItemsInARow > 0 ? MaximumNonDominantItemsInARow : NumberOfItemsInPastProductionQueue, sequenceRandom);
            var productionStateGenerator = new RestrictiveProductionStateGenerator(sequenceGenerator, NumberOfDominantItems, NumberOfNonDominantItems, WarehouseRows, WarehouseColumns, NumberOfItemsInFutureProductionQueue, NumberOfItemsInPastProductionQueue);
            res.ProductionStates.AddRange(SampleCoefficients(weightRandom).Select(item => {
                CancellationToken.ThrowIfCancellationRequested();
                sequenceGenerator.DominantToNonDominantTransitionProbability = item.ProbabiityOfNonDominantItemAsNextOne;
                productionStateGenerator.UniformProbabilityWeight = item.UniformProbabilityWeight;
                productionStateGenerator.DominantDistanceWeight = item.DominantDistanceWeight;
                productionStateGenerator.NonDominantDistanceWeight = item.NonDominantDistanceWeight;
                return productionStateGenerator.GenerateProductionState();
            }));
            res.ProductionStatesBackup.AddRange(res.ProductionStates.Select(t => {
                CancellationToken.ThrowIfCancellationRequested();
                return (ProductionState)t.Clone(); }));
            return res;
        }

        private class GeneratorCoefficients
        {
            public double ProbabiityOfNonDominantItemAsNextOne { get; set; }
            public double DominantDistanceWeight { get; set; }
            public double NonDominantDistanceWeight { get; set; }
            public double UniformProbabilityWeight { get; set; }
        }
    }
}
