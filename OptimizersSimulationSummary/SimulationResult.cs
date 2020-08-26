using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizersSimulationSummary
{
    public class SimulationResult
    {
        public String Name { get; set; }
        public int MissingSteps { get; set; }

        public string GetCsvRecord(string separator)
        {
            return $"{Name}{separator}{MissingSteps}";
        }
        public override string ToString()
        {
            return $"Simulation name {Name}\tmissing steps {MissingSteps}";
        }
    }
}
