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
        public List<BaseStepModel> StepLog { get; set; } = new List<BaseStepModel>();

        public Stack<ProductionState> History { get; set; } = new Stack<ProductionState>();
        protected Dictionary<PositionCodes, List<PositionCodes>> SortedPositionCodes;
        protected const int TimeLimit = 36;
        public int TimeLimitForOneStep { get => TimeLimit; }
        public int ClockTime { get => 55; }

        public void InitSortedPositionCodes()
        {
            SortedPositionCodes = new Dictionary<PositionCodes, List<PositionCodes>>();
            foreach (PositionCodes sourcePosition in Enum.GetValues(typeof(PositionCodes)))
            {
                Dictionary<PositionCodes, double> cellsTimes = new Dictionary<PositionCodes, double>();

                foreach (PositionCodes destinationPosition in Enum.GetValues(typeof(PositionCodes)))
                {
                    cellsTimes[destinationPosition] = ProductionState.TimeMatrix[ProductionState.GetTimeMatrixIndex(PositionCodes.Stacker), ProductionState.GetTimeMatrixIndex(destinationPosition)];
                }

                SortedPositionCodes[sourcePosition] = cellsTimes.OrderBy(i => i.Value).Select(x => x.Key).ToList();
            }            
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
        public virtual void RenewControllerState()
        {
            InitSortedPositionCodes();
            StepLog.Clear();
            History.Clear();
        }

        protected PositionCodes GetNearestEmptyPosition(ProductionState actualProductionState)
        {
            return GetNearesElementWarehousePosition(actualProductionState, ItemState.Empty);
        }
        protected PositionCodes GetNearestEmptyPosition(ProductionState actualProductionState, PositionCodes sourcePosition)
        {
            return GetNearesElementWarehousePosition(actualProductionState, sourcePosition, ItemState.Empty);
        }
        protected PositionCodes GetNearesElementWarehousePosition(ProductionState actualProductionState, ItemState itemState)
        {
            return GetNearesElementWarehousePosition(actualProductionState, PositionCodes.Stacker, itemState);
        }
        protected PositionCodes GetNearesElementWarehousePosition(ProductionState actualProductionState, PositionCodes sourcePosition, ItemState itemState)
        {
            foreach (PositionCodes positionCode in SortedPositionCodes[sourcePosition])
            {
                (int r, int c) = actualProductionState.GetWarehouseIndex(positionCode);
                if (actualProductionState.WarehouseState[r, c] == itemState)
                {
                    return positionCode;
                }
            }
            throw new ArgumentException("ItemState is not in warehouse.");
        }

        public virtual void Undo()
        {
            ProductionState = History.Pop();
            StepLog.RemoveAt(StepLog.Count - 1);
        }

        public virtual bool CanUndo() => History.Count > 0;

        public abstract bool NextStep();
    }
}
