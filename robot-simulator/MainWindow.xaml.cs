using MahApps.Metro.Controls;
using OptimizationLogic;
using OptimizationLogic.DAL;
using OptimizationLogic.DTO;
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
            var productionState = new ProductionState();
            var scenarioLoader = new ProductionStateLoader(LoadScenarionPaths("InputFiles"), "InputFiles/ProcessingTimeMatrix.csv");
            scenarioLoader.LoadScenarion(productionState, 0);
            var naiveController = new NaiveController(productionState);
            ViewModel = new MainWindowViewModel(naiveController);
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
