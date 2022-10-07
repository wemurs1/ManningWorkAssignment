using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

namespace find_flows
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private Network MyNetwork = new Network();

        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.DefaultExt = ".net";
                dialog.Filter = "Network Files|*.net|All Files|*.*";

                // Display the dialog.
                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    // Open the network.
                    MyNetwork = new Network(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MyNetwork = new Network();
            }

            // Display the network.
            DrawNetwork();
        }

        private void DrawNetwork()
        {
            // Remove any previous drawing.
            mainCanvas.Children.Clear();

            // Make the network draw itself.
            MyNetwork.Draw(mainCanvas);
        }

        private void ExitCommand_Executed(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void makeTestNetworks_Click(object sender, RoutedEventArgs e)
        {
            BuildGridNetwork("3x3_grid.net", 300, 300, 3, 3);
            BuildGridNetwork("4x4_grid.net", 300, 300, 4, 4);
            BuildGridNetwork("5x8_grid.net", 600, 400, 5, 8);
            BuildGridNetwork("6x10_grid.net", 600, 400, 6, 10);
            BuildGridNetwork("10x15_grid.net", 600, 400, 10, 15);
            BuildGridNetwork("20x30_grid.net", 600, 400, 20, 30);
            MessageBox.Show("Done");
        }

        // Set the network's shortest path algorithm.
        private void algorithmComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem? item = algorithmComboBox.SelectedItem as ComboBoxItem;
            if (item!.Content.ToString() == "Label Setting")
                MyNetwork.AlgorithmType = Network.AlgorithmTypes.LabelSetting;
            else if (item.Content.ToString() == "Label Correcting")
                MyNetwork.AlgorithmType = Network.AlgorithmTypes.LabelCorrecting;
        }
    }
}
