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
        public int FirstDelayProductionPlanCount { get; set; }
        public double DelayTime { get; set; }
        public int PlannedTimeProductionPlanCount{ get; set; }

        public string GetCsvRecord(string separator)
        {
            return $"{Name}{separator}{FirstDelayProductionPlanCount}{separator}{PlannedTimeProductionPlanCount}{separator}{DelayTime}";
        }
        public override string ToString()
        {
            return GetCsvRecord(";");
        }
    }
}
