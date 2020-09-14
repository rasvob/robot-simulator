﻿using OptimizationLogic;
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
        static readonly int SimulatorsConfigurationNum = 4;
        static readonly int AssignedScenariosNum = 4;
        static readonly int GeneratedScenariosNum = 20;

        static void Main(string[] args)
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            SimulationResult[] simulationResults = new SimulationResult[AssignedScenariosNum * SimulatorsConfigurationNum];
            for (int i = 8; i < 9; i++)
            {
                var matrixFilename = Path.Combine(startupPath, @"robot-simulator\robot-simulator\DailyPlans\ProcessingTimeMatrix.csv");
                var warehouseFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\WarehouseInitialState_daily{i}.csv");
                var historyFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\HistoricalProductionList_daily{i}.txt");
                var planFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\FutureProductionList_daily{i}.txt");

                var simulations = GetSimulationsDict(i.ToString(), matrixFilename, warehouseFilename, historyFilename, planFilename);
                RunSimulations(simulations, simulationResults, 0);
            }

            //SimulateAssignedScenarios();
            //SimulateGeneratedScenarios();
            //SimulateGeneratedDayScenarios();
            //SimulateGeneratedWeekScenarios();

            Console.WriteLine("Finished. Press any key to close.");
            Console.ReadKey();
        }

        static Dictionary<string, RealProductionSimulator> GetSimulationsDict(string key, string matrixFilename, string warehouseFilename, string historyFilename, string planFilename)
        {
            Dictionary<string, RealProductionSimulator> simulationsDict = new Dictionary<string, RealProductionSimulator>();

            /*simulationsDict[$"naive-skip_break-{key}"] = new RealProductionSimulator(
                    new NaiveController(new ProductionState(), matrixFilename, warehouseFilename, historyFilename, planFilename)
                    );

            simulationsDict[$"naive-reorganization-{key}"] = new RealProductionSimulator(
                new NaiveController(new ProductionState(), matrixFilename, warehouseFilename, historyFilename, planFilename),
                new GreedyWarehouseReorganizer(maxDepth: 10, selectBestCnt: 1)
                );*/

            simulationsDict[$"async-skip_break-{key}"] = new RealProductionSimulator(
                new NaiveAsyncControllerWithHalfCycleDelay(new ProductionState(), matrixFilename, warehouseFilename, historyFilename, planFilename)
                );

            /*simulationsDict[$"async-reorganization-{key}"] = new RealProductionSimulator(
                new NaiveAsyncControllerWithHalfCycleDelay(new ProductionState(), matrixFilename, warehouseFilename, historyFilename, planFilename),
                new GreedyWarehouseReorganizer(maxDepth: 10, selectBestCnt: 1)
                );*/
            return simulationsDict;
        }

        static void SimulateAssignedScenarios()
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            SimulationResult[] simulationResults = new SimulationResult[AssignedScenariosNum * SimulatorsConfigurationNum];
            for (int i = 1; i < AssignedScenariosNum; i++)
            {
                var matrixFilename = Path.Combine(startupPath, @"robot-simulator\OptimizationLogic\InputFiles\ProcessingTimeMatrix.csv");
                var warehouseFilename= Path.Combine(startupPath, $@"robot-simulator\OptimizationLogic\InputFiles\situation{i}\WarehouseInitialState.csv");
                var historyFilename = Path.Combine(startupPath, $@"robot-simulator\OptimizationLogic\InputFiles\situation{i}\HistoricalProductionList.txt");
                var planFilename = Path.Combine(startupPath, $@"robot-simulator\OptimizationLogic\InputFiles\situation{i}\FutureProductionList.txt");

                var simulations = GetSimulationsDict(i.ToString(), matrixFilename, warehouseFilename, historyFilename, planFilename);
                RunSimulations(simulations, simulationResults, i);
            }
            File.WriteAllLines(Path.Combine(startupPath, @"robot-simulator\OptimizersSimulationSummary\simulations_assigned_output.csv"), simulationResults.Select(x => x.GetCsvRecord(";")).ToList());
        }

        static void SimulateGeneratedScenarios() 
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            SimulationResult[] simulationResults = new SimulationResult[GeneratedScenariosNum * SimulatorsConfigurationNum];
            var result = Parallel.For(0, GeneratedScenariosNum, (i) =>
            {
                var matrixFilename = Path.Combine(startupPath, @"robot-simulator\robot-simulator\GeneratedInput\ProcessingTimeMatrix.csv");
                var warehouseFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\GeneratedInput\generated_situation{i}\WarehouseInitialState{i}.csv");
                var historyFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\GeneratedInput\generated_situation{i}\HistoricalProductionList{i}.txt");
                var planFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\GeneratedInput\generated_situation{i}\FutureProductionList{i}.txt");

                var simulations = GetSimulationsDict(i.ToString(), matrixFilename, warehouseFilename, historyFilename, planFilename);
                RunSimulations(simulations, simulationResults, i);
            });
            File.WriteAllLines(Path.Combine(startupPath, @"robot-simulator\OptimizersSimulationSummary\simulations_100_output.csv"), simulationResults.Select(x => x.GetCsvRecord(";")).ToList());
        }

        static void SimulateGeneratedDayScenarios()
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            SimulationResult[] simulationResults = new SimulationResult[GeneratedScenariosNum * SimulatorsConfigurationNum];
            var result = Parallel.For(0, GeneratedScenariosNum, (i) =>
            {
                var matrixFilename = Path.Combine(startupPath, @"robot-simulator\robot-simulator\DailyPlans\ProcessingTimeMatrix.csv");
                var warehouseFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\WarehouseInitialState_daily{i}.csv");
                var historyFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\HistoricalProductionList_daily{i}.txt");
                var planFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\FutureProductionList_daily{i}.txt");

                var simulations = GetSimulationsDict(i.ToString(), matrixFilename, warehouseFilename, historyFilename, planFilename);
                RunSimulations(simulations, simulationResults, i);
            });
            File.WriteAllLines(Path.Combine(startupPath, @"robot-simulator\OptimizersSimulationSummary\simulations_day_output.csv"), simulationResults.Select(x => x.GetCsvRecord(";")).ToList());
        }

        static void SimulateGeneratedWeekScenarios()
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            SimulationResult[] simulationResults = new SimulationResult[GeneratedScenariosNum * SimulatorsConfigurationNum];
            var result = Parallel.For(0, GeneratedScenariosNum, (i) =>
            {
                var matrixFilename = Path.Combine(startupPath, @"robot-simulator\robot-simulator\WeeklyPlans\ProcessingTimeMatrix.csv");
                var warehouseFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\WeeklyPlans\generated_situation_weekly{i}\WarehouseInitialState_weekly{i}.csv");
                var historyFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\WeeklyPlans\generated_situation_weekly{i}\HistoricalProductionList_weekly{i}.txt");
                var planFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\WeeklyPlans\generated_situation_weekly{i}\FutureProductionList_weekly{i}.txt");

                var simulations = GetSimulationsDict(i.ToString(), matrixFilename, warehouseFilename, historyFilename, planFilename);
                RunSimulations(simulations, simulationResults, i);
            });
            File.WriteAllLines(Path.Combine(startupPath, @"robot-simulator\OptimizersSimulationSummary\simulations_week_output.csv"), simulationResults.Select(x => x.GetCsvRecord(";")).ToList());
        }

        static void RunSimulations(Dictionary<string, RealProductionSimulator> simulationsDict, SimulationResult[] simulationResults, int index)
        {
            int simulationCounter = 0;
            foreach (var simulation in simulationsDict)
            {
                var simulationName = simulation.Key;
                var simulator = simulation.Value;

                SimulationResult res = SimulateRun(simulator);
                res.Name = simulationName;
                simulationResults[index*SimulatorsConfigurationNum + simulationCounter++] = res;
                Console.WriteLine(res);                
            }
        }

        static int GetPlanedTime(int plannedProductionLength)
        {
            switch (plannedProductionLength)
            {
                // TODO: modify to match correct numbers 
                case 100: return plannedProductionLength * 55;
                case 1440: return 86400;
                case 7200: return 5 * 86400;
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
               
                if (productionSimulator.Controller.Delay > 0 && result.FirstDelayProductionPlanCount == 0)
                {
                    result.FirstDelayProductionPlanCount = productionSimulator.Controller.ProductionState.FutureProductionPlan.Count;
                }

                if (productionSimulator.Controller.RealTime <= plannedRealProcessingTime)
                {
                    result.PlannedTimeProductionPlanCount = productionSimulator.Controller.ProductionState.FutureProductionPlan.Count;
                }
                Console.WriteLine($"{productionSimulator.Controller.RealTime} {productionSimulator.Controller.Delay} {productionSimulator.Controller.ProductionState.FutureProductionPlan.Count}");
            }
            result.DelayTime = productionSimulator.Controller.Delay;

            return result;
        }
    }
}
