using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.DAL
{
    public class ProductionStateLoader
    {
        public ProductionStateLoader(List<ProductionScenarioPaths> scenarios, string timeMatrixCsvPath)
        {
            DefaultScenarios = scenarios;
            TimeMatrixCsvPath = timeMatrixCsvPath;
        }

        public List<ProductionScenarioPaths> DefaultScenarios { get; set; }
        public string TimeMatrixCsvPath { get; set; }

        public void LoadScenarion(ProductionState productionState, int scenarioIdx=0)
        {
            var current = DefaultScenarios[scenarioIdx];

            productionState.LoadTimeMatrix(TimeMatrixCsvPath);
            productionState.LoadWarehouseState(current.WarehouseInitialStateCsv);
            productionState.LoadFutureProductionPlan(current.FutureProductionListCsv);
            productionState.LoadProductionHistory(current.HistoricalProductionListCsv);
        }
    }

    public class ProductionScenarioPaths
    {
        public string WarehouseInitialStateCsv { get; set; }
        public string HistoricalProductionListCsv { get; set; }
        public string FutureProductionListCsv { get; set; }
    }
}
