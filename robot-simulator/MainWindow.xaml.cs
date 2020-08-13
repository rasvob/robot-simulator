using MahApps.Metro.Controls;
using OptimizationLogic;
using OptimizationLogic.DTO;
using robot_simulator.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            var naiveController = new NaiveController(productionState, )
            //var naiveController = new NaiveController()
            //ViewModel = new MainWindowViewModel()

        }
    }
}
