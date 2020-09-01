using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.DTO
{
    public class ProgressEventArgs: EventArgs
    {
        public ProgressState State { get; set; }
        public int CurrentValue { get; set; }
    }

    public enum ProgressState: int
    {
        Start, End, Update
    }
}
