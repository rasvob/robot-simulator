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
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Documents;
using System.Web.UI;

namespace robot_simulator.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private ObservableCollection<WarehouseItemViewModel> currentWarehouseState;

        public BaseController SelectedController { get; set; }
        public ProductionState ProductionState { get; set; }

        public ICommand NextStep { get; private set; }
        public ICommand LoadSelectedScenario { get; private set; }
        public ICommand SetSelectedOptimizer { get; private set; }
        public ICommand LoadWarehouseState { get; private set; }
        public ICommand LoadFutureProductionPlan { get; private set; }
        public ICommand LoadProductionHistory { get; private set; }
        public ICommand Undo { get; private set; }
        public ICommand LoadProductionState { get; private set; }

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

        private int _selectedOptimizerComboBox = 0;

        public int SelectedOptimizerComboBox
        {
            get => _selectedOptimizerComboBox;

            set
            {
                if (_selectedOptimizerComboBox != value)
                {
                    _selectedOptimizerComboBox = value;
                    OnPropertyChanged(nameof(SelectedOptimizerComboBox));
                }
            }
        }

        private List<int> _frontStackLevelsCollection = new List<int> { 1, 2, 3 };

        public List<int> FrontStackLevelCollection
        {
            get { return _frontStackLevelsCollection; }

            set
            {
                if (_frontStackLevelsCollection != value)
                {
                    _frontStackLevelsCollection = value;
                    OnPropertyChanged(nameof(FrontStackLevelCollection));
                }
            }
        }

        private int _frontStackLevelsCount = 2;

        public int FrontStackLevelsCount
        {
            get { return _frontStackLevelsCount; }

            set
            {
                if (_frontStackLevelsCount != value)
                {
                    _frontStackLevelsCount = value;
                    OnPropertyChanged(nameof(FrontStackLevelsCount));
                }
            }
        }

        private int _numberOfFreePositionsInStacker = 7;

        public int NumberOfFreePositionsInStacker
        {
            get { return _numberOfFreePositionsInStacker; }

            set
            {
                if (_numberOfFreePositionsInStacker != value)
                {
                    _numberOfFreePositionsInStacker = value;
                    OnPropertyChanged(nameof(NumberOfFreePositionsInStacker));
                }
            }
        }

        private int _numberOfItemsInPastProductionQueue = 50;

        public int NumberOfItemsInPastProductionQueue
        {
            get { return _numberOfItemsInPastProductionQueue; }

            set
            {
                if (_numberOfItemsInPastProductionQueue != value)
                {
                    _numberOfItemsInPastProductionQueue = value;
                    OnPropertyChanged(nameof(NumberOfItemsInPastProductionQueue));
                }
            }
        }

        private bool _isMqbDominant = true;

        public bool IsMqbDominant
        {
            get { return _isMqbDominant; }

            set
            {
                if (_isMqbDominant != value)
                {
                    _isMqbDominant = value;
                    OnPropertyChanged(nameof(IsMqbDominant));
                }
            }
        }

        private int _restrictionSelectedIndex = 1;

        public int RestrictionSelectedIndex
        {
            get { return _restrictionSelectedIndex; }

            set
            {
                if (_restrictionSelectedIndex != value)
                {
                    _restrictionSelectedIndex = value;
                    OnPropertyChanged(nameof(RestrictionSelectedIndex));
                }
            }
        }

        private List<string> _productionQueueRestrictions;

        public List<string> ProductionQueueRestrictions
        {
            get { return _productionQueueRestrictions; }

            set
            {
                if (_productionQueueRestrictions != value)
                {
                    _productionQueueRestrictions = value;
                    OnPropertyChanged(nameof(ProductionQueueRestrictions));
                }
            }
        }


        public int CurrentStep { get => ProductionState.StepCounter; }
        public int NumberOfItemsInProductionQueue { get => ProductionState.FutureProductionPlan.Count; }
        public double TimeSpentInSimulation { get => ProductionState.TimeSpentInSimulation; }
        public int TotalSimulationTime { get => SelectedController.ProductionState.StepCounter * SelectedController.ClockTime; }
        public bool ProductionStateIsOk { get => ProductionState.ProductionStateIsOk; }
        public double CurrentStepTime { get => ProductionState.CurrentStepTime; }
        public double CurrentDelay { get => Math.Floor(SelectedController.Delay); }
        public double RealTime { get => SelectedController.RealTime; }
        public double NextItemFromFlow { get => (SelectedController as NaiveAsyncController)?.GetClosestNextIntakeTime() ?? 0; }
        public double NextItemFromQueue { get => (SelectedController as NaiveAsyncController)?.GetClosestNextOuttakeTime() ?? 0; }
        public IEnumerable<string> StepLog { get => SelectedController.StepLog.Select((t, i) => $"#{i}\t{t}").Reverse(); }

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

        public BaseController NaiveController { get; }
        public BaseController AsyncController { get; }
        public GreedyWarehouseReorganizer Reorganizer { get; }
        public RealProductionSimulator RealProductionSimulator { get; }
        public IOpenFileService OpenFolderDialog { get; }
        public IDialogCoordinator DialogCoordinator { get; }
        public ProgressDialogController ProgressDialog { get; set; }

        public bool WarehouserReorganizationIsRunning { get; set; } = false;

        public MainWindowViewModel(BaseController naiveController, BaseController asyncController, GreedyWarehouseReorganizer reorganizer, RealProductionSimulator realProductionSimulator, ProductionStateLoader scenarioLoader, OpenFileDialogService openFileDialogService, IOpenFileService openFolderDialog, MahApps.Metro.Controls.Dialogs.IDialogCoordinator dialogCoordinator)
        {
            NaiveController = naiveController;
            AsyncController = asyncController;
            Reorganizer = reorganizer;

            SelectedController = naiveController;
            ProductionState = SelectedController.ProductionState;
            ScenarioLoader = scenarioLoader;
            ScenarioLoader.LoadScenarioFromDisk(ProductionState, 0);
            NextStep = new SimpleCommand(NextStepClickedExecuteAsync, (_) => !ProductionState.SimulationFinished && !WarehouserReorganizationIsRunning);
            LoadSelectedScenario = new SimpleCommand(LoadSelectedScenarioExecute);
            SetSelectedOptimizer = new SimpleCommand(SetSelectedOptimizerExecute);
            LoadWarehouseState = new SimpleCommand(LoadWarehouseStateExecute);
            LoadFutureProductionPlan = new SimpleCommand(LoadFutureProductionPlanExecute);
            LoadProductionHistory = new SimpleCommand(LoadProductionHistoryExecute);
            LoadProductionState = new SimpleCommand(LoadProductionStateExecute);
            Undo = new SimpleCommand(UndoExecute, _ => SelectedController.CanUndo());
            UpdateProductionStateInView();
            OpenFileDialogService = openFileDialogService;
            RealProductionSimulator = realProductionSimulator;
            OpenFolderDialog = openFolderDialog;
            RealProductionSimulator.WarehouseReorganizationProgressUpdated += RealProductionSimulator_WarehouseReorganizationProgressUpdated;
            DialogCoordinator = dialogCoordinator;

            //TODO: Add item-to-command for dominant type change
            ProductionQueueRestrictions = CreateProductionQueueRestrictions();
        }

        private List<string> CreateProductionQueueRestrictions(int maximumOfNonDominantInRow = 5)
        {
            ItemState nonDominant = !IsMqbDominant ? ItemState.MQB : ItemState.MEB;
            var res = new List<string> { "Full production of dominant type" };
            res.AddRange(Enumerable.Range(1, maximumOfNonDominantInRow).Select(t => $"Maximum of {t} {nonDominant} items in a row"));
            return res;
        }

        private async void RealProductionSimulator_WarehouseReorganizationProgressUpdated(object sender, ProgressEventArgs e)
        {
            switch (e.State)
            {
                case ProgressState.Start:
                    WarehouserReorganizationIsRunning = true;
                    ProgressDialog = await DialogCoordinator.ShowProgressAsync(this, $"Warehouse reorganization running", "Please wait...", false);
                    ProgressDialog.Minimum = 0;
                    ProgressDialog.Maximum = e.CurrentValue;
                    break;
                case ProgressState.End:
                    if (ProgressDialog?.IsOpen == true)
                    {
                        await ProgressDialog?.CloseAsync();
                    }
                    WarehouserReorganizationIsRunning = false;
                    break;
                case ProgressState.Update:
                    ProgressDialog?.SetMessage($"Step {e.CurrentValue} out of {ProgressDialog?.Maximum}");
                    ProgressDialog?.SetProgress(e.CurrentValue);
                    break;
                default:
                    if (ProgressDialog?.IsOpen == true)
                    {
                        await ProgressDialog?.CloseAsync();
                    }
                    WarehouserReorganizationIsRunning = false;
                    break;
            }
        }

        private void LoadProductionStateExecute(object obj)
        {
            try
            {
                string folder = OpenFolderDialog.Open();
                if (string.IsNullOrEmpty(folder))
                {
                    ShowNotification("Folder does not exist");
                    return;
                }
                ScenarioLoader.LoadScenarioFromFolder(ProductionState, folder);
                UpdateProductionStateInView();
                ShowNotification("Scenario loaded");
            }
            catch (ArgumentException exc)
            {
                ShowNotification(exc.Message);
            }
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
            var file = OpenFileDialogService.Open();

            if (file == null)
            {
                ShowNotification("File can't be opened");
                return;
            }

            try
            {
                loadAction(file);
                ShowNotification(message);
                UpdateProductionStateInView();
            }
            catch (Exception exc)
            {
                ShowNotification(exc.Message);
            }
        }

        private void SetSelectedOptimizerExecute(object obj)
        {
            switch (SelectedOptimizerComboBox)
            {
                case 0:
                    RealProductionSimulator.Controller = NaiveController;
                    RealProductionSimulator.WarehouseReorganizer = null;
                    break;
                case 1:
                    RealProductionSimulator.Controller = NaiveController;
                    RealProductionSimulator.WarehouseReorganizer = Reorganizer;
                    break;
                case 2:
                    RealProductionSimulator.Controller = AsyncController;
                    RealProductionSimulator.WarehouseReorganizer = null;
                    break;
                case 3:
                    RealProductionSimulator.Controller = AsyncController;
                    RealProductionSimulator.WarehouseReorganizer = Reorganizer;
                    break;
                default:
                    RealProductionSimulator.Controller = NaiveController;
                    RealProductionSimulator.WarehouseReorganizer = null;
                    break;
            }
            RealProductionSimulator.Controller.ProductionState = ProductionState;
            SelectedController = RealProductionSimulator.Controller;
            ProductionState.ResetState();
            SelectedController.RenewControllerState();
            UpdateProductionStateInView();
            SelectedOptimizer = SelectedOptimizerComboBox;
        }

        public void UndoExecute(object o)
        {
            SelectedController.Undo();
            ProductionState = SelectedController.ProductionState;
            UpdateProductionStateInView();
        }

        public async void NextStepClickedExecuteAsync(object o)
        {
            await Task.Factory.StartNew(() => RealProductionSimulator.NextStep());
            UpdateProductionStateInView();
        }

        public void LoadSelectedScenarioExecute(object o)
        {
            ScenarioLoader.LoadScenarioFromMemory(ProductionState, SelectedPredefinedScenario);
            ProductionState.ResetState();
            SelectedController.RenewControllerState();
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
            var res = Enumerable.Range(0, ProductionState.WarehouseRows)
                .SelectMany(row => whColls.Select(col => (row, col)))
                .Select(cell => new WarehouseItemViewModel()
                {
                    Row = cell.row,
                    Col = cell.col,
                    PositionCode = ProductionState.GetWarehouseCell(cell.row, cell.col).ToGuiString(),
                    StateStr = ProductionState.WarehouseState[cell.row, cell.col].ToString(),
                }).ToList();
            res.InsertRange(24, Enumerable.Range(0, ProductionState.WarehouseColls).Select(cell => new WarehouseItemViewModel()
                {
                    Row = 0,
                    Col = 0,
                    PositionCode = string.Empty,
                    StateStr = "Shuttle",
                }));
            return res;
        }

        public IEnumerable<WarehouseItemViewModel> CreateItemStateCollectionFromQueue(Queue<ItemState> queue) => queue.ToArray().Select((t, idx) => new WarehouseItemViewModel { StateStr = t.ToString(), PositionCode = idx == 0 ? "Next" : $"Next+{idx}" });
    }
}
