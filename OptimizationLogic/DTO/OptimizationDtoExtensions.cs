using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.DTO
{
    public static class OptimizationDtoExtensions
    {
        public static string ToGuiString(this PositionCodes code) => code switch
        {
            PositionCodes.Stacker => code.ToString(),
            PositionCodes.Service => code.ToString(),
            _ => code.ToString().Remove(0, 1)
        };
    }
}
