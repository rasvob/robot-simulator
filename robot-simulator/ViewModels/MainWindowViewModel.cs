using OptimizationLogic;
using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace robot_simulator.ViewModels
{
    public class MainWindowViewModel: BaseViewModel
    {
        public NaiveController NaiveController { get; set; }
        public ProductionState ProductionState { get; set; }

        public ICommand NextStep { get; private set; }

        public MainWindowViewModel(NaiveController naiveController)
        {
            NaiveController = naiveController;
            ProductionState = NaiveController.ProductionState;
            NextStep = new SimpleCommand((_) => NaiveController.NextStep());
        }
    }
}
