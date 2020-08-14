using System;
using System.Collections.Generic;
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

namespace robot_simulator.Views
{
    /// <summary>
    /// Interakční logika pro QueueViewControl.xaml
    /// </summary>
    public partial class QueueViewControl : UserControl
    {
        public string ControlTitle
        {
            get { return (string)GetValue(ControlTitleProperty); }
            set { SetValue(ControlTitleProperty, value); }
        }

        public static readonly DependencyProperty ControlTitleProperty =
            DependencyProperty.Register("ControlTitle", typeof(string), typeof(QueueViewControl), new PropertyMetadata(string.Empty));



        public int ColumnsCount
        {
            get { return (int)GetValue(ColumnsCountProperty); }
            set { SetValue(ColumnsCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnsCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnsCountProperty =
            DependencyProperty.Register("ColumnsCount", typeof(int), typeof(QueueViewControl), new PropertyMetadata(1));

        public QueueViewControl()
        {
            InitializeComponent();
        }
    }
}
