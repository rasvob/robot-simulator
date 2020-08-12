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
    }
}
