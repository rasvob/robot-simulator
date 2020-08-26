using OptimizationLogic;
using OptimizationLogic.AsyncControllers;
using OptimizationLogic.DTO;
using OptimizersSimulationSummary;
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
            //SimulateAssignedScenarios();
            SimulateGeneratedScenarios();

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
                    maxDepth: 3, selectBestCnt:3);
            }
            RunSimulations(simulationsDict);
        }

        static void SimulateGeneratedScenarios() 
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            Dictionary<string, BaseController> simulationsDict = new Dictionary<string, BaseController>();

            for (int i = 1; i <= 1200; i++)
            {
                var matrixFilename = Path.Combine(startupPath, @"robot-simulator\robot-simulator\GeneratedInput\ProcessingTimeMatrix.csv");
                var warehouseFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\GeneratedInput\generated_situation{i}\WarehouseInitialState{i}.csv");
                var historyFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\GeneratedInput\generated_situation{i}\HistoricalProductionList{i}.txt");
                var planFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\GeneratedInput\generated_situation{i}\FutureProductionList{i}.txt");

                simulationsDict[$"naive-{i}"] = new NaiveController(new ProductionState(), matrixFilename, warehouseFilename, historyFilename, planFilename);
                simulationsDict[$"greedy-{i}"] = new GreedyWarehouseOptimizationController(new ProductionState(), matrixFilename, warehouseFilename, historyFilename, planFilename,
                    maxDepth: 3, selectBestCnt: 3);
            }
            RunSimulations(simulationsDict, Path.Combine(startupPath, @"robot-simulator\OptimizersSimulationSummary\simulations_output.csv"));
        }

        static void RunSimulations(Dictionary<string, BaseController> simulationsDict, string outputFilename=null)
        {
            List<SimulationResult> simulationResults = new List<SimulationResult>();
            foreach (var simulation in simulationsDict)
            {
                var simulationName = simulation.Key;
                var controller = simulation.Value;

                while (controller.ProductionState.ProductionStateIsOk && controller.ProductionState.FutureProductionPlan.Count > 0)
                {
                    controller.NextStep();
                }
                var res = new SimulationResult() { Name = simulationName, MissingSteps = controller.ProductionState.FutureProductionPlan.Count };
                simulationResults.Add(res);
                Console.WriteLine(res);
            }
            if (outputFilename != null)
            {
                File.WriteAllLines(outputFilename, simulationResults.Select(x => x.GetCsvRecord(";")).ToList());
            }
        }
    }
}
