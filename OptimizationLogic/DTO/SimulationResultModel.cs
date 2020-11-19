using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.DTO
{
    public class SimulationResultModel
    {
        public int SimulationNumber { get; set; }
        public double Delay { get; set; }
        public int NumberOfNonProducedCars { get; set; }
    }
}
