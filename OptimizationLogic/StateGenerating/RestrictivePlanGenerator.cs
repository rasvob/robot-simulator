using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;

namespace OptimizationLogic.StateGenerating
{
    public class RestrictivePlanGenerator : IPlanGenerator
    {
        public Random RandomGenerator { get; set ; }
        public int SequenceLength { get; set; }
        public ItemState DominantItem { get; set; }
        public ItemState NonDominantItem { get; set; }
        public int NonDominantItemInARowLimit { get; set; }

        public double DominantToNonDominantTransitionProbability { get; set; } = 0.5;

        public RestrictivePlanGenerator(int sequenceLength, ItemState dominantItem, ItemState nonDominantItem, int nonDominantItemInARowLimit, Random random = null)
        {
            SequenceLength = sequenceLength;
            DominantItem = dominantItem;
            NonDominantItem = nonDominantItem;
            NonDominantItemInARowLimit = nonDominantItemInARowLimit;
            RandomGenerator = random ?? new Random();
        }

        public List<ItemState> GenerateSequence()
        {
            var res = new List<ItemState>(SequenceLength);
            int nonDominantCounter = 0;
            for (int i = 0; i < SequenceLength; i++)
            {
                ItemState currentState = RandomGenerator.NextDouble() < DominantToNonDominantTransitionProbability && nonDominantCounter < NonDominantItemInARowLimit ? NonDominantItem : DominantItem;

                if (currentState == DominantItem)
                {
                    nonDominantCounter = 0;
                }
                else
                {
                    nonDominantCounter++;
                }

                res.Add(currentState);
            }

            return res;
        }
    }
}
