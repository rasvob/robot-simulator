using OptimizationLogic;
using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace robot_simulator.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private ObservableCollection<WarehouseItemViewModel> currentWarehouseState;

        public NaiveController NaiveController { get; set; }
        public ProductionState ProductionState { get; set; }

        public ICommand NextStep { get; private set; }

        public ObservableCollection<WarehouseItemViewModel> CurrentWarehouseState { get => currentWarehouseState;
            set
            {
                if (currentWarehouseState != value)
                {
                    currentWarehouseState = value;
                    OnPropertyChanged(nameof(CurrentWarehouseState));
                }
            }
        }

        public MainWindowViewModel(NaiveController naiveController)
        {
            NaiveController = naiveController;
            ProductionState = NaiveController.ProductionState;

            NextStep = new SimpleCommand(NextStepClickedExecute);
            CurrentWarehouseState = new ObservableCollection<WarehouseItemViewModel>(CreateWarehouseViewModelCollection());
        }

        public void NextStepClickedExecute(object o)
        {
            NaiveController.NextStep();
        }

        public IEnumerable<WarehouseItemViewModel> CreateWarehouseViewModelCollection()
        {
            var whRows = Enumerable.Range(0, ProductionState.WarehouseRows);
            var whColls = Enumerable.Range(0, ProductionState.WarehouseColls);
            return whRows
                .SelectMany(row => whColls.Select(col => (row, col)))
                .Select(cell => new WarehouseItemViewModel()
                {
                    Row = cell.row,
                    Col = cell.col,
                    PosiionCode = ProductionState.GetWarehouseCell(cell.row, cell.col).ToGuiString(),
                    StateStr = ProductionState.WarehouseState[cell.row, cell.col].ToString(),
                });
        }
    }

}
