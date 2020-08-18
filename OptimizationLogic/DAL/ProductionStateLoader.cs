﻿using OptimizationLogic.DTO;
using System.Collections.Generic;
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
                var productionState = new ProductionState();
                productionState.LoadTimeMatrix(TimeMatrixCsvPath);
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
            productionState.LoadTimeMatrix(TimeMatrixCsvPath);
            productionState.LoadWarehouseState(current.WarehouseInitialStateCsv);
            productionState.LoadFutureProductionPlan(current.FutureProductionListCsv);
            productionState.LoadProductionHistory(current.HistoricalProductionListCsv);
        }

        public void LoadScenarioFromMemory(ProductionState productionState, int scenarioIdx = 0)
        {
            var current = (ProductionState)DefaultScenariosInMemory[scenarioIdx].Clone();
            productionState.TimeMatrix = current.TimeMatrix;
            productionState.FutureProductionPlan = current.FutureProductionPlan;
            productionState.ProductionHistory = current.ProductionHistory;
            productionState.WarehouseState = current.WarehouseState;
        }
    }

    public class ProductionScenarioPaths
    {
        public string WarehouseInitialStateCsv { get; set; }
        public string HistoricalProductionListCsv { get; set; }
        public string FutureProductionListCsv { get; set; }
    }
}
