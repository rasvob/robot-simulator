using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.BatchSimulation
{
    public class SingleSimulationResult
    {
        public int SimulationNumber { get; set; }
        public string ConfigurationName { get; set; }
        public double Delay { get; set; }
        public int MissingCarsCount { get; set; }
    }
}
