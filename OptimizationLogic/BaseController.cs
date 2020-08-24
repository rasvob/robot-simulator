using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic
{
    public abstract class BaseController
    {
        public ProductionState ProductionState { get; set; }
        public List<StepModel> StepLog { get; set; } = new List<StepModel>();

        public Stack<ProductionState> History { get; set; } = new Stack<ProductionState>();

        protected List<PositionCodes> SortedPositionCodes;
        protected const int TimeLimit = 36;
        public int TimeLimitForOneStep { get => TimeLimit; }
        public int ClockTime { get => 55; }

        public void InitSortedPositionCodes()
        {
            Dictionary<PositionCodes, double> cellsTimes = new Dictionary<PositionCodes, double>();
            foreach (PositionCodes positionCode in Enum.GetValues(typeof(PositionCodes)))
            {
                if (positionCode != PositionCodes.Service && positionCode != PositionCodes.Stacker)
                {
                    cellsTimes[positionCode] = ProductionState.TimeMatrix[ProductionState.GetTimeMatrixIndex(PositionCodes.Stacker), ProductionState.GetTimeMatrixIndex(positionCode)];
                }
            }
            SortedPositionCodes = cellsTimes.OrderBy(i => i.Value).Select(x => x.Key).ToList();
        }

        public BaseController(ProductionState productionState, string csvProcessingTimeMatrix, string csvWarehouseInitialState, string csvHistroicalProduction, string csvFutureProductionPlan)
        {
            ProductionState = productionState;
            ProductionState.LoadFutureProductionPlan(csvFutureProductionPlan);
            ProductionState.LoadProductionHistory(csvHistroicalProduction);
            ProductionState.LoadTimeMatrix(csvProcessingTimeMatrix);
            ProductionState.LoadWarehouseState(csvWarehouseInitialState);
            InitSortedPositionCodes();
        }

        public BaseController(ProductionState state, string csvProcessingTimeMatrix)
        {
            ProductionState = state;
            ProductionState.LoadTimeMatrix(csvProcessingTimeMatrix);
            InitSortedPositionCodes();
        }

        public BaseController(ProductionState state)
        {
            ProductionState = state;
            InitSortedPositionCodes();
        }
        public void RenewControllerState()
        {
            InitSortedPositionCodes();
            StepLog.Clear();
            History.Clear();
        }

        protected PositionCodes GetNearestEmptyPosition()
        {
            return GetNearesElementWarehousePosition(ItemState.Empty);
        }

        protected PositionCodes GetNearesElementWarehousePosition(ItemState itemState)
        {
            foreach (PositionCodes positionCode in SortedPositionCodes)
            {
                (int r, int c) = ProductionState.GetWarehouseIndex(positionCode);
                if (ProductionState.WarehouseState[r, c] == itemState)
                {
                    return positionCode;
                }
            }
            throw new ArgumentException("ItemState is not in warehouse.");
        }

        public void Undo()
        {
            ProductionState = History.Pop();
            StepLog.RemoveAt(StepLog.Count - 1);
        }

        public bool CanUndo() => History.Count > 0;

        public abstract bool NextStep();
    }
}
