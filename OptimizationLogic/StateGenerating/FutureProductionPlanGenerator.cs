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
        public FutureProductionPlanGenerator(double mqbToMebTransitionProbability)
        {
            MqbToMebTransitionProbability = mqbToMebTransitionProbability;
            RandomGenerator = !RandomSeed.HasValue ? new Random() : new Random(RandomSeed.Value);
        }

        public virtual double ComputeFinalMqbToMebTransitionProbability()
        {
            return MqbToMebTransitionProbability;
        }

        public List<ItemState> GenerateSequence(int count)
        {
            var res = new List<ItemState>(count);

            var currentState = RandomGenerator.NextDouble() < ProbabilityOfStartingInMqbState ? ItemState.MQB : ItemState.MEB;

            for (int i = 0; i < count; i++)
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
