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
            //SimulateGeneratedScenarios();
            SimulateGeneratedDayScenarios();
            //SimulateGeneratedWeekScenarios();

            Console.WriteLine("Finished. Press any key to close.");
            Console.ReadKey();
        }

        static Dictionary<string, RealProductionSimulator> GetSimulationsDict(string key, string matrixFilename, string warehouseFilename, string historyFilename, string planFilename)
        {
            Dictionary<string, RealProductionSimulator> simulationsDict = new Dictionary<string, RealProductionSimulator>();

            simulationsDict[$"naive-break_skip-{key}"] = new RealProductionSimulator(
                    new NaiveController(new ProductionState(), matrixFilename, warehouseFilename, historyFilename, planFilename)
                    );

            /*simulationsDict[$"naive-reorganization-{key}"] = new RealProductionSimulator(
                new NaiveController(new ProductionState(), matrixFilename, warehouseFilename, historyFilename, planFilename),
                new GreedyWarehouseReorganizer(maxDepth: 10, selectBestCnt: 1)
                );*/
            return simulationsDict;
        }

        static void SimulateAssignedScenarios()
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            List<SimulationResult> simulationResults = new List<SimulationResult>();
            for (int i = 1; i < 4; i++)
            {
                var matrixFilename = Path.Combine(startupPath, @"robot-simulator\OptimizationLogic\InputFiles\ProcessingTimeMatrix.csv");
                var warehouseFilename= Path.Combine(startupPath, $@"robot-simulator\OptimizationLogic\InputFiles\situation{i}\WarehouseInitialState.csv");
                var historyFilename = Path.Combine(startupPath, $@"robot-simulator\OptimizationLogic\InputFiles\situation{i}\HistoricalProductionList.txt");
                var planFilename = Path.Combine(startupPath, $@"robot-simulator\OptimizationLogic\InputFiles\situation{i}\FutureProductionList.txt");

                var simulations = GetSimulationsDict(i.ToString(), matrixFilename, warehouseFilename, historyFilename, planFilename);
                RunSimulations(simulations, simulationResults);
            }            
            File.WriteAllLines(Path.Combine(startupPath, @"robot-simulator\OptimizersSimulationSummary\simulations_assigned_output.csv"), simulationResults.Select(x => x.GetCsvRecord(";")).ToList());
        }

        static void SimulateGeneratedScenarios() 
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            List<SimulationResult> simulationResults = new List<SimulationResult>();
            for (int i = 1; i <= 1200; i++)
            {
                var matrixFilename = Path.Combine(startupPath, @"robot-simulator\robot-simulator\GeneratedInput\ProcessingTimeMatrix.csv");
                var warehouseFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\GeneratedInput\generated_situation{i}\WarehouseInitialState{i}.csv");
                var historyFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\GeneratedInput\generated_situation{i}\HistoricalProductionList{i}.txt");
                var planFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\GeneratedInput\generated_situation{i}\FutureProductionList{i}.txt");

                var simulations = GetSimulationsDict(i.ToString(), matrixFilename, warehouseFilename, historyFilename, planFilename);
                RunSimulations(simulations, simulationResults);
            }
            File.WriteAllLines(Path.Combine(startupPath, @"robot-simulator\OptimizersSimulationSummary\simulations_100_output.csv"), simulationResults.Select(x => x.GetCsvRecord(";")).ToList());
        }

        static void SimulateGeneratedDayScenarios()
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            List<SimulationResult> simulationResults = new List<SimulationResult>();
            for (int i = 1; i <= 1200; i++)
            {
                var matrixFilename = Path.Combine(startupPath, @"robot-simulator\robot-simulator\DailyPlans\ProcessingTimeMatrix.csv");
                var warehouseFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\WarehouseInitialState_daily{i}.csv");
                var historyFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\HistoricalProductionList_daily{i}.txt");
                var planFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\FutureProductionList_daily{i}.txt");

                var simulations = GetSimulationsDict(i.ToString(), matrixFilename, warehouseFilename, historyFilename, planFilename);
                RunSimulations(simulations, simulationResults);
            }
            File.WriteAllLines(Path.Combine(startupPath, @"robot-simulator\OptimizersSimulationSummary\simulations_day_output.csv"), simulationResults.Select(x => x.GetCsvRecord(";")).ToList());
        }

        static void SimulateGeneratedWeekScenarios()
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            List<SimulationResult> simulationResults = new List<SimulationResult>();
            for (int i = 1; i <= 1200; i++)
            {
                var matrixFilename = Path.Combine(startupPath, @"robot-simulator\robot-simulator\WeeklyPlans\ProcessingTimeMatrix.csv");
                var warehouseFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\WeeklyPlans\generated_situation_weekly{i}\WarehouseInitialState_weekly{i}.csv");
                var historyFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\WeeklyPlans\generated_situation_weekly{i}\HistoricalProductionList_weekly{i}.txt");
                var planFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\WeeklyPlans\generated_situation_weekly{i}\FutureProductionList_weekly{i}.txt");

                var simulations = GetSimulationsDict(i.ToString(), matrixFilename, warehouseFilename, historyFilename, planFilename);
                RunSimulations(simulations, simulationResults);
            }
            File.WriteAllLines(Path.Combine(startupPath, @"robot-simulator\OptimizersSimulationSummary\simulations_week_output.csv"), simulationResults.Select(x => x.GetCsvRecord(";")).ToList());
        }

        static void RunSimulations(Dictionary<string, RealProductionSimulator> simulationsDict, List<SimulationResult> simulationResults)
        {
            foreach (var simulation in simulationsDict)
            {
                var simulationName = simulation.Key;
                var simulator = simulation.Value;

                SimulationResult res = SimulateRun(simulator);
                res.Name = simulationName;
                simulationResults.Add(res);
                Console.WriteLine(res);                
            }
        }

        static int GetPlanedTime(int plannedProductionLength)
        {
            switch (plannedProductionLength)
            {
                case 100: return plannedProductionLength * 55;
                case 1416: return plannedProductionLength * 55 + 8100;
                case 8968: return plannedProductionLength * 55 + 19 * 2700;
                default: throw new ArgumentException("Wrong production length");
            }
        }

        static SimulationResult SimulateRun(RealProductionSimulator productionSimulator)
        {
            var result = new SimulationResult();
            productionSimulator.Controller.RealTime = -300;
            var plannedRealProcessingTime = GetPlanedTime(productionSimulator.Controller.ProductionState.FutureProductionPlan.Count);
            
            while (productionSimulator.Controller.ProductionState.FutureProductionPlan.Count > 0)
            {
                productionSimulator.NextStep();
                
                if (productionSimulator.Controller.ProductionState.ProductionStateIsOk == false && result.FirstDelayProductionPlanCount == 0)
                {
                    result.FirstDelayProductionPlanCount = productionSimulator.Controller.ProductionState.FutureProductionPlan.Count;
                }

                if (productionSimulator.Controller.RealTime >= plannedRealProcessingTime && result.PlannedTimeProductionPlanCount == 0)
                {
                    result.PlannedTimeProductionPlanCount = productionSimulator.Controller.ProductionState.FutureProductionPlan.Count;
                }
            }
            result.DelayTime = productionSimulator.Controller.Delay;

            return result;
        }
    }
}
