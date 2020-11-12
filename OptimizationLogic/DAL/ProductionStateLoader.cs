using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OptimizationLogic.DAL
{
    public class ProductionStateLoader
    {
        public ProductionStateLoader(List<ProductionScenarioPaths> scenarios, string timeMatrixCsvPath)
        {
            DefaultScenarios = scenarios;
            TimeMatrixCsvPath = timeMatrixCsvPath;
            DefaultScenariosInMemory = DefaultScenarios.Select(current => {
                var productionState = new ProductionState(12, 4);
                productionState.LoadWarehouseState(current.WarehouseInitialStateCsv);
                productionState.LoadFutureProductionPlan(current.FutureProductionListCsv);
                productionState.LoadProductionHistory(current.HistoricalProductionListCsv);
                return productionState;
            }).ToList();
        }

        public List<ProductionScenarioPaths> DefaultScenarios { get; set; }
        public List<ProductionState> DefaultScenariosInMemory { get; set; }
        public string TimeMatrixCsvPath { get; set; }
        public void LoadScenarioFromDisk(ProductionState productionState, int scenarioIdx=0)
        {
            var current = DefaultScenarios[scenarioIdx];
            productionState.LoadWarehouseState(current.WarehouseInitialStateCsv);
            productionState.LoadFutureProductionPlan(current.FutureProductionListCsv);
            productionState.LoadProductionHistory(current.HistoricalProductionListCsv);
        }

        public void LoadScenarioFromMemory(ProductionState productionState, int scenarioIdx = 0)
        {
            var current = (ProductionState)DefaultScenariosInMemory[scenarioIdx].Clone();
            productionState.FutureProductionPlan = current.FutureProductionPlan;
            productionState.ProductionHistory = current.ProductionHistory;
            productionState.WarehouseState = current.WarehouseState;
        }

        public void LoadScenarioFromFolder(ProductionState productionState, string folder)
        {
            var files = Directory.GetFiles(folder);
            var current = new ProductionScenarioPaths()
            {
                    FutureProductionListCsv = files.FirstOrDefault(s => s.Contains("Future")),
                    HistoricalProductionListCsv = files.FirstOrDefault(s => s.Contains("Historical")),
                    WarehouseInitialStateCsv = files.FirstOrDefault(s => s.Contains("Warehouse"))
            };

            if (!current.IsValid)
            {
                throw new ArgumentException($"Folder: {folder} does not contain required files");
            }

            productionState.LoadWarehouseState(current.WarehouseInitialStateCsv);
            productionState.LoadFutureProductionPlan(current.FutureProductionListCsv);
            productionState.LoadProductionHistory(current.HistoricalProductionListCsv);
        }

        public ProductionState LoadScenarioFromMemory(int scenarioIdx = 0)
        {
            return (ProductionState)DefaultScenariosInMemory[scenarioIdx].Clone();
        }
    }

    public class ProductionScenarioPaths
    {
        public string WarehouseInitialStateCsv { get; set; }
        public string HistoricalProductionListCsv { get; set; }
        public string FutureProductionListCsv { get; set; }
        public bool IsValid => !(string.IsNullOrEmpty(WarehouseInitialStateCsv) || string.IsNullOrEmpty(HistoricalProductionListCsv) || string.IsNullOrEmpty(FutureProductionListCsv));
    }
}
