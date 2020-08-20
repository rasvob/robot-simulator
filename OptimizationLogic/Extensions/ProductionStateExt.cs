using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.Extensions
{
    public static class ProductionStateExt
    {
        public static T Copy<T>(this T obj) where T : class, ICloneable
        {
            return obj.Clone() as T;
        }
    }
}
