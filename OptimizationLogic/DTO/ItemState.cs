using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.DTO
{
    public enum ItemState : int
    {
        Forbidden = -1,
        Empty = 0,
        MEB = 1,
        MQB = 2
    }
}
