using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using OptimizationLogic;
using OptimizationLogic.AsyncControllers;
using OptimizationLogic.DAL;
using OptimizationLogic.DTO;
using robot_simulator.Dialogs;
using robot_simulator.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace robot_simulator
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindowViewModel ViewModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            var productionState = new ProductionState(12, 4);
            var scenarioLoader = new ProductionStateLoader(LoadScenarionPaths("InputFiles"), "InputFiles/ProcessingTimeMatrix.csv");
            var naiveController = new NaiveController(productionState);
            BaseController asyncController = new NaiveAsyncControllerWithHalfCycleDelay(productionState);
            GreedyWarehouseReorganizer reorganizer = new GreedyWarehouseReorganizer(new NaiveController(null));
            RealProductionSimulator realProductionSimulator = new RealProductionSimulator(naiveController, null);
            //ViewModel = new MainWindowViewModel(naiveController, scenarioLoader);
            var openFileDialog = new OpenFileDialogService();
            IOpenFileService openFolderDialog = new OpenFolderDialogService();
            ViewModel = new MainWindowViewModel(naiveController, asyncController, reorganizer, realProductionSimulator, scenarioLoader, openFileDialog, openFolderDialog, DialogCoordinator.Instance);
            DataContext = ViewModel;
        }

        private List<ProductionScenarioPaths> LoadScenarionPaths(string folder) =>
            Directory.GetDirectories(folder, "*situation*")
            .Select(Directory.GetFiles)
            .Select(t =>
            {
                return new ProductionScenarioPaths()
                {
                    FutureProductionListCsv = t.FirstOrDefault(s => s.Contains("Future")),
                    HistoricalProductionListCsv = t.FirstOrDefault(s => s.Contains("Historical")),
                    WarehouseInitialStateCsv = t.FirstOrDefault(s => s.Contains("Warehouse"))
                };
            })
            .ToList();
    }
}
