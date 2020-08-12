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

            ProductionState productionState = new ProductionState();
            Console.WriteLine("Processing time matrix");
            productionState.LoadTimeMatrix(Path.Combine(startupPath, @"OptimizationLogic\InputFiles\ProcessingTimeMatrix.csv"));
            Console.WriteLine(productionState.TimeMatrix[0, 0]);
            Console.WriteLine(productionState.TimeMatrix[0, 1]);
            Console.WriteLine(productionState.TimeMatrix[1, 0]);
            Console.WriteLine(productionState.TimeMatrix[0, 10]);

            Console.WriteLine("Warehouse initial state matrix");
            productionState.LoadWarehouseState(Path.Combine(startupPath, @"OptimizationLogic\InputFiles\situation1\WarehouseInitialState.csv"));
            Console.WriteLine(productionState.WarehouseState[0, 0]);
            Console.WriteLine(productionState.WarehouseState[0, 1]);
            Console.WriteLine(productionState.WarehouseState[1, 0]);
            Console.WriteLine(productionState.WarehouseState[1, 1]);
            Console.WriteLine(productionState.WarehouseState[2, 0]);
            Console.WriteLine(productionState.WarehouseState[2, 0]);

            Console.WriteLine("Production history queue");
            productionState.LoadProductionHistory(Path.Combine(startupPath, @"OptimizationLogic\InputFiles\situation1\HistoricalProductionList.txt"));
            Console.WriteLine(productionState.ProductionHistory.Count);
            Console.WriteLine(productionState.ProductionHistory.Dequeue());
            Console.WriteLine(productionState.ProductionHistory.Dequeue());
            Console.WriteLine(productionState.ProductionHistory.Dequeue());

            Console.WriteLine("Future production plan queue");
            productionState.LoadFutureProductionPlan(Path.Combine(startupPath, @"OptimizationLogic\InputFiles\situation1\FutureProductionList.txt"));
            Console.WriteLine(productionState.FutureProductionPlan.Count);
            Console.WriteLine(productionState.FutureProductionPlan.Dequeue());
            Console.WriteLine(productionState.FutureProductionPlan.Dequeue());
            Console.WriteLine(productionState.FutureProductionPlan.Dequeue());

        }
    }
}
