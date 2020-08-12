using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.DTO
{
    public class StepModel
    {
        public PositionCodes InsertToCell { get; set; }
        public PositionCodes WithdrawFromCell { get; set; }
        public double InsertTime { get; set; }
        public double WithdrawTime { get; set; }
        public double MoveToDifferentCellTime { get; set; }
        public ItemState InsertType { get; set; }
        public ItemState WithdrawType { get; set; }

        public override string ToString()
        {
            return String.Format("Inserting to {0} (time {1:f}, type {5}), move to {2}, time({3:f}), and going to stacker (time {4:f}, withdrawing type {6}), total time {7}", InsertToCell, InsertTime, WithdrawFromCell, MoveToDifferentCellTime, WithdrawTime, InsertType, WithdrawType, InsertTime+MoveToDifferentCellTime+WithdrawTime);
        }
    }
}
