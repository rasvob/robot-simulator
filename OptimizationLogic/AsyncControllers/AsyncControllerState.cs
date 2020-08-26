using System;
using System.Threading.Tasks;
using OptimizationLogic.DTO;
using OptimizationLogic.Extensions;

namespace OptimizationLogic.AsyncControllers
{

    public enum AsyncControllerState
    {
        Start,
        SwapChain,
        Put,
        Get,
        End
    }
}
