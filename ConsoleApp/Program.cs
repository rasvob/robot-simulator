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
            RunNaiveVersion();
        }
        static void RunNaiveVersion()
        {
            ProductionState productionState = new ProductionState();
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            NaiveController naiveController = new NaiveController(productionState,
                Path.Combine(startupPath, @"OptimizationLogic\InputFiles\ProcessingTimeMatrix.csv"),
                Path.Combine(startupPath, @"OptimizationLogic\InputFiles\situation1\WarehouseInitialState.csv"),
                Path.Combine(startupPath, @"OptimizationLogic\InputFiles\situation1\HistoricalProductionList.txt"),
                Path.Combine(startupPath, @"OptimizationLogic\InputFiles\situation1\FutureProductionList.txt"));


            while (productionState.ProductionStateIsOk && productionState.FutureProductionPlan.Count > 0)
            {
                Console.WriteLine(String.Format("Step {0}", naiveController.ProductionState.StepCounter));
                naiveController.NextStep();
            }
            if (productionState.ProductionStateIsOk == false)
            {
                Console.WriteLine(String.Format("Error occured in step {0}", naiveController.ProductionState.StepCounter));
            }

            foreach (StepModel stepModel in naiveController.StepLog)
            {
                Console.WriteLine(stepModel);
            }
        }
    }
}
