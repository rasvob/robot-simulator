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
using System.ComponentModel;
using OptimizationLogic.BatchSimulation;
using System.Threading;
using System.Diagnostics;

namespace robot_simulator.ViewModels
{
    public class MainWindowViewModel : BaseViewModel, IDataErrorInfo
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
        public ICommand RunSimulations { get; private set; }
        public ICommand LoadSelectedSimulation { get; private set; }
        public ICommand SaveSelectedSimulation { get; private set; }
        public ICommand SetIntervalLengthConfiguration { get; private set; }

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

        private int _frontStackColumnsCount = 13;

        public int FrontStackColumnsCount
        {
            get { return _frontStackColumnsCount; }

            set
            {
                if (_frontStackColumnsCount != value)
                {
                    _frontStackColumnsCount = value;
                    OnPropertyChanged(nameof(FrontStackColumnsCount));
                }
            }
        }


        private int _numberOfFreePositionsInStacker = 9;

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

        private int _numberOfItemsInPastProductionQueue = 63;

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

        private ObservableCollection<ObservableString> _productionQueueRestrictions;

        public ObservableCollection<ObservableString> ProductionQueueRestrictions
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

        private bool _areSimulationSequencesRandom;

        public bool AreSimulationSequencesRandom
        {
            get { return _areSimulationSequencesRandom; }

            set
            {
                if (_areSimulationSequencesRandom != value)
                {
                    _areSimulationSequencesRandom = value;
                    OnPropertyChanged(nameof(AreSimulationSequencesRandom));
                }
            }
        }

        private int _numberOfSimulations = 10;

        public int NumberOfSimulations
        {
            get { return _numberOfSimulations; }

            set
            {
                if (_numberOfSimulations != value)
                {
                    _numberOfSimulations = value;
                    OnPropertyChanged(nameof(NumberOfSimulations));
                }
            }
        }

        private int _numberOfItemsInFutureProductionQueue = 100;

