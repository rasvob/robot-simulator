using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.BatchSimulation
{
    public class ExperimentConfig
    {
        public List<ProductionState> ProductionStates { get; set; } = new List<ProductionState>();
        public List<ProductionState> ProductionStatesBackup { get; set; } = new List<ProductionState>();
        public int TimeLimit { get; set; }
        public int ClockTime { get; set; }
        public bool UseReorganization { get; set; }
        public int ReorganizationComplexityLevel { get; set; }
    }
}
