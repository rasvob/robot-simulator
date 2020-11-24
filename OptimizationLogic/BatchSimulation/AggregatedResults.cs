using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.BatchSimulation
{
    public class AggregatedResults
    {
        public string ConfigurationName { get; set; }
        public double PercentageOfCompletePlans { get; set; }
        public double AverageMissingCars { get; set; }
        public double MedianMissingCars { get; set; }
        public double AverageDelay { get; set; }
        public double MedianDelay { get; set; }
        public double MissingCars90Percentile { get; set; }

        public override string ToString()
        {
            string separator = "\t";
            return $"{ConfigurationName}{separator}{PercentageOfCompletePlans}{separator}{AverageMissingCars}{separator}{AverageDelay}{separator}{MissingCars90Percentile}";
        }
    }
}
