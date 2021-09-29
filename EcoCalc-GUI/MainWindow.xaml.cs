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
using EcoCalc_GUI.Model;

namespace EcoCalc_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowModel Model { get; set; } = new();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = Model;
        }

        private void CraftButton_OnClick(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Model.Craft();
            });
        }
    }
}
