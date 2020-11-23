using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizersSimulationSummary
{
    class SimulationsReportRecord
    {
        public string ConfigurationName { get; set; }
        public double PercentageOfCompletePlans { get; set; }
        public double AverageMissingCars { get; set; }
        public double AverageDelay { get; set; }
        public int MissingCars90Percentile { get; set; }

        public override string ToString()
        {
            string separator = "\t";
            return $"{ConfigurationName}{separator}{PercentageOfCompletePlans}{separator}{AverageMissingCars}{separator}{AverageDelay}{separator}{MissingCars90Percentile}";
        }
    }
}
