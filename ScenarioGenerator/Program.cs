using OptimizationLogic.DAL;
using OptimizationLogic.DTO;
using OptimizationLogic.StateGenerating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScenarioGenerator
{
    class Program
    {
        private static void GenerateNewScenariosWith100Items()
        {
            var historyGenerator = new ProductionHistoryGenerator(0.5)
            {
                ProbabilityOfStartingInMqbState = 0.5,
                SequenceLength = 64,
            };

            var futureGenerator = new FutureProductionPlanGenerator(0.5)
            {
                ProbabilityOfStartingInMqbState = 0.5,
                SequenceLength = 100
            };

            var loaderFiles = Directory.GetDirectories("InputFiles", "situation*")
                .Select(Directory.GetFiles)
                .Select(t =>
                {
                    return new ProductionScenarioPaths()
                    {
                        FutureProductionListCsv = t.FirstOrDefault(s => s.Contains("Future")),
                        HistoricalProductionListCsv = t.FirstOrDefault(s => s.Contains("Historical")),
                        WarehouseInitialStateCsv = t.FirstOrDefault(s => s.Contains("Warehouse"))
                    };
                })
                .ToList();
            var loader = new ProductionStateLoader(loaderFiles, "InputFiles/ProcessingTimeMatrix.csv");
            var prodStateGenerator = new ProductionStateGenerator(historyGenerator, futureGenerator, loader.TimeMatrix, 0.5, 0.5, 1.0);

            var logFile = File.CreateText("probability_log.txt");
            logFile.WriteLine("Id;ProbabilityOfStartingInMqbState;MqbToMebTransitionProbabilityHistory;MqbToMebTransitionProbabilityFuture;UniformProbabilityWeight;MqbDistanceWeight;MebDistanceWeight");
            int suffix = 1;
            for (int historyProb = 0; historyProb <= 10; historyProb++)
            {
                for (int futureProb = 0; futureProb <= 10; futureProb++)
                {
                    for (int warehouseCount = 0; warehouseCount < 10; warehouseCount++)
                    {
                        prodStateGenerator.ProductionHistoryGenerator.MqbToMebTransitionProbability = historyProb / 10.0;
                        prodStateGenerator.FutureProductionPlanGenerator.MqbToMebTransitionProbability = futureProb / 10.0;
                        prodStateGenerator.UniformProbabilityWeight = prodStateGenerator.RandomGenerator.Next(0, 101) / 100.0;
                        prodStateGenerator.MebDistanceWeight = prodStateGenerator.RandomGenerator.Next(0, 101) / 100.0;
                        prodStateGenerator.MqbDistanceWeight = prodStateGenerator.RandomGenerator.Next(0, 101) / 100.0;
                        var state = prodStateGenerator.GenerateProductionState();
                        state.SaveProductionState("gen", suffix.ToString());
                        logFile.WriteLine($"{suffix};{prodStateGenerator.FutureProductionPlanGenerator.ProbabilityOfStartingInMqbState};{prodStateGenerator.ProductionHistoryGenerator.MqbToMebTransitionProbability};{prodStateGenerator.FutureProductionPlanGenerator.MqbToMebTransitionProbability};{prodStateGenerator.UniformProbabilityWeight};{prodStateGenerator.MqbDistanceWeight};{prodStateGenerator.MebDistanceWeight}");
                        suffix++;
                        Console.WriteLine($"Processed:{suffix}");
                    }
                }
            }
            logFile.Close();
        }

        private static void GenerateFromExistingPlans(int seqLen, string folder)
        {
            var futureGenerator = new FutureProductionPlanGenerator(0.5)
            {
                ProbabilityOfStartingInMqbState = 0.5,
                SequenceLength = seqLen
            };

            var probabilities = File.ReadAllLines("probability_log.txt").Select(t => t.Split(';')).Select(t => t[3]).Skip(1).Select(double.Parse).ToArray();

            var loaderFiles = Directory.GetDirectories("GeneratedInput", "generated_situation*")
                .Select(Directory.GetFiles)
                .Select(t =>
                {
                    return new ProductionScenarioPaths()
                    {
                        FutureProductionListCsv = t.FirstOrDefault(s => s.Contains("Future")),
                        HistoricalProductionListCsv = t.FirstOrDefault(s => s.Contains("Historical")),
                        WarehouseInitialStateCsv = t.FirstOrDefault(s => s.Contains("Warehouse"))
                    };
                })
                .ToList();
            var loader = new ProductionStateLoader(loaderFiles, "GeneratedInput/ProcessingTimeMatrix.csv");

            for (int i = 0; i < loader.DefaultScenariosInMemory.Count; i++)
            {
                var state = loader.LoadScenarioFromMemory(i);
                futureGenerator.MqbToMebTransitionProbability = probabilities[i];
                var seq = futureGenerator.GenerateSequence();
                state.FutureProductionPlan = new Queue<ItemState>(seq);
                state.SaveProductionState(folder, $"_weekly{i}");
                Console.WriteLine($"Processed: {i}");
            }
        }
        
        static void Main(string[] args)
        {
            GenerateFromExistingPlans(8968, "WeeklyPlans");
        }
    }
}
