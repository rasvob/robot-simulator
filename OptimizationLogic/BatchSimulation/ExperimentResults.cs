using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.BatchSimulation
{
    public class ExperimentResults
    {
        public List<AggregatedResults> AggregatedResults { get; set; } = new List<AggregatedResults>();
        public List<SingleSimulationResult> SimulationResults { get; set; } = new List<SingleSimulationResult>();
    }
}
