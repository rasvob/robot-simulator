using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;

namespace OptimizationLogic.StateGenerating
{
    public interface IPlanGenerator
    {
        public Random RandomGenerator { get; set; }
        public int SequenceLength { get; set; }
        public List<ItemState> GenerateSequence();
    }
}
