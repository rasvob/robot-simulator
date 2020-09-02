using OptimizationLogic.AsyncControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimizationLogic.DTO;

namespace OptimizationLogic
{
    public class RealProductionSimulator: IController
    {
        public BaseController Controller { get; set; }
        public GreedyWarehouseReorganizer WarehouseReorganizer
        {
            get { return warehouseReorganizer; }
            set
            {
                if (value == warehouseReorganizer)
                {
                    return;
                }

                if (warehouseReorganizer != null)
                {
                    warehouseReorganizer.ProgressTriggered -= WarehouseReorganizer_ProgressTriggered;
                }
                warehouseReorganizer = value;

                if (value != null)
                {
                    warehouseReorganizer.ProgressTriggered += WarehouseReorganizer_ProgressTriggered;
                }
            }
        }

        private void WarehouseReorganizer_ProgressTriggered(object sender, ProgressEventArgs e)
        {
            WarehouseReorganizationProgressUpdated?.Invoke(this, e);
        }

        private List<Tuple<double, double>> productionDayBreaks = new List<Tuple<double, double>>();
        private GreedyWarehouseReorganizer warehouseReorganizer;
        private const int secondsInProductionDay = 86400;
        private const int tactTime = 55;
        public event EventHandler<ProgressEventArgs> WarehouseReorganizationProgressUpdated;
        public RealProductionSimulator(BaseController controller, GreedyWarehouseReorganizer warehouseReorganizer = null)
        {
            Controller = controller;
            WarehouseReorganizer = warehouseReorganizer;

            productionDayBreaks.Add(new Tuple<double, double>(7200, 7500));
            productionDayBreaks.Add(new Tuple<double, double>(15300, 17100));
            productionDayBreaks.Add(new Tuple<double, double>(21600, 21900));
            productionDayBreaks.Add(new Tuple<double, double>(28500, 28800));
            productionDayBreaks.Add(new Tuple<double, double>(36000, 36300));
            productionDayBreaks.Add(new Tuple<double, double>(44100, 45900));
            productionDayBreaks.Add(new Tuple<double, double>(50400, 50700));
            productionDayBreaks.Add(new Tuple<double, double>(64800, 65100));
            productionDayBreaks.Add(new Tuple<double, double>(72900, 74700));
            productionDayBreaks.Add(new Tuple<double, double>(79200, 79500));
            productionDayBreaks.Add(new Tuple<double, double>(86100, 86400));
        }

        private int GetBreakTimeIndex(double realTimeStamp)
        {
            double time = (realTimeStamp + secondsInProductionDay) % secondsInProductionDay;

            for (int i = 0; i < productionDayBreaks.Count; i++)
            {
                var breakPair = productionDayBreaks[i];
                if (time >= breakPair.Item1 - tactTime && time < breakPair.Item2)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool NextStep()
        {
            var breakTimeIndex = GetBreakTimeIndex(Controller.RealTime);
            if (breakTimeIndex >= 0 && Controller.IsReadyForBreak)
            {
                if (WarehouseReorganizer != null)
                {
                    WarehouseReorganizationProgressUpdated?.Invoke(this, new ProgressEventArgs() { State = ProgressState.Start, CurrentValue = WarehouseReorganizer.MaxDepth});
                }
                var breakPair = productionDayBreaks[breakTimeIndex];
                var breakDuration = (breakPair.Item2 - (Controller.RealTime % secondsInProductionDay)) % secondsInProductionDay;
                Controller.StepLog.Add(new BaseStepModel() { Message = $"Break time (duration {breakDuration})" });
                WarehouseReorganizer?.ReorganizeWarehouse(Controller.ProductionState, Controller.StepLog, breakDuration);
                Controller.RealTime += breakDuration;
                if (WarehouseReorganizer != null)
                {
                    WarehouseReorganizationProgressUpdated?.Invoke(this, new ProgressEventArgs() { State = ProgressState.End, CurrentValue = WarehouseReorganizer.MaxDepth });
                }
            }
            else
            {
                Controller.NextStep();
            }

            return true;
        }
    }
}
