using OptimizationLogic;
using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizersSimulationsSummary
{
    class Program
    {
        static void Main(string[] args)
        {
            SimulateAssignedScenarios();

            Console.WriteLine("Finished. Press any key to close.");
            Console.ReadKey();
        }

        static void SimulateAssignedScenarios()
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            Dictionary<string, BaseController> simulationsDict = new Dictionary<string, BaseController>();

            for (int i = 1; i < 4; i++)
            {
                var matrixFilename = Path.Combine(startupPath, @"robot-simulator\OptimizationLogic\InputFiles\ProcessingTimeMatrix.csv");
                var warehouseFilename= Path.Combine(startupPath, $@"robot-simulator\OptimizationLogic\InputFiles\situation{i}\WarehouseInitialState.csv");
                var historyFilename = Path.Combine(startupPath, $@"robot-simulator\OptimizationLogic\InputFiles\situation{i}\HistoricalProductionList.txt");
                var planFilename = Path.Combine(startupPath, $@"robot-simulator\OptimizationLogic\InputFiles\situation{i}\FutureProductionList.txt");

                simulationsDict[$"naive-{i}"] = new NaiveController(new ProductionState(), matrixFilename, warehouseFilename, historyFilename, planFilename);
                simulationsDict[$"greedy-{i}"] = new GreedyWarehouseOptimizationController(new ProductionState(), matrixFilename, warehouseFilename, historyFilename, planFilename,
                    maxDepth: 5, selectBestCnt:10);
            }
            RunSimulations(simulationsDict);
        }

        static void SimulateGeneratedScenarios() 
        {
        }

        static void RunSimulations(Dictionary<string, BaseController> simulationsDict)
        {
            foreach (var simulation in simulationsDict)
            {
                var simulationName = simulation.Key;
                var controller = simulation.Value;

                while (controller.ProductionState.ProductionStateIsOk && controller.ProductionState.FutureProductionPlan.Count > 0)
                {
                    controller.NextStep();
                }
                Console.WriteLine($"simulation {simulationName}\tmissing steps {controller.ProductionState.FutureProductionPlan.Count}");
            }
        }
    }
}
