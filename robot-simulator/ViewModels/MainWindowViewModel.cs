using OptimizationLogic;
using OptimizationLogic.AsyncControllers;
using OptimizationLogic.DAL;
using OptimizationLogic.DTO;
using robot_simulator.Dialogs;
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

        public BaseController NaiveController { get; set; }
        public ProductionState ProductionState { get; set; }

        public ICommand NextStep { get; private set; }
        public ICommand LoadSelectedScenario { get; private set; }
        public ICommand SetSelectedOptimizer { get; private set; }
        public ICommand LoadWarehouseState { get; private set; }
        public ICommand LoadFutureProductionPlan { get; private set; }
        public ICommand LoadProductionHistory { get; private set; }
        public ICommand Undo { get; private set; }

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

        public ObservableCollection<WarehouseItemViewModel> FutureQueue 
        {
            get => futureQueue;
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
        private int selectedOptimizer = 0;
        private string notificationText = string.Empty;
        private bool isNofiticationOpen = false;

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

        public int SelectedOptimizer
        {
            get { return selectedOptimizer; }

            set
            {
                if (selectedOptimizer != value)
                {
                    selectedOptimizer = value;
                    OnPropertyChanged(nameof(SelectedOptimizer));
                }
            }
        }

        public int CurrentStep { get => ProductionState.StepCounter; }
        public int NumberOfItemsInProductionQueue { get => ProductionState.FutureProductionPlan.Count; }
        public double TimeSpentInSimulation { get => ProductionState.TimeSpentInSimulation; }
        public int TotalSimulationTime { get => NaiveController.ProductionState.StepCounter * NaiveController.ClockTime; }
        public bool ProductionStateIsOk { get => ProductionState.ProductionStateIsOk; }
        public double CurrentStepTime { get => ProductionState.CurrentStepTime; }
        public double CurrentDelay { get => NaiveController.Delay; }
        public double RealTime { get => NaiveController.RealTime; }
        public double NextItemFromFlow { get => (NaiveController as NaiveAsyncController)?.GetClosestNextIntakeTime() ?? 0; }
        public double NextItemFromQueue { get => (NaiveController as NaiveAsyncController)?.GetClosestNextOuttakeTime() ?? 0; }
        public IEnumerable<string> StepLog { get => NaiveController.StepLog.Select((t, i) => $"#{i}\t{t}").Reverse(); }

        public string NotificationText
        {
            get { return notificationText; }

            set
            {
                if (notificationText != value)
                {
                    notificationText = value;
                    OnPropertyChanged(nameof(NotificationText));
                }
            }
        }

        public bool IsNofiticationOpen
        {
            get { return isNofiticationOpen; }

            set
            {
                if (isNofiticationOpen != value)
                {
                    isNofiticationOpen = value;
                    OnPropertyChanged(nameof(IsNofiticationOpen));
                }
            }
        }

        public void ShowNotification(string message)
        {
            NotificationText = message;
            IsNofiticationOpen = true;
        }

        public ProductionStateLoader ScenarioLoader { get; }
        public OpenFileDialogService OpenFileDialogService { get; }

        public MainWindowViewModel(BaseController naiveController, ProductionStateLoader scenarioLoader, OpenFileDialogService openFileDialogService)
        {
            NaiveController = naiveController;
            ProductionState = NaiveController.ProductionState;
            ScenarioLoader = scenarioLoader;
            ScenarioLoader.LoadScenarioFromDisk(ProductionState, 0);
            NextStep = new SimpleCommand(NextStepClickedExecute, (_) => !ProductionState.SimulationFinished);
            LoadSelectedScenario = new SimpleCommand(LoadSelectedScenarioExecute);
            SetSelectedOptimizer = new SimpleCommand(SetSelectedOptimizerExecute);
            LoadWarehouseState = new SimpleCommand(LoadWarehouseStateExecute);
            LoadFutureProductionPlan = new SimpleCommand(LoadFutureProductionPlanExecute);
            LoadProductionHistory = new SimpleCommand(LoadProductionHistoryExecute);
            Undo = new SimpleCommand(UndoExecute, _ => NaiveController.CanUndo());
            UpdateProductionStateInView();
            OpenFileDialogService = openFileDialogService;
        }

        private void LoadProductionHistoryExecute(object obj)
        {
            LoadCurrentStateFromFile(ProductionState.LoadProductionHistory, "Items returning from flow loaded");
        }

        private void LoadFutureProductionPlanExecute(object obj)
        {
            LoadCurrentStateFromFile(ProductionState.LoadFutureProductionPlan, "Production queue loaded");
        }

        private void LoadWarehouseStateExecute(object obj)
        {
            LoadCurrentStateFromFile(ProductionState.LoadWarehouseState, "Warehouse state loaded");
        }

        private void LoadCurrentStateFromFile(Action<string> loadAction, string message)
        {
            var file = OpenFileDialogService.OpenFile();

            if (file == null)
            {
                ShowNotification("File can't be opened");
                return;
            }

            loadAction(file);
            ShowNotification(message);
            UpdateProductionStateInView();
        }

        private void SetSelectedOptimizerExecute(object obj)
        {
            //TODO: Create optimizer list
            ProductionState.ResetState();
            NaiveController.RenewControllerState();
            return;
        }

        public void UndoExecute(object o)
        {
            NaiveController.Undo();
            ProductionState = NaiveController.ProductionState;
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
            ProductionState.ResetState();
            NaiveController.RenewControllerState();
            UpdateProductionStateInView();
        }

        public void UpdateProductionStateInView()
        {
            CurrentWarehouseState = new ObservableCollection<WarehouseItemViewModel>(CreateWarehouseViewModelCollection());
            HistoryQueue = new ObservableCollection<WarehouseItemViewModel>(CreateItemStateCollectionFromQueue(ProductionState.ProductionHistory));
            FutureQueue = new ObservableCollection<WarehouseItemViewModel>(CreateItemStateCollectionFromQueue(ProductionState.FutureProductionPlan));
            OnPropertyChanged(nameof(CurrentStep));
            OnPropertyChanged(nameof(NumberOfItemsInProductionQueue));
            OnPropertyChanged(nameof(TimeSpentInSimulation));
            OnPropertyChanged(nameof(TotalSimulationTime));
            OnPropertyChanged(nameof(TotalSimulationTime));
            OnPropertyChanged(nameof(ProductionStateIsOk));
            OnPropertyChanged(nameof(CurrentStepTime));
            OnPropertyChanged(nameof(StepLog));
            OnPropertyChanged(nameof(CurrentDelay));
            OnPropertyChanged(nameof(RealTime));
            OnPropertyChanged(nameof(NextItemFromFlow));
            OnPropertyChanged(nameof(NextItemFromQueue));
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
