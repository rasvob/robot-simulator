using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.DTO
{
    class WarehouseReorganizationRecord
    {
        public Tuple<PositionCodes, PositionCodes> Swap { get; set; }
        public WarehouseReorganizationRecord PreviousRecord { get; set; }
        public double RemainingTime { get; set; }
        public int MissingSimulationSteps { get; set; }
        public ProductionState ProductionState { get; set; }
    }
}
