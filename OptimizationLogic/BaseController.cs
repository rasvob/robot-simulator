using OptimizationLogic.DTO;
using OptimizationLogic.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic
{
    public abstract class BaseController : IController
    {
        public ProductionState ProductionState
        {
            get { return _productionState; }
            set 
            {
                _productionState = value;
                if (value != null)
                {
                    InitSortedPositionCodes();
                }
            }
        }
        public List<BaseStepModel> StepLog { get; set; } = new List<BaseStepModel>();

        public Stack<ProductionState> History { get; set; } = new Stack<ProductionState>();
        protected Dictionary<PositionCodes, List<PositionCodes>> SortedPositionCodes;
        private ProductionState _productionState;

        public int TimeLimit { get; set; }
        public int TimeLimitForOneStep { get => TimeLimit; }
        public int ClockTime { get; set; }

        public double RealTime { get; set; } = -300;
        public double Delay { get; set; } = 0;
        public double BreakTime { get; set; } = -300;
        public bool IsReadyForBreak { get; set; } = true;

        public void InitSortedPositionCodes()
        {
            if (ProductionState is null)
            {
                return;
            }
            
            SortedPositionCodes = new Dictionary<PositionCodes, List<PositionCodes>>();
            foreach (PositionCodes sourcePosition in ((PositionCodes[])Enum.GetValues(typeof(PositionCodes))).FilterPositions(ProductionState.WarehouseRows, ProductionState.WarehouseColls))
            {
                Dictionary<PositionCodes, double> cellsTimes = new Dictionary<PositionCodes, double>();

                foreach (PositionCodes destinationPosition in ((PositionCodes[])Enum.GetValues(typeof(PositionCodes))).FilterPositions(ProductionState.WarehouseRows, ProductionState.WarehouseColls))
                {
                    cellsTimes[destinationPosition] = ProductionState[PositionCodes.Stacker, destinationPosition];
                }

                SortedPositionCodes[sourcePosition] = cellsTimes.OrderBy(i => i.Value).Select(x => x.Key).ToList();
            }
        }

        public BaseController(ProductionState productionState, string csvWarehouseInitialState, string csvHistroicalProduction, string csvFutureProductionPlan)
        {
            ProductionState = productionState;
            ProductionState.LoadFutureProductionPlan(csvFutureProductionPlan);
            ProductionState.LoadProductionHistory(csvHistroicalProduction);
            ProductionState.LoadWarehouseState(csvWarehouseInitialState);
            //InitSortedPositionCodes();
            SetControllerTimes();
        }

        public BaseController(ProductionState state)
        {
            ProductionState = state;
            //InitSortedPositionCodes();
            SetControllerTimes();
        }

        public BaseController(ProductionState state, int tactTime, int operationLimit)
        {
            ProductionState = state;
            //InitSortedPositionCodes();
            SetControllerTimes(tactTime, operationLimit);
        }

        public virtual void SetControllerTimes(int tactTime = 55, int operationLimit = 36)
        {
            TimeLimit = operationLimit;
            ClockTime = tactTime;
        }

        public int InitialIntakeTime
        {
            get { return (int)((ClockTime - TimeLimit) / 2); }
        }

        public virtual void RenewControllerState()
        {
            InitSortedPositionCodes();
            StepLog.Clear();
            History.Clear();
            Delay = 0;
            RealTime = -300;
            IsReadyForBreak = true;
        }
        public virtual void SetTimebaseShift(double value)
        {
        }

        public virtual void IncreaseTimebaseShift(double value)
        {
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

        public abstract BaseController CreateNew(ProductionState state);
    }
}
