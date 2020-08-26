using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic
{
    public class WarehouseSwapStepModel : BaseStepModel
    {
        public PositionCodes SwapFromCell { get; set; }
        public PositionCodes SwapToCell { get; set; }
        public double MoveTime { get; set; }
        public double SwapTime { get; set; }
        public double ExtraTime { get; set; }
        public ItemState SwapElement { get; set; }
        public override string ToString()
        {
            if (ExtraTime > 0)
            {
                return $"Moving to {SwapFromCell} in time {MoveTime:f}, swaping item {SwapElement} ({SwapFromCell} -> {SwapToCell}) in time {SwapTime:f} and returning to stacker in time {ExtraTime:f}";
            }
            else
            {
                return $"Moving to {SwapFromCell} in time {MoveTime:f}, swaping item {SwapElement} ({SwapFromCell} -> {SwapToCell}) in time {SwapTime:f}";
            }

        }
    }
}
