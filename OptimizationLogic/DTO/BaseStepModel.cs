using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.DTO
{
    public class BaseStepModel
    {
        public string Message { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
