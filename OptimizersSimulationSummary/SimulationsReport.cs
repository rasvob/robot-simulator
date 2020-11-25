using OptimizationLogic;
using OptimizationLogic.AsyncControllers;
using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OptimizersSimulationSummary
{
    public class SimulationsReport
    {
        private List<SimulationResult> simulationResults = new List<SimulationResult>();

        private readonly int GeneratedScenariosNum = 3;
        private List<int> RandomSequence;

        public SimulationsReport()
        {
            Random rnd = new Random(13);
            RandomSequence = Enumerable.Repeat(0, GeneratedScenariosNum).Select(t => rnd.Next(0, 1199)).ToList();
        }

        public void Run()
        {
            SimulateGeneratedDayScenarios();

            // Process results and aggragate results to table
            Console.WriteLine("Number of records for aggregation {0}", simulationResults.Count);

            List<SimulationsReportRecord> simulationsReportRecords = new List<SimulationsReportRecord>();

            foreach (string configurationName in new List<string>() { "naive-skip_break", "async-skip_break" })
            {
                var filteredRecords = simulationResults.FindAll(x => x.Name.StartsWith(configurationName));

                var sortedMissing = filteredRecords.OrderBy(x => x.PlannedTimeProductionPlanCount).ToList();
                simulationsReportRecords.Add(new SimulationsReportRecord()
                {
                    ConfigurationName = configurationName,
                    PercentageOfCompletePlans = filteredRecords.Count(x => x.PlannedTimeProductionPlanCount == 0) / (double)filteredRecords.Count * 100,
                    AverageMissingCars = filteredRecords.Sum(x => x.PlannedTimeProductionPlanCount) / (double)filteredRecords.Count,
                    AverageDelay = filteredRecords.Sum(x => x.DelayTime) / (double)filteredRecords.Count,
                    MissingCars90Percentile = sortedMissing.ElementAt((int)(sortedMissing.Count*0.9)).PlannedTimeProductionPlanCount
                });
            }

            foreach (var item in simulationsReportRecords)
            {
                Console.WriteLine(item);
            }
        }

        private void ClearSimulationResults()
        {
            simulationResults.Clear();
        }

        Dictionary<string, RealProductionSimulator> GetSimulationsDict(string key, string warehouseFilename, string historyFilename, string planFilename)
        {
            Dictionary<string, RealProductionSimulator> simulationsDict = new Dictionary<string, RealProductionSimulator>();

            simulationsDict[$"naive-skip_break-{key}"] = new RealProductionSimulator(
                    new NaiveController(new ProductionState(12, 4), warehouseFilename, historyFilename, planFilename)
                    );

            simulationsDict[$"naive-reorganization-{key}"] = new RealProductionSimulator(
                new NaiveController(new ProductionState(12, 4), warehouseFilename, historyFilename, planFilename),
                new GreedyWarehouseReorganizer(new NaiveController(null), 5, 1)
                );

            /*simulationsDict[$"async-skip_break-{key}"] = new RealProductionSimulator(
                new NaiveAsyncControllerWithHalfCycleDelay(new ProductionState(12, 4), warehouseFilename, historyFilename, planFilename)
                );

            simulationsDict[$"async-reorganization-{key}"] = new RealProductionSimulator(
                new NaiveAsyncControllerWithHalfCycleDelay(new ProductionState(12, 4), warehouseFilename, historyFilename, planFilename),
                new GreedyWarehouseReorganizer(new NaiveController(null), 5, 1)
                );*/
            return simulationsDict;
        }

        public void SimulateGeneratedDayScenarios()
        {
            ClearSimulationResults();
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            int completed = 0;
            var sw = new Stopwatch();
            sw.Start();
            var result = Parallel.For(0, GeneratedScenariosNum, (j) =>
            {
                int i = RandomSequence[j];
                var warehouseFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\WarehouseInitialState_daily{i}.csv");
                var historyFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\HistoricalProductionList_daily{i}.txt");
                var planFilename = Path.Combine(startupPath, $@"robot-simulator\robot-simulator\DailyPlans\generated_situation_daily{i}\FutureProductionList_daily{i}.txt");

                var simulations = GetSimulationsDict(i.ToString(), warehouseFilename, historyFilename, planFilename);
                RunSimulations(simulations, simulationResults);  //TODO: consult if is necessary to lock list of results in threads
                Interlocked.Increment(ref completed);
                Trace.WriteLine($"Completed: {completed}");
                Console.WriteLine($"Completed: {completed}");

            });
            sw.Stop();
            Trace.WriteLine($"Time for one run: {sw.ElapsedMilliseconds}");
        }

        void RunSimulations(Dictionary<string, RealProductionSimulator> simulationsDict, List<SimulationResult> simulationResults)
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

        int GetPlanedTime(int plannedProductionLength)
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

        SimulationResult SimulateRun(RealProductionSimulator productionSimulator)
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
