using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic
{
    public class NaiveController
    {
        public ProductionState State { get; set; }
        public List<StepModel> StepLog { get; set; } = new List<StepModel>();

        public NaiveController(ProductionState state, string csvProcessingTimeMatrix, string csvWarehouseInitialState, string csvHistroicalProduction, string csvFutureProductionPlan)
        {
            State = state;
            State.LoadFutureProductionPlan(csvFutureProductionPlan);
            State.LoadProductionHistory(csvHistroicalProduction);
            State.LoadTimeMatrix(csvProcessingTimeMatrix);
            State.LoadWarehouseState(csvWarehouseInitialState);
        }

        // TODO: Generate random inputs
        public NaiveController(ProductionState state, string csvProcessingTimeMatrix)
        {
            State = state;
            State.LoadTimeMatrix(csvProcessingTimeMatrix);
        }

        public void NextStep()
        {
            var needed = State.FutureProductionPlan.Dequeue();
            var current = State.ProductionHistory.Dequeue();
        }
    }
}
