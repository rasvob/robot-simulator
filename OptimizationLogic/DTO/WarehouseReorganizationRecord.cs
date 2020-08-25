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

        public List<Tuple<PositionCodes, PositionCodes>> GetSwapsFromRoot()
        {
            List<Tuple<PositionCodes, PositionCodes>> swaps = new List<Tuple<PositionCodes, PositionCodes>>();
            WarehouseReorganizationRecord currentRecord = this;
            while (currentRecord.PreviousRecord != null)
            {
                swaps.Add(currentRecord.Swap);
                currentRecord = currentRecord.PreviousRecord;
            }
            swaps.Reverse();
            return swaps;
        }

        public string PrintSwapsFromRoot()
        {
            var swaps = GetSwapsFromRoot();
            if (swaps.Count == 0)
            {
                return "No swap";
            }
            StringBuilder sb = new StringBuilder();
            
            foreach (var item in swaps)
            {
                sb.Append(String.Format("{0} => {1}, ", item.Item1, item.Item2));
            }
            return sb.ToString();
        }
    }
}
