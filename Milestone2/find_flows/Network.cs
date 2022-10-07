using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media;

namespace find_flows
{
    internal class Network
    {
        internal Node? StartNode = null;
        internal Node? EndNode = null;
        internal List<Node> Nodes = new List<Node>();
        internal List<Link> Links = new List<Link>();

        internal Network()
        {
            Clear();
        }

        // Load a network from a file.
        internal Network(string filename)
        {
            ReadFromFile(filename);
        }

        internal void Clear()
        {
            StartNode = null;
            EndNode = null;
            Nodes = new List<Node>();
            Links = new List<Link>();
        }

        internal void AddNode(Node node)
        {
            node.Index = Nodes.Count;
            Nodes.Add(node);
        }

        internal void AddLink(Link link)
        {
            Links.Add(link);
        }

        // Return the network's serialization.
        internal string Serialization()
        {
            // Set the node indices.
            for (int i = 0; i < Nodes.Count; i++) Nodes[i].Index = i;

            // Save the node and link counts.
            StringWriter sw = new StringWriter();
            sw.WriteLine(string.Format("{0} # Num nodes.", Nodes.Count));
            sw.WriteLine(string.Format("{0} # Num links.", Links.Count));
            sw.WriteLine();

            // Save the nodes.
            sw.WriteLine("# Nodes.");
            foreach (Node node in Nodes)
            {
                sw.WriteLine(string.Format("{0},{1},{2}",
                    node.Center.X, node.Center.Y, node.Text));
            }
            sw.WriteLine();

            // Save the links.
            sw.WriteLine("# Links.");
            foreach (Link link in Links)
            {
                sw.WriteLine(string.Format("{0},{1},{2}",
                    link.FromNode.Index, link.ToNode.Index, link.Cost));
            }

            return sw.ToString();
        }

        // Write the network into a file.
        internal void SaveIntoFile(string filename)
        {
            File.WriteAllText(filename, Serialization());
        }

        // Initialize the network from a serialization.
        internal void Deserialize(string serialization)
        {
            Clear();

            // Get a stream to read the serialization one line at a time.
            using (StringReader reader = new StringReader(serialization))
            {
                // Get the number of nodes and links.
                int numNodes = int.Parse(ReadNextLine(reader)!);
                int numLinks = int.Parse(ReadNextLine(reader)!);

                // Read the nodes.
                for (int i = 0; i < numNodes; i++)
                {
                    // Read the next node's values.
                    string[] fields = ReadNextLine(reader)!.Split(',');
                    double x = double.Parse(fields[0]);
                    double y = double.Parse(fields[1]);
                    string text = fields[2].Trim();

                    // Make the node. (This adds the node to the network.)
                    Node node = new Node(this, new Point(x, y), text);
                }

                // Read the links.
                for (int i = 0; i < numLinks; i++)
                {
                    // Read the next link's values.
                    string[] fields = ReadNextLine(reader)!.Split(',');
                    int index1 = int.Parse(fields[0]);
                    int index2 = int.Parse(fields[1]);
                    double cost = double.Parse(fields[2]);

                    // Make the link. (This adds the link to the network.)
                    Link link = new Link(this, Nodes[index1], Nodes[index2], cost);
                }
            }
        }

        // Read the next non-blank line from the serialization.
        private string? ReadNextLine(StringReader reader)
        {
            // Repeat until we get a line or reach the end.
            string? line = reader.ReadLine();
            while(line != null)
            {
                // Trim comments.
                line = line.Split('#')[0];
                line = line.Trim();

                // If the line is non-blank, return it.
                if (line.Length > 0) return line;
                line = reader.ReadLine();
            }
            // If we've reached the end of the stream, return null.
            return null;
        }

        internal void ReadFromFile(string filename)
        {
            Deserialize(File.ReadAllText(filename));
        }

