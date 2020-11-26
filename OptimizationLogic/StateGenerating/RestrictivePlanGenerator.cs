using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.StateGenerating
{
    public class RestrictivePlanGenerator
    {
        public Random RandomGenerator { get; set ; }
        public ItemState DominantItem { get; set; }
        public ItemState NonDominantItem { get; set; }
        public int NonDominantItemInARowLimit { get; set; }

        public double DominantToNonDominantTransitionProbability { get; set; } = 0.5;

        public RestrictivePlanGenerator(ItemState dominantItem, ItemState nonDominantItem, int nonDominantItemInARowLimit, Random random = null)
        {
            DominantItem = dominantItem;
            NonDominantItem = nonDominantItem;
            NonDominantItemInARowLimit = nonDominantItemInARowLimit;
            RandomGenerator = random ?? new Random();
        }

        public List<ItemState> GenerateSequence(int sequenceLength)
        {
            return GenerateSequenceNew(sequenceLength);
        }

        public List<ItemState> GenerateSequenceAlt(int sequenceLength)
        {
            var res = new List<ItemState>(sequenceLength);
            int nonDominantCounter = 0;
            for (int i = 0; i < sequenceLength; i++)
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

        public List<ItemState> GenerateSequenceNew(int sequenceLength)
        {
            var res = new List<ItemState>();

            if (NonDominantItemInARowLimit == 0)
            {
                for (int i = 0; i < sequenceLength; i++)
                {
                    ItemState currentState = RandomGenerator.NextDouble() < DominantToNonDominantTransitionProbability ? NonDominantItem : DominantItem;
                    res.Add(currentState);
                }
            }

            while (res.Count < sequenceLength)
            {
                ItemState currentState = RandomGenerator.NextDouble() < DominantToNonDominantTransitionProbability ? NonDominantItem : DominantItem;
                res.Add(currentState);

                if (currentState == NonDominantItem)
                {
                    for (int i = 0; i < NonDominantItemInARowLimit; i++)
                    {
                        res.Add(DominantItem);
                    }
                }
            }

            return res.Take(sequenceLength).ToList();
        }
    }
}
