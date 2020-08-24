using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.StateGenerating
{
    public class ProductionHistoryGenerator : FutureProductionPlanGenerator
    {
        private int _numberOfUsedMebItems;
        public int MaximumNumberOfMebItems { get; set; } = 35;

        public ProductionHistoryGenerator(double mqbToMebTransitionProbability, int sequenceLength=64) : base(mqbToMebTransitionProbability, sequenceLength)
        {

        }
        
        public override double ComputeFinalMqbToMebTransitionProbability()
        {
            return MqbToMebTransitionProbability * (_numberOfUsedMebItems == MaximumNumberOfMebItems ? 0.0 : 1.0);
        }

        new public List<ItemState> GenerateSequence()
        {
            _numberOfUsedMebItems = 0;
            var res = new List<ItemState>(SequenceLength);

            var currentState = RandomGenerator.NextDouble() < ProbabilityOfStartingInMqbState ? ItemState.MQB : ItemState.MEB;

            for (int i = 0; i < SequenceLength; i++)
            {
                _numberOfUsedMebItems += currentState == ItemState.MEB ? 1 : 0;
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