        internal void Draw(Canvas canvas)
        {
            // Size the canvas.
            const double MARGIN = 20;
            Rect bounds = GetBounds();
            canvas.Width = bounds.Right + MARGIN;
            canvas.Height = bounds.Bottom + MARGIN;

            // Draw the links.
            foreach (Link link in Links) link.Draw(canvas);

            // See if we should draw labels.
            bool drawLabels = (Nodes.Count < 100);

            // Label the links.
            if (drawLabels)
                foreach (Link link in Links)
                    link.DrawLabel(canvas);

            // Draw and label the nodes.
            foreach (Node node in Nodes) node.Draw(canvas, drawLabels);
        }

        // Get the bounds for the network.
        internal Rect GetBounds()
        {
            if (Nodes.Count < 1) return new Rect(0, 0, 1, 1);

            double xmin = double.PositiveInfinity;
            double xmax = double.NegativeInfinity;
            double ymin = double.PositiveInfinity;
            double ymax = double.NegativeInfinity;
            foreach (Node node in Nodes)
            {
                xmin = Math.Min(xmin, node.Center.X);
                xmax = Math.Max(xmax, node.Center.X);
                ymin = Math.Min(ymin, node.Center.Y);
                ymax = Math.Max(ymax, node.Center.Y);
            }

            // Make sure width and height are at least 1.
            double width = xmax - xmin;
            if (width < 1) width = 1;
            double height = ymax - ymin;
            if (height < 1) height = 1;
            return new Rect(xmin, ymin, width, height);
        }

