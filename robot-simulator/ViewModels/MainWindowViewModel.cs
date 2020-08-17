using OptimizationLogic;
using OptimizationLogic.DAL;
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
        public ICommand LoadSelectedScenario { get; private set; }

        public ObservableCollection<WarehouseItemViewModel> CurrentWarehouseState
        {
            get => currentWarehouseState;
            set
            {
                if (currentWarehouseState != value)
                {
                    currentWarehouseState = value;
                    OnPropertyChanged(nameof(CurrentWarehouseState));
                }
            }
        }

        private ObservableCollection<WarehouseItemViewModel> historyQueue;
        private ObservableCollection<WarehouseItemViewModel> futureQueue;

        public ObservableCollection<WarehouseItemViewModel> HistoryQueue
        {
            get => historyQueue;

            set
            {
                if (historyQueue != value)
                {
                    historyQueue = value;
                    OnPropertyChanged(nameof(HistoryQueue));
                }
            }
        }

        public ObservableCollection<WarehouseItemViewModel> FutureQueue { get => futureQueue;
            set
            {
                if (futureQueue != value)
                {
                    futureQueue = value;
                    OnPropertyChanged(nameof(FutureQueue));
                }
            }
        }

        private int _selectedPredefinedScenario = 0;

        public int SelectedPredefinedScenario
        {
            get { return _selectedPredefinedScenario; }

            set
            {
                if (_selectedPredefinedScenario != value)
                {
                    _selectedPredefinedScenario = value;
                    OnPropertyChanged(nameof(SelectedPredefinedScenario));
                }
            }
        }


        public ProductionStateLoader ScenarioLoader { get; }
        
        public MainWindowViewModel(NaiveController naiveController, ProductionStateLoader scenarioLoader)
        {
            NaiveController = naiveController;
            ProductionState = NaiveController.ProductionState;
            ScenarioLoader = scenarioLoader;
            ScenarioLoader.LoadScenarioFromDisk(ProductionState, 0);
            NextStep = new SimpleCommand(NextStepClickedExecute);
            LoadSelectedScenario = new SimpleCommand(LoadSelectedScenarioExecute);
            UpdateProductionStateInView();
        }

        public void NextStepClickedExecute(object o)
        {
            NaiveController.NextStep();
            UpdateProductionStateInView();
        }

        public void LoadSelectedScenarioExecute(object o)
        {
            ScenarioLoader.LoadScenarioFromMemory(ProductionState, SelectedPredefinedScenario);
            NaiveController.RenewControllerState();
            UpdateProductionStateInView();
        }

        public void NotifyCurrentWarehouseState()
        {
            OnPropertyChanged(nameof(CurrentWarehouseState));
            OnPropertyChanged(nameof(HistoryQueue));
            OnPropertyChanged(nameof(FutureQueue));
        }

        public void UpdateProductionStateInView()
        {
            CurrentWarehouseState = new ObservableCollection<WarehouseItemViewModel>(CreateWarehouseViewModelCollection());
            HistoryQueue = new ObservableCollection<WarehouseItemViewModel>(CreateItemStateCollectionFromQueue(ProductionState.ProductionHistory));
            FutureQueue = new ObservableCollection<WarehouseItemViewModel>(CreateItemStateCollectionFromQueue(ProductionState.FutureProductionPlan));
        }

        public IEnumerable<WarehouseItemViewModel> CreateWarehouseViewModelCollection()
        {
            var whColls = Enumerable.Range(0, ProductionState.WarehouseColls);
            return Enumerable.Range(0, ProductionState.WarehouseRows)
                .SelectMany(row => whColls.Select(col => (row, col)))
                .Select(cell => new WarehouseItemViewModel()
                {
                    Row = cell.row,
                    Col = cell.col,
                    PositionCode = ProductionState.GetWarehouseCell(cell.row, cell.col).ToGuiString(),
                    StateStr = ProductionState.WarehouseState[cell.row, cell.col].ToString(),
                });
        }

        public IEnumerable<WarehouseItemViewModel> CreateItemStateCollectionFromQueue(Queue<ItemState> queue) => queue.ToArray().Select((t, idx) => new WarehouseItemViewModel { StateStr = t.ToString(), PositionCode = idx == 0 ? "Next" : $"Next+{idx}" });
    }

}