        public int NumberOfItemsInFutureProductionQueue
        {
            get { return _numberOfItemsInFutureProductionQueue; }

            set
            {
                if (_numberOfItemsInFutureProductionQueue != value)
                {
                    _numberOfItemsInFutureProductionQueue = value;
                    OnPropertyChanged(nameof(NumberOfItemsInFutureProductionQueue));
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

        private bool _areCoefficientValuesFixed = false;

        public bool AreCoefficientValuesFixed
        {
            get { return _areCoefficientValuesFixed; }

            set
            {
                if (_areCoefficientValuesFixed != value)
                {
                    _areCoefficientValuesFixed = value;
                    OnPropertyChanged(nameof(AreCoefficientValuesFixed));
                }
            }
        }

        private double _nextNonDominantItemProbability = 0.5;

        public double NextNonDominantItemProbability
        {
            get { return _nextNonDominantItemProbability; }

            set
            {
                if (_nextNonDominantItemProbability != value)
                {
                    _nextNonDominantItemProbability = value;
                    OnPropertyChanged(nameof(NextNonDominantItemProbability));
                }
            }
        }

        private double _dominantDistanceWeight = 1.0;

        public double DominantDistanceWeight
        {
            get { return _dominantDistanceWeight; }

            set
            {
                if (_dominantDistanceWeight != value)
                {
                    _dominantDistanceWeight = value;
                    OnPropertyChanged(nameof(DominantDistanceWeight));
                }
            }
        }

        private double _nonDominantDistanceWeight = 1.0;

        public double NonDominantDistanceWeight
        {
            get { return _nonDominantDistanceWeight; }

            set
            {
                if (_nonDominantDistanceWeight != value)
                {
                    _nonDominantDistanceWeight = value;
                    OnPropertyChanged(nameof(NonDominantDistanceWeight));
                }
            }
        }

        private double _uniformProbabilityWeight = 1.0;
        private List<SimulationResultModel> _simulationResults;
        private SingleSimulationResult _selectedSimulationResult;

        public double UniformProbabilityWeight
        {
            get { return _uniformProbabilityWeight; }

            set
            {
                if (_uniformProbabilityWeight != value)
                {
                    _uniformProbabilityWeight = value;
                    OnPropertyChanged(nameof(UniformProbabilityWeight));
                }
            }
        }

        public List<SimulationResultModel> SimulationResults
        {
            get { return _simulationResults; }

            set
            {
                if (_simulationResults != value)
                {
                    _simulationResults = value;
                    OnPropertyChanged(nameof(SimulationResults));
                }
            }
        }

        public SingleSimulationResult SelectedSimulationResult
        {
            get { return _selectedSimulationResult; }

            set
            {
                if (_selectedSimulationResult != value)
                {
                    _selectedSimulationResult = value;
                    OnPropertyChanged(nameof(SelectedSimulationResult));
                }
            }
        }

        private int _tiimeLimit = 36;
        private int _clockTime = 55;
        private bool _useWarehouseReorganization = false;
        private ExperimentResults _experimentResults;
        private int _selectedTabIndex = 0;
        private bool _scrollToTop;
        private int _reorganizationComplexityLevel = 2;

        public int TimeLimit
        {
            get { return _tiimeLimit; }

            set
            {
                if (_tiimeLimit != value)
                {
                    _tiimeLimit = value;
                    OnPropertyChanged(nameof(TimeLimit));
                }
            }
        }

        public int ClockTime
        {
            get { return _clockTime; }

            set
            {
                if (_clockTime != value)
                {
                    _clockTime = value;
                    OnPropertyChanged(nameof(ClockTime));
                }
            }
        }

        public bool UseWarehouseReorganization
        {
            get { return _useWarehouseReorganization; }

            set
            {
                if (_useWarehouseReorganization != value)
                {
                    _useWarehouseReorganization = value;
                    OnPropertyChanged(nameof(UseWarehouseReorganization));
                }
            }
        }

        public ExperimentResults ExperimentResults
        {
            get { return _experimentResults; }

            set
            {
                if (_experimentResults != value)
                {
                    _experimentResults = value;
                    OnPropertyChanged(nameof(ExperimentResults));
                }
            }
        }

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }

            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    OnPropertyChanged(nameof(SelectedTabIndex));
                }
            }
        }

        public bool ScrollToTop
        {
            get { return _scrollToTop; }

            set
            {
                _scrollToTop = value;
                OnPropertyChanged(nameof(ScrollToTop));
            }
        }

        public int ReorganizationComplexityLevel
        {
            get { return _reorganizationComplexityLevel; }

            set
            {
                if (_reorganizationComplexityLevel != value)
                {
                    _reorganizationComplexityLevel = value;
                    OnPropertyChanged(nameof(ReorganizationComplexityLevel));
                }
            }
        }

        public List<ProductionState> ProductionStatesFromConfig { get; set; }

        public int NumberOfDominantItems { get => ProductionState.ComputeDominantTypeItemsCount(NumberOfItemsInPastProductionQueue); }
        public int NumberOfNonDominantItems { get => ProductionState.ComputeNonDominantTypeItemsCount(NumberOfItemsInPastProductionQueue, RestrictionSelectedIndex); }
        public string DominantItemString { get => IsMqbDominant ? "MQB" : "MEB"; }
        public string NonDominantItemString { get => !IsMqbDominant ? "MQB" : "MEB"; }

        public string NumberOfDominantItemsHeader { get => $"Number of {DominantItemString} items"; }
        public string NumberOfNonDominantItemsHeader { get => $"Number of {NonDominantItemString} items"; }
        public int WarehouseRows { get => ProductionState.WarehouseRows; }

        public IEnumerable<WarehouseItemViewModel> ExampleOfGeneratedSequence {
            get {
                int resLen = 12;
                var res = new List<WarehouseItemViewModel>();

                if (RestrictionSelectedIndex > 0)
                {
                    while (res.Count < resLen)
                    {
                        res.Add(new WarehouseItemViewModel { StateStr = NonDominantItemString });
                        res.AddRange(Enumerable.Repeat(new WarehouseItemViewModel { StateStr = DominantItemString }, RestrictionSelectedIndex));
                    }
                }
                else
                {
                    res.AddRange(Enumerable.Repeat(new WarehouseItemViewModel { StateStr = NonDominantItemString }, resLen));
                }
                

                for (int i = 0; i < res.Count; i++)
                {
                    res[i].PositionCode = $"#{i + 1}";
                }

                return res.Take(resLen);
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
        public bool SimulatioanIsRunning { get; set; } = false;

        public string Error => string.Empty;

        public string this[string columnName] => columnName switch
        {
            nameof(NumberOfFreePositionsInStacker) => NumberOfFreePositionsInStacker < 1 ? "Number of free positions in stacker has to be >= 1" : null,
            nameof(NumberOfItemsInPastProductionQueue) => NumberOfItemsInPastProductionQueue < 5 ? "Number of items in pas production queue has to be >= 5" : null,
            nameof(NextNonDominantItemProbability) => NextNonDominantItemProbability < 0 || NextNonDominantItemProbability > 1 ? "Probability must be in range from 0 to 1" : null,
            nameof(UniformProbabilityWeight) => UniformProbabilityWeight < 0  ? "Uniform placing probability weight must be >= 0" : null,
            nameof(ClockTime) => ClockTime < 10 || TimeLimit >= ClockTime ? $"Time for one operation cycle must be >= 10 and more than Intake to outtake cycle length ({TimeLimit})" : null,
            nameof(TimeLimit) => TimeLimit < 10 || TimeLimit >= ClockTime ? $"Intake to outtake cycle length must be >= 10 and less than Time for one operation cycle lenght ({ClockTime})" : null,
            _ => null
        };

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
            RunSimulations = new SimpleCommand(RunSimulationsExecuteAsync, RunSimulationsCanExecute);
            LoadSelectedSimulation = new SimpleCommand(LoadSelectedSimulationExecute);
            SaveSelectedSimulation = new SimpleCommand(SaveSelectedSimulationExecute);
            SetIntervalLengthConfiguration = new SimpleCommand(SetIntervalLengthConfigurationExecute, SetIntervalLengthConfigurationCanExecute);
            UpdateProductionStateInView();
            OpenFileDialogService = openFileDialogService;
            RealProductionSimulator = realProductionSimulator;
            OpenFolderDialog = openFolderDialog;
            RealProductionSimulator.WarehouseReorganizationProgressUpdated += RealProductionSimulator_WarehouseReorganizationProgressUpdated;
            DialogCoordinator = dialogCoordinator;
            ProductionQueueRestrictions = new ObservableCollection<ObservableString>();
            UpdateProductionQueueRestrictions();
            SimulationResults = new List<SimulationResultModel>() { new SimulationResultModel { Delay = 0, NumberOfNonProducedCars = 0, SimulationNumber = 0 }, new SimulationResultModel { Delay = 10, NumberOfNonProducedCars = 10, SimulationNumber = 1 } };
            this.PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        private void SaveSelectedSimulationExecute(object obj)
        {
            int idx = SelectedSimulationResult.SimulationNumber;
            var prodState = ProductionStatesFromConfig[idx];

            try
            {
                string folder = OpenFolderDialog.Open();
                if (string.IsNullOrEmpty(folder))
                {
                    ShowNotification("Folder does not exist");
                    return;
                }
                prodState.SaveProductionState(folder, $"_{idx}");
                ShowNotification("Scenario saved");
            }
            catch (ArgumentException exc)
            {
                ShowNotification(exc.Message);
            }

        }

        private bool RunSimulationsCanExecute(object arg)
        {
            return SetIntervalLengthConfigurationCanExecute(arg);
        }

        private bool SetIntervalLengthConfigurationCanExecute(object arg)
        {
            return this[nameof(TimeLimit)] == null && this[nameof(ClockTime)] == null;
        }

        private void SetIntervalLengthConfigurationExecute(object obj)
        {
            SelectedController.ClockTime = ClockTime;
            SelectedController.TimeLimit = TimeLimit;
            Reorganizer.Controller.SetControllerTimes(ClockTime, TimeLimit);
        }

        private void LoadSelectedSimulationExecute(object obj)
        {
            int idx = SelectedSimulationResult.SimulationNumber;
            ProductionState = (ProductionState)ProductionStatesFromConfig[idx].Clone();
            ProductionState.ResetState();
            SelectedController.ProductionState = ProductionState;
            SelectedController.RenewControllerState();
            UpdateProductionStateInView();
            ScrollToTop = true;
            SelectedTabIndex = 0;
            ShowNotification("Scenario loaded");
        }

        private async void RunSimulationsExecuteAsync(object obj)
        {
            ExperimentResults = null;
            var runner = new ExperimentRunner();
            var factory = new ExperimentFactory
            {
                BatchSize = NumberOfSimulations,
                ClockTime = ClockTime,
                DominantDistanceWeight = DominantDistanceWeight,
                IsMqbDominant = IsMqbDominant,
                MaximumNonDominantItemsInARow = RestrictionSelectedIndex,
                NonDominantDistanceWeight = NonDominantDistanceWeight,
                NumberOfFreePositions = NumberOfFreePositionsInStacker,
                NumberOfItemsInFutureProductionQueue = NumberOfItemsInFutureProductionQueue,
                NumberOfItemsInPastProductionQueue = NumberOfItemsInPastProductionQueue,
                ProbabiityOfNonDominantItemAsNextOne = NextNonDominantItemProbability,
                SequencesAreDeterministic = !AreSimulationSequencesRandom,
                TimeLimit = TimeLimit,
                UniformProbabilityWeight = UniformProbabilityWeight,
                UseFixedCoefficient = AreCoefficientValuesFixed,
                UseReorganization = UseWarehouseReorganization,
                WarehouseColumns = FrontStackColumnsCount,
                WarehouseRows = FrontStackLevelsCount * 2,
                ReorganizationComplexityLevel = ReorganizationComplexityLevel
            };

            CancellationTokenSource cts = new CancellationTokenSource();
            runner.CancellationToken = cts.Token;
            factory.CancellationToken = cts.Token;

            factory.ProgressUpdated += (sender, args) =>
            {
                if (args.State == ProgressState.Update)
                {
                    ProgressDialog?.SetMessage($"Generated {args.CurrentValue} experiment parameters out of {ProgressDialog?.Maximum}");
                    ProgressDialog?.SetProgress(args.CurrentValue);
                }
            };

            SimulatioanIsRunning = true;
            ProgressDialog = await DialogCoordinator.ShowProgressAsync(this, "Simulation is running", "Please wait...", true);
            ProgressDialog.Minimum = 0;
            ProgressDialog.Maximum = NumberOfSimulations;
            ProgressDialog.Canceled += (s, e) => cts.Cancel();

            runner.CounterUpdated += async (sender, e) =>
            {
                switch (e.State)
                {
                    case ProgressState.Start:
                        break;
                    case ProgressState.End:
                        break;
                    case ProgressState.Update:
                        ProgressDialog?.SetMessage($"Completed {e.CurrentValue} simulations out of {ProgressDialog?.Maximum}");
                        ProgressDialog?.SetProgress(e.CurrentValue);
                        break;
                }
            };

            try
            {
                ExperimentResults = await Task.Factory.StartNew(() => {
                    ProgressDialog?.SetMessage($"Creating experiments configuration");
                    runner.Config = factory.CreateExperimentConfig();
                    ProductionStatesFromConfig = runner.Config.ProductionStatesBackup;
                    return runner.RunExperiments();
                });
            }
            catch (Exception exc) when (exc is AggregateException || exc is OperationCanceledException)
            {
                Debug.WriteLine(exc.Message);
            }
            finally
            {
                if (ProgressDialog?.IsOpen == true)
                {
                    await ProgressDialog?.CloseAsync();
                }
                SimulatioanIsRunning = false;
            }
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var warehouseSizeMasterProperties = new List<string>() { nameof(FrontStackLevelsCount), nameof(NumberOfItemsInPastProductionQueue), nameof(NumberOfFreePositionsInStacker), nameof(RestrictionSelectedIndex) };
            if (warehouseSizeMasterProperties.Contains(e.PropertyName))
            {
                FrontStackColumnsCount = ProductionState.ComputeNeededColumnsInWarehouse(FrontStackLevelsCount * 2, NumberOfDominantItems, NumberOfNonDominantItems, NumberOfFreePositionsInStacker, NumberOfItemsInPastProductionQueue);
                OnPropertyChanged(nameof(NumberOfDominantItems));
                OnPropertyChanged(nameof(NumberOfNonDominantItems));
                OnPropertyChanged(nameof(ExampleOfGeneratedSequence));
            }
            else if (e.PropertyName == nameof(IsMqbDominant))
            {
                UpdateProductionQueueRestrictions();
                OnPropertyChanged(nameof(NumberOfDominantItems));
                OnPropertyChanged(nameof(NumberOfNonDominantItems));
                OnPropertyChanged(nameof(NumberOfDominantItemsHeader));
                OnPropertyChanged(nameof(NumberOfNonDominantItemsHeader));
                OnPropertyChanged(nameof(ExampleOfGeneratedSequence));
            }
        }

        private void UpdateProductionQueueRestrictions(int maximumOfNonDominantInRow = 5)
        {
            ItemState nonDominant = !IsMqbDominant ? ItemState.MQB : ItemState.MEB;
            ItemState dominant = IsMqbDominant ? ItemState.MQB : ItemState.MEB;

            if (ProductionQueueRestrictions.Any())
            {
                
                for (int i = 1; i < maximumOfNonDominantInRow; i++)
                {
                    ProductionQueueRestrictions[i].Value = $"Minimum of {i} {dominant} items between two {nonDominant}";
                }
                return;
            }

            ProductionQueueRestrictions.Clear();
            ProductionQueueRestrictions.Add(new ObservableString() { Value = "Full production of non-dominant type" });
            foreach (var item in Enumerable.Range(1, maximumOfNonDominantInRow).Select(t => $"Minimum of {t} {dominant} items between two {nonDominant}"))
            {
                ProductionQueueRestrictions.Add(new ObservableString() { Value = item });
            }
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
            Reorganizer.MaxDepth = ReorganizationComplexityLevel;
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
            ProductionState = ScenarioLoader.LoadScenarioFromMemory(SelectedPredefinedScenario);
            ProductionState.ResetState();
            SelectedController.ProductionState = ProductionState;
            SelectedController.RenewControllerState();
            UpdateProductionStateInView();

            //ScenarioLoader.LoadScenarioFromMemory(ProductionState, SelectedPredefinedScenario);
            //ProductionState.ResetState();
            //SelectedController.RenewControllerState();
            //UpdateProductionStateInView();
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
            OnPropertyChanged(nameof(WarehouseRows));
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
            res.InsertRange(ProductionState.WarehouseRows/2 * ProductionState.WarehouseColls, Enumerable.Range(0, ProductionState.WarehouseColls).Select(_ => new WarehouseItemViewModel()
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
