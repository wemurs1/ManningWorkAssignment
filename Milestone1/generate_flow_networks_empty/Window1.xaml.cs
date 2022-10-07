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

namespace generate_flow_networks
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

        private Network MyNetwork = null;

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

        private void BuildGridNetwork(string filename,
            int width, int height, int num_rows, int num_cols)
        {
            const double margin = 20;
            double col_wid = (width - 2 * margin) / (num_cols - 1);
            double row_hgt = (height - 2 * margin) / (num_rows - 1);

            // Make the network.
            Network network = new Network();

            // Make the nodes.
            List<List<Node>> node_list = new List<List<Node>>();
            for (int r = 0; r < num_rows; r++)
            {
                List<Node> new_nodes = new List<Node>();
                node_list.Add(new_nodes);
                for (int c = 0; c < num_cols; c++)
                {
                    double x = margin + c * col_wid;
                    double y = margin + r * row_hgt;
                    string text = network.Nodes.Count.ToString();
                    Node node = new Node(network, new Point(x, y), text);
                    new_nodes.Add(node);
                }
            }

            // Make the horizontal links.
            for (int r = 0; r < num_rows; r++)
            {
                for (int c = 0; c < num_cols - 1; c++)
                {
                    MakeRandomizedLink(network, node_list[r][c], node_list[r][c + 1]);
                }
            }

            // Make the horizontal links.
            for (int r = 0; r < num_rows - 1; r++)
            {
                for (int c = 0; c < num_cols; c++)
                {
                    MakeRandomizedLink(network, node_list[r][c], node_list[r + 1][c]);
                }
            }

            // Save the network into the file.
            network.SaveIntoFile(filename);
        }

        // Make links between the nodes with lengths equal to the distance
        // between them times a random amount between 1.0 and 1.2.
        Random Rand = new Random();
        private void MakeRandomizedLink(Network network,
            Node node1, Node node2)
        {
            double dist = Distance(node1.Center, node2.Center);

            double cost12 = Math.Round(dist * Rand.Next(1.0, 1.2));
            Link link12 = new Link(network, node1, node2, cost12);

            double cost21 = Math.Round(dist * Rand.Next(1.0, 1.2));
            Link link21 = new Link(network, node2, node1, cost21);
        }

        // Return the distance between two points.
        private double Distance(Point point1, Point point2)
        {
            double dx = point1.X - point2.X;
            double dy = point1.Y - point2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        // Generate some test networks.
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
    }
}
