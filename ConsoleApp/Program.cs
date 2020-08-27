using OptimizationLogic;
using OptimizationLogic.DTO;
using System;
using System.IO;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            NaiveController naiveController = new NaiveController(new ProductionState(),
                   Path.Combine(startupPath, @"OptimizationLogic\InputFiles\ProcessingTimeMatrix.csv"),
                   Path.Combine(startupPath, @"OptimizationLogic\InputFiles\situation1\WarehouseInitialState.csv"),
                   Path.Combine(startupPath, @"OptimizationLogic\InputFiles\situation1\HistoricalProductionList.txt"),
                   Path.Combine(startupPath, @"OptimizationLogic\InputFiles\situation1\FutureProductionList.txt"));
            GreedyWarehouseReorganizer warehouseReorganizer = new GreedyWarehouseReorganizer(maxDepth: 5, selectBestCnt: 1);
            RealProductionSimulator productionSimulator = new RealProductionSimulator(naiveController, warehouseReorganizer);
            productionSimulator.Run();

        }
    }
}