        // Respond to node clicks.
        internal void ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse? ellipse = sender as Ellipse;
            Node? node = ellipse!.Tag as Node;
            NodeClicked(node!, e);
        }
        internal void label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label? label = sender as Label;
            Node? node = label!.Tag as Node;
            NodeClicked(node!, e);
        }
        private void NodeClicked(Node node, MouseButtonEventArgs e)
        {
            // Update the start/end node.
            if (e.ChangedButton == MouseButton.Left)
            {
                // Update the start node.
                // Deselect the previous start node.
                if (StartNode != null) StartNode.IsStartNode = false;

                // Select the new start node.
                StartNode = node;
                StartNode.IsStartNode = true;
            }
            else
            {
                // Update the end node.
                // Deselect the previous end node.
                if (EndNode != null) EndNode.IsEndNode = false;

                // Select the new end node.
                EndNode = node;
                EndNode.IsEndNode = true;
            }

            // Check for shortest paths.
            CheckForPath();
        }

        #region Shortest path routines

        // Build the shortest path tree and the shortest path,
        // depending on whether we have start and end nodes selected.
        internal void CheckForPath()
        {
            // We have a start node. Build the shortest path tree.
            if (StartNode != null)
            {
                switch (AlgorithmType)
                {
                    case AlgorithmTypes.LabelSetting:
                        FindPathTreeLabelSetting();
                        break;
                    case AlgorithmTypes.LabelCorrecting:
                        FindPathTreeLabelCorrecting();
                        break;
                }

                // If we also have an end node, find the shortest path.
                if (EndNode != null) FindPath();
            }
        }

        // Build a shortest path tree rooted at the start node.
        private void FindPathTreeLabelCorrecting()
        {
            // Reset all nodes and links.
            foreach (Node node in Nodes)
            {
                node.TotalCost = double.PositiveInfinity;
                node.IsInPath = false;
                node.ShortestPathLink = null;
            }
            foreach (Link link in Links)
            {
                link.IsInTree = false;
                link.IsInPath = false;
            }

            // Place the start node on the candidate list.
            StartNode!.TotalCost = 0;
            List<Node> candidateList = new List<Node>();
            candidateList.Add(StartNode);

            // Process the candidate list until it is empty.
            // See https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm.
            int numPops = 0;
            while (candidateList.Count > 0)
            {
                // Process the first item in the candidate list.
                Node bestCandidate = candidateList[0];
                candidateList.RemoveAt(0);
                numPops++;

                // Check this node's links.
                foreach (Link link in bestCandidate.Links)
                {
                    // Get the node at the other end of this link.
                    Node otherNode = link.ToNode;

                    // See if we can improve the other node's total cost.
                    double newTotalCost = bestCandidate.TotalCost + link.Cost;
                    if (newTotalCost < otherNode.TotalCost)
                    {
                        otherNode.TotalCost = newTotalCost;
                        otherNode.ShortestPathLink = link;

                        // Add the other node to the candidate list.
                        candidateList.Add(otherNode);
                    }
                }
            }

            // Print stats.
            Console.WriteLine(string.Format("Pops: {0}", numPops));

            // Set IsInTree for links in the shortest path tree.
            foreach (Node node in Nodes)
            {
                if (node.ShortestPathLink != null)
                    node.ShortestPathLink.IsInTree = true;
            }
        }

        // Build a shortest path tree rooted at the start node.
        private void FindPathTreeLabelSetting()
        {
            // Reset all nodes and links.
            foreach (Node node in Nodes)
            {
                node.TotalCost = double.PositiveInfinity;
                node.IsInPath = false;
                node.ShortestPathLink = null;
                node.Visited = false;
            }
            foreach (Link link in Links)
            {
                link.IsInTree = false;
                link.IsInPath = false;
            }

            // Place the start node on the candidate list.
            StartNode!.TotalCost = 0;
            List<Node> candidateList = new List<Node>();
            candidateList.Add(StartNode);

            // Process the candidate list until it is empty.
            // See https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm.
            int numPops = 0;
            int numChecks = 0;
            while (candidateList.Count > 0)
            {
                // Find the candidate with the smallest totalCost.
                double bestTotalCost = double.PositiveInfinity;
                int bestIndex = -1;
                for (int index = 0; index < candidateList.Count; index++)
                {
                    numChecks++;
                    if (bestTotalCost > candidateList[index].TotalCost)
                    {
                        bestTotalCost = candidateList[index].TotalCost;
                        bestIndex = index;
                    }
                }

                // Process the best candidate.
                Node bestCandidate = candidateList[bestIndex];
                candidateList.RemoveAt(bestIndex);
                bestCandidate.Visited = true;
                numPops++;

                // Check this node's links.
                foreach (Link link in bestCandidate.Links)
                {
                    // Get the node at the other end of this link.
                    Node otherNode = link.ToNode;
                    if (otherNode.Visited) continue;

                    // See if we can improve the other node's totalCost.
                    double newTotalCost = bestCandidate.TotalCost + link.Cost;
                    if (newTotalCost < otherNode.TotalCost)
                    {
                        otherNode.TotalCost = newTotalCost;
                        otherNode.ShortestPathLink = link;

                        // Add the other node to the candidate list.
                        candidateList.Add(otherNode);
                    }
                }
            }

            // Print stats.
            Console.WriteLine(string.Format("Checks: {0}", numChecks));
            Console.WriteLine(string.Format("Pops:   {0}", numPops));

            // Set IsInTree for links in the shortest path tree.
            foreach (Node node in Nodes)
            {
                if (node.ShortestPathLink != null)
                    node.ShortestPathLink.IsInTree = true;
            }
        }

        private void FindPath()
        {
            // If there is no path between the start and end nodes, return.
            if (EndNode!.ShortestPathLink == null) return;

            // Follow the path backwards from the end node to the start node.
            Node node = EndNode;
            while (node != StartNode)
            {
                // Mark this node's shortest path link.
                node.ShortestPathLink!.IsInPath = true;
                node = node.ShortestPathLink.FromNode;
            }
            Console.WriteLine(string.Format("Total cost: {0}", EndNode.TotalCost));
        }

        #endregion Shortest path routines

        // Determine which algorithm to use.
        internal enum AlgorithmTypes
        {
            LabelSetting,
            LabelCorrecting,
        }
        private AlgorithmTypes algorithmType = AlgorithmTypes.LabelSetting;
        internal AlgorithmTypes AlgorithmType
        {
            get
            {
                return algorithmType;
            }
            set
            {
                // Save the new value.
                algorithmType = value;

                // Use the newly selected algorithm to
                // check for a tree and path.
                CheckForPath();
            }
        }

    }
}
