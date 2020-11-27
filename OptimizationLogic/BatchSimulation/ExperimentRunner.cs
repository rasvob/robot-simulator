using OptimizationLogic.AsyncControllers;
using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OptimizationLogic.BatchSimulation
{
    public enum AttributeName
    {
        Delay,
        MissingCars
    }

    public class ExperimentRunner
    {
        private int _counter = 0;

        public int Counter
        {
            get { return _counter; }
            set { _counter = value; }
        }
        public event EventHandler<ProgressEventArgs> CounterUpdated;
        protected virtual void OnCounterUpdated(ProgressEventArgs e)
        {
            CounterUpdated?.Invoke(this, e);
        }

        public ExperimentConfig Config { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public ExperimentRunner(ExperimentConfig config)
        {
            Config = config;
        }

        public ExperimentRunner()
        {

        }

        public ExperimentResults RunExperiments()
        {
            Counter = 0;
            OnCounterUpdated(new ProgressEventArgs { CurrentValue = Counter, State = ProgressState.Start });

            ExperimentResults results = new ExperimentResults();
            results.SimulationResults = SimulateScenarios();

            // aggregate results
            var simulators = GetSimulationsDict();
            foreach (string configurationName in simulators.Keys) 
            {
                var filteredRecords = results.SimulationResults.FindAll(x => x.ConfigurationName == configurationName);
                if (filteredRecords.Count == 0)
                    continue;

                var sortedMissing = filteredRecords.OrderBy(x => x.MissingCarsCount).ToList();
                var sortedDelay = filteredRecords.OrderBy(x => x.Delay).ToList();
                results.AggregatedResults.Add(new AggregatedResults()
                {
                    ConfigurationName = configurationName,
                    PercentageOfCompletePlans = filteredRecords.Count(x => x.MissingCarsCount == 0) / (double)filteredRecords.Count * 100,
                    AverageMissingCars = filteredRecords.Sum(x => x.MissingCarsCount) / (double)filteredRecords.Count,
                    MedianMissingCars = GetPercentileValue(sortedMissing, AttributeName.MissingCars, 0.5),
                    AverageDelay = filteredRecords.Sum(x => x.Delay) / (double)filteredRecords.Count,
                    MedianDelay = GetPercentileValue(sortedDelay, AttributeName.Delay, 0.5),
                    MissingCars90Percentile = GetPercentileValue(sortedMissing, AttributeName.MissingCars, 0.9)
                });
            }

            OnCounterUpdated(new ProgressEventArgs { CurrentValue = Counter, State = ProgressState.End });
            return results;
        }

        private List<SingleSimulationResult> SimulateScenarios()
        {
            var simulators = GetSimulationsDict();
            SingleSimulationResult[] simulationResultsArray = Enumerable.Repeat(0, Config.ProductionStates.Count * simulators.Count).Select(T => new SingleSimulationResult()).ToArray();

            var result = Parallel.For(0, Config.ProductionStates.Count, (i) =>
            {
                foreach (var simulation in simulators)
                {
                    var simulationName = simulation.Key;
                    var simulatorOrder = simulation.Value.Item1;
                    var simulator = simulation.Value.Item2;

                    var resultIndex = i * simulators.Count + simulatorOrder;
                    simulationResultsArray[resultIndex].ConfigurationName = simulationName;
                    simulationResultsArray[resultIndex].SimulationNumber = i;

                    var localSimulator = simulator.CreateNew((DTO.ProductionState)Config.ProductionStates[i].Clone());
                    SimulateSingleRun(localSimulator, simulationResultsArray[resultIndex]);

                    CancellationToken.ThrowIfCancellationRequested();
                }
                Interlocked.Increment(ref _counter);
                OnCounterUpdated(new ProgressEventArgs { CurrentValue = Counter, State = ProgressState.Update });
            });

            return new List<SingleSimulationResult>(simulationResultsArray);
        }

        Dictionary<string, Tuple<int, RealProductionSimulator>> GetSimulationsDict()
        {
            Dictionary<string, Tuple<int, RealProductionSimulator>> simulationsDict = new Dictionary<string, Tuple<int, RealProductionSimulator>>();

            simulationsDict[$"naive-skip_break"] = new Tuple<int, RealProductionSimulator>(0, new RealProductionSimulator(new NaiveController(null, Config.ClockTime, Config.TimeLimit)));

            simulationsDict[$"async-skip_break"] = new Tuple<int, RealProductionSimulator>(1, new RealProductionSimulator(new NaiveAsyncControllerWithHalfCycleDelay(null, Config.ClockTime, Config.TimeLimit)));


            if (Config.UseReorganization)
            {
                simulationsDict[$"naive-reorganization"] = new Tuple<int, RealProductionSimulator>(2,
                    new RealProductionSimulator(new NaiveController(null, Config.ClockTime, Config.TimeLimit),
                    new GreedyWarehouseReorganizer(new NaiveController(null, Config.ClockTime, Config.TimeLimit), 5, 1) { CancellationToken = CancellationToken }));

                simulationsDict[$"async-reorganization"] = new Tuple<int, RealProductionSimulator>(3,
                    new RealProductionSimulator(new NaiveAsyncControllerWithHalfCycleDelay(null, Config.ClockTime, Config.TimeLimit),
                    new GreedyWarehouseReorganizer(new NaiveController(null, Config.ClockTime, Config.TimeLimit), 5, 1) { CancellationToken = CancellationToken }));
            }

            return simulationsDict;
        }

        private double GetPercentileValue(List<SingleSimulationResult> orderedList, AttributeName attribute, double percentile)
        {
            if (orderedList.Count % 2 == 0)
            {
                int indexLower = (int)(orderedList.Count * percentile);
                switch (attribute)
                {
                    case AttributeName.Delay:
                        return (orderedList.ElementAt(indexLower).Delay + orderedList.ElementAt(indexLower < orderedList.Count - 1 ? indexLower + 1 : indexLower).Delay) / 2;
                    case AttributeName.MissingCars:
                        return (orderedList.ElementAt(indexLower).MissingCarsCount + orderedList.ElementAt(indexLower < orderedList.Count - 1 ? indexLower + 1 : indexLower).MissingCarsCount) / 2;
                }
            }
            else
            {
                switch (attribute)
                {
                    case AttributeName.Delay:
                        return orderedList.ElementAt((int)(orderedList.Count * percentile)).Delay;
                    case AttributeName.MissingCars:
                        return orderedList.ElementAt((int)(orderedList.Count * percentile)).MissingCarsCount;
                }
            }
            throw new ArgumentException("Wrong arguments in GetPercentileValue");
        }

        void SimulateSingleRun(RealProductionSimulator productionSimulator, SingleSimulationResult simulationResult)
        {
            productionSimulator.Controller.RealTime = -300;
            var plannedRealProcessingTime = productionSimulator.GetPlannedTimeWithBreaks();

            while (productionSimulator.Controller.ProductionState.FutureProductionPlan.Count > 0)
            {
                productionSimulator.NextStep();

                if (productionSimulator.Controller.RealTime <= plannedRealProcessingTime)
                {
                    simulationResult.MissingCarsCount = productionSimulator.Controller.ProductionState.FutureProductionPlan.Count;
                }
            }

            simulationResult.Delay = productionSimulator.Controller.Delay;
        }
    }
}
