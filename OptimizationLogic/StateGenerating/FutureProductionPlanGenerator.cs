using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.StateGenerating
{
    public class FutureProductionPlanGenerator
    {
        public double MqbToMebTransitionProbability { get; set; } = 0.5;
        public double ProbabilityOfStartingInMqbState { get; set; } = 1.0;
        public int? RandomSeed { get; set; }
        public Random RandomGenerator { get; set; }
        public int SequenceLength { get; set; } = 100;

        public FutureProductionPlanGenerator(double mqbToMebTransitionProbability, int sequenceLength = 100)
        {
            MqbToMebTransitionProbability = mqbToMebTransitionProbability;
            RandomGenerator = !RandomSeed.HasValue ? new Random() : new Random(RandomSeed.Value);
            SequenceLength = sequenceLength;
        }

        public virtual double ComputeFinalMqbToMebTransitionProbability()
        {
            return MqbToMebTransitionProbability;
        }

        public List<ItemState> GenerateSequence()
        {
            var res = new List<ItemState>(SequenceLength);

            var currentState = RandomGenerator.NextDouble() < ProbabilityOfStartingInMqbState ? ItemState.MQB : ItemState.MEB;

            for (int i = 0; i < SequenceLength; i++)
            {
                res.Add(currentState);
                currentState = currentState switch
                {
                    ItemState.MEB => ItemState.MQB,
                    ItemState.MQB => RandomGenerator.NextDouble() < ComputeFinalMqbToMebTransitionProbability() ? ItemState.MEB : ItemState.MQB,
                    _ => ItemState.MQB
                };
            }

            return res;
        }
    }
}
