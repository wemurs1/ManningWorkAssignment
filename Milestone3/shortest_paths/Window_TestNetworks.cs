using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

namespace shortest_paths
{
    public partial class Window1
    {
        // Used to generate random link costs.
        private Random Rand = new Random();

        // Create a randomized grid network,
        // save it into a file, and return it.
        private Network BuildGridNetwork(string filename,
            double width, double height, int numRows, int numCols)
        {
            const double MARGIN = 20;

            double colWid = (width - 2 * MARGIN) / (numCols - 1);
            double rowHgt = (height - 2 * MARGIN) / (numRows - 1);

            // Make the network.
            Network network = new Network();

            // Make the nodes.
            Node[,] nodes = new Node[numRows, numCols];
            int index = 1;
            for (int r = 0; r < numRows; r++)
            {
                for (int c = 0; c < numCols; c++)
                {
                    double x = MARGIN + c * colWid;
                    double y = MARGIN + r * rowHgt;
                    nodes[r, c] = new Node(network, new Point(x, y), index.ToString());
                    index++;
                }
            }

            // Make the horizontal links.
            // This assumes that each link has a reversed
            // link with a differently randomized cost.
            for (int r = 0; r < numRows; r++)
            {
                for (int c = 0; c < numCols - 1; c++)
                {
                    MakeRandomizedLink(network, nodes[r, c], nodes[r, c + 1]);
                }
            }

            // Make the horizontal links.
            for (int r = 0; r < numRows - 1; r++)
            {
                for (int c = 0; c < numCols; c++)
                {
                    MakeRandomizedLink(network, nodes[r, c], nodes[r + 1, c]);
                }
            }

            // Save the network into the file.
            network.SaveIntoFile(filename);
            return network;
        }

        // Make links between the nodes with lengths equal to the distance
        // between them times a random amount between 1.0 and 1.2.
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
    }
}
