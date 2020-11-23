using OptimizationLogic;
using OptimizationLogic.AsyncControllers;
using OptimizationLogic.DTO;
using OptimizersSimulationSummary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace OptimizersSimulationsSummary
{
    class Program
    {
        static readonly int SimulatorsConfigurationNum = 4;
        static readonly int AssignedScenariosNum = 4;
        static readonly int GeneratedScenariosNum = 1200;
        static Random rnd = new Random(13);
        //static List<int> RandomSequence = Enumerable.Repeat(0, GeneratedScenariosNum).Select(t => rnd.Next(0, 1199)).ToList();
        static List<int> RandomSequence = Enumerable.Range(0, GeneratedScenariosNum).ToList();

        static void Main(string[] args)
        {
            SimulationsReport simulationsReport = new SimulationsReport();
            simulationsReport.Run();
            //SimulateOne();            
            //SimulateAssignedScenarios();
            //SimulateGeneratedScenarios();
            //SimulateGeneratedDayScenarios();
            //SimulateGeneratedWeekScenarios();

            Console.WriteLine("Finished. Press any key to close.");
            Trace.WriteLine("Finished. Press any key to close.");
            Console.ReadKey();
        }

        static void SimulateOne() 
        {
            //async - skip_break - 98
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            int i = 8;
            List<SimulationResult> simulationResults = new List<SimulationResult>();
            var warehouseFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\WarehouseInitialState_daily{i}.csv");
            var historyFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\HistoricalProductionList_daily{i}.txt");
            var planFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\FutureProductionList_daily{i}.txt");

            var simulations = GetSimulationsDict(i.ToString(), warehouseFilename, historyFilename, planFilename);
            RunSimulations(simulations, simulationResults);

        }

        static Dictionary<string, RealProductionSimulator> GetSimulationsDict(string key, string warehouseFilename, string historyFilename, string planFilename)
        {
            Dictionary<string, RealProductionSimulator> simulationsDict = new Dictionary<string, RealProductionSimulator>();

            simulationsDict[$"naive-skip_break-{key}"] = new RealProductionSimulator(
                    new NaiveController(new ProductionState(12, 4), warehouseFilename, historyFilename, planFilename)
                    );

            simulationsDict[$"naive-reorganization-{key}"] = new RealProductionSimulator(
                new NaiveController(new ProductionState(12, 4), warehouseFilename, historyFilename, planFilename),
                new GreedyWarehouseReorganizer(maxDepth: 10, selectBestCnt: 1)
                );

            simulationsDict[$"async-skip_break-{key}"] = new RealProductionSimulator(
                new NaiveAsyncControllerWithHalfCycleDelay(new ProductionState(12, 4), warehouseFilename, historyFilename, planFilename)
                );

            simulationsDict[$"async-reorganization-{key}"] = new RealProductionSimulator(
                new NaiveAsyncControllerWithHalfCycleDelay(new ProductionState(12, 4), warehouseFilename, historyFilename, planFilename),
                new GreedyWarehouseReorganizer(maxDepth: 10, selectBestCnt: 1)
                );
            return simulationsDict;
        }

        static void SimulateAssignedScenarios()
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            List<SimulationResult> simulationResults = new List<SimulationResult>();
            for (int i = 1; i < AssignedScenariosNum; i++)
            {
                var warehouseFilename= Path.Combine(startupPath, $@"robot-simulator\OptimizationLogic\InputFiles\situation{i}\WarehouseInitialState.csv");
                var historyFilename = Path.Combine(startupPath, $@"robot-simulator\OptimizationLogic\InputFiles\situation{i}\HistoricalProductionList.txt");
                var planFilename = Path.Combine(startupPath, $@"robot-simulator\OptimizationLogic\InputFiles\situation{i}\FutureProductionList.txt");

                var simulations = GetSimulationsDict(i.ToString(), warehouseFilename, historyFilename, planFilename);
                RunSimulations(simulations, simulationResults);
            }
            File.WriteAllLines(Path.Combine(startupPath, @"robot-simulator\OptimizersSimulationSummary\simulations_assigned_output.csv"), simulationResults.Select(x => x.GetCsvRecord(";")).ToList());
        }

        //static void SimulateGeneratedScenarios() 
        //{
        //    string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
        //    SimulationResult[] simulationResults = new SimulationResult[GeneratedScenariosNum * SimulatorsConfigurationNum];
        //    var result = Parallel.For(0, GeneratedScenariosNum, (i) =>
        //    {
        //        var matrixFilename = Path.Combine(startupPath, @"robot-simulator\robot-simulator\GeneratedInput\ProcessingTimeMatrix.csv");
        //        var warehouseFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\GeneratedInput\generated_situation{i}\WarehouseInitialState{i}.csv");
        //        var historyFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\GeneratedInput\generated_situation{i}\HistoricalProductionList{i}.txt");
        //        var planFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\GeneratedInput\generated_situation{i}\FutureProductionList{i}.txt");

        //        var simulations = GetSimulationsDict(i.ToString(), matrixFilename, warehouseFilename, historyFilename, planFilename);
        //        RunSimulations(simulations, simulationResults, i);
        //    });
        //    File.WriteAllLines(Path.Combine(startupPath, @"robot-simulator\OptimizersSimulationSummary\simulations_100_output.csv"), simulationResults.Select(x => x.GetCsvRecord(";")).ToList());
        //}

        static void SimulateGeneratedDayScenarios()
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            int completed = 0;
            var sw = new Stopwatch();
            sw.Start();
            var result = Parallel.For(0, GeneratedScenariosNum, (j) =>
            {
                int i = RandomSequence[j];
                List<SimulationResult> simulationResults = new List<SimulationResult>();
                var warehouseFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\WarehouseInitialState_daily{i}.csv");
                var historyFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\HistoricalProductionList_daily{i}.txt");
                var planFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\FutureProductionList_daily{i}.txt");

                var simulations = GetSimulationsDict(i.ToString(), warehouseFilename, historyFilename, planFilename);
                RunSimulations(simulations, simulationResults);
                Interlocked.Increment(ref completed);
                Trace.WriteLine($"Completed: {completed}");
                Console.WriteLine($"Completed: {completed}");
                File.WriteAllLines(Path.Combine(startupPath, $@"robot-simulator\OptimizersSimulationSummary\simulations_day_output_{i}.csv"), simulationResults.Select(x => x.GetCsvRecord(";")).ToList());
            });
            sw.Stop();
            Trace.WriteLine($"Time for one run: {sw.ElapsedMilliseconds}");
        }

        static void SimulateGeneratedWeekScenarios()
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            int completed = 0;
            var result = Parallel.For(0, GeneratedScenariosNum, (j) =>
            {
                int i = RandomSequence[j];
                List<SimulationResult> simulationResults = new List<SimulationResult>();
                var warehouseFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\WeeklyPlans\generated_situation_weekly{i}\WarehouseInitialState_weekly{i}.csv");
                var historyFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\WeeklyPlans\generated_situation_weekly{i}\HistoricalProductionList_weekly{i}.txt");
                var planFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\WeeklyPlans\generated_situation_weekly{i}\FutureProductionList_weekly{i}.txt");

                var simulations = GetSimulationsDict(i.ToString(), warehouseFilename, historyFilename, planFilename);
                RunSimulations(simulations, simulationResults);
                Interlocked.Increment(ref completed);
                Trace.WriteLine($"Completed: {completed}");
                Console.WriteLine($"Completed: {completed}");
                File.WriteAllLines(Path.Combine(startupPath, $@"robot-simulator\OptimizersSimulationSummary\simulations_week_output_{i}.csv"), simulationResults.Select(x => x.GetCsvRecord(";")).ToList());
            });
        }

        static void RunSimulations(Dictionary<string, RealProductionSimulator> simulationsDict, List<SimulationResult> simulationResults)
        {
            int simulationCounter = 0;
            
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
                //Console.WriteLine(productionSimulator.Controller.StepLog.Last());
                //Console.WriteLine($"{productionSimulator.Controller.RealTime}; {productionSimulator.Controller.Delay}; {productionSimulator.Controller.ProductionState.FutureProductionPlan.Count}");                
                if (productionSimulator.Controller.Delay > 0 && result.FirstDelayProductionPlanCount == 0)
                {
                    result.FirstDelayProductionPlanCount = productionSimulator.Controller.ProductionState.FutureProductionPlan.Count;
                }

                if (productionSimulator.Controller.RealTime <= plannedRealProcessingTime)
                {
                    result.PlannedTimeProductionPlanCount = productionSimulator.Controller.ProductionState.FutureProductionPlan.Count;
                }
            }
            // problem with last step of async controller
            bool missingGet = productionSimulator.Controller.ProductionState.ProductionHistory.Count > 64; // TODO: add constant
            result.DelayTime = productionSimulator.Controller.Delay;
            return result;
        }
    }
}
