using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;

namespace work_assignment
{
    internal class Network
    {
        internal Node? StartNode = null;
        internal Node? EndNode = null;
        internal List<Node> Nodes = new List<Node>();
        internal List<Link> Links = new List<Link>();
        internal List<Node> Employees = new List<Node>();
        internal List<Node> Jobs = new List<Node>();

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
            Employees = new List<Node>();
            Jobs = new List<Node>();
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
                sw.WriteLine(string.Format("{0},{1},{2}", node.Center.X, node.Center.Y, node.Text));
            }
            sw.WriteLine();

            // Save the links.
            sw.WriteLine("# Links.");

            foreach (Link link in Links)
            {
                sw.WriteLine(string.Format("{0},{1},{2}", link.FromNode.Index, link.ToNode.Index, link.Capacity));
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
                    string[]? skills = null;
                    Node node = new Node(this, new Point(x, y), text, "", skills);
                }

                // Read the links.
                for (int i = 0; i < numLinks; i++)
                {
                    // Read the next link's values.
                    string[] fields = ReadNextLine(reader)!.Split(',');
                    int index1 = int.Parse(fields[0]);
                    int index2 = int.Parse(fields[1]);
                    double capacity = double.Parse(fields[2]);

                    // Make the link. (This adds the link to the network.)
                    Link link = new Link(this, Nodes[index1], Nodes[index2], capacity);
                }
            }
        }

        // Read the next non-blank line from the serialization.
        private string? ReadNextLine(StringReader reader)
        {
            // Repeat until we get a line or reach the end.
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                // Trim comments.
                line = line.Split('#')[0];
                line = line.Trim();

                // If the line is non-blank, return it.
                if (line.Length > 0) return line;
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
            bool drawLabels = false;

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

            //// Check for shortest paths.
            //CheckForPath();

            // Calculate maximal flows.
            CalculateFlows();
        }

        // Find maximal flows.
        private void CalculateFlows()
        {
            // Make sure we have source and sink nodes.
            if (StartNode == null) return;
            if (EndNode == null) return;

            // Calculate maximal flows.

            // Prepare the links and nodes.
            foreach (Node node in Nodes)
            {
                node.Visited = false;
                node.BackLinks = new List<Link>();
            }
            foreach (Link link in Links)
            {
                link.ToNode.BackLinks!.Add(link);
                link.Flow = 0;
            }

            // Note that the BackLink lists contain the same
            // objects used in the nodes' Links lists.
            // That means if we update the flow on a link,
            // it is updated for the BackLink and vice versa.

            // Repeat until we can find no more improvements:
            for (; ; )
            {
                // Add the source node to the candidate list.
                List<Node> candidateList = new List<Node>();
                candidateList.Add(StartNode);
                StartNode.Visited = true;

                // Repeat until the candidate list is empty:
                while (candidateList.Count > 0)
                {
                    // Get the next candidate.
                    Node node = candidateList[0];
                    candidateList.RemoveAt(0);

                    // See if we can add flow to the node's links.
                    foreach (Link link in node.Links)
                    {
                        // See if we should add this neighbor to the candidate list.
                        Node neighbor = link.ToNode;
                        if ((!neighbor.Visited) && (link.Flow < link.Capacity))
                        {
                            // Add this neighbor to the candidate list.
                            candidateList.Add(neighbor);
                            neighbor.Visited = true;

                            // Record the node and link that got to the neighbor.
                            neighbor.FromNode = node;
                            neighbor.FromLink = link;
                        }
                    }

                    // See if we can subtract flow from the node's back links.
                    foreach (Link link in node.BackLinks!)
                    {
                        Node neighbor = link.FromNode;
                        if ((!neighbor.Visited) && (link.Flow > 0))
                        {
                            // Add this neighbor to the candidate list.
                            candidateList.Add(neighbor);
                            neighbor.Visited = true;

                            // Record the node and link that got to the neighbor.
                            neighbor.FromNode = node;
                            neighbor.FromLink = link;
                        }
                    }

                    // If we have reached the sink node, break out
                    // of the while len(candidateList) > 0 loop.
                    if (EndNode.Visited) break;
                }

                // If we didn't visit the sink, then we didn't find
                // an augmenting path so break out of the for(;;) loop.
                if (!EndNode.Visited) break;

                // Work back through the augmenting path updating the link flows.
                // First find the smallest unused capacity on the augmenting path.
                double smallest_capacity = double.PositiveInfinity;
                Node test_node = EndNode;
                while (test_node != StartNode)
                {
                    // Get the link that got us to this node.
                    Link link = test_node.FromLink!;

                    // See if this link was used as a normal link or a backlink.
                    double unused_capacity;
                    if (link.ToNode == test_node)
                        // Normal link.
                        unused_capacity = link.Capacity - link.Flow;
                    else
                        // Backlink.
                        unused_capacity = link.Flow;
                    if (smallest_capacity > unused_capacity)
                        smallest_capacity = unused_capacity;

                    // Go to the previous node in the path.
                    test_node = test_node.FromNode!;
                }

                // To update the augmenting path, follow the path
                // again, this time updating the flows.
                test_node = EndNode;
                while (test_node != StartNode)
                {
                    // Get the link that got us to this node.
                    Link link = test_node.FromLink!;

                    // See if this link was used as a
                    // normal link or a reverse link.
                    if (link.ToNode == test_node)
                        // Normal link.
                        link.Flow += smallest_capacity;
                    else
                        // Backlink.
                        link.Flow -= smallest_capacity;

                    // Go to the previous node in the path.
                    test_node = test_node.FromNode!;
                }

                // Reset the nodes' visited flags for the
                // next attempt at finding an augmenting path.
                foreach (Node node in Nodes) node.Visited = false;
            }

            // We're done. The total flow equals the
            // total flow out of the source. (Or into the sink.)
            double flow = 0;
            foreach (Link link in StartNode.Links) flow += link.Flow;
            Console.WriteLine(string.Format("Total flow: {0}", flow));

            // Update the link colors and thicnesses.
            foreach (Link link in Links) link.SetLinkAppearance();
        }

        internal void LoadJobsFile(string filename)
        {
            JobsBuilder(File.ReadAllText(filename));
        }

        internal void JobsBuilder(string serialization)
        {
            Clear();

            // Get a stream to read the serialization one line at a time.
            using (StringReader reader = new StringReader(serialization))
            {
                string? line;
                while ((line = ReadNextLine(reader)) != null)
                {
                    string[] fields = line.Split(';');
                    string[] skillsTools = fields[2].Split(',', StringSplitOptions.RemoveEmptyEntries);
                    Node node = new Node(this, new Point(), fields[0], fields[1], skillsTools);
                    var type = fields[0].Substring(0, 1).ToUpper();
                    if (type == "E")
                    {
                        Employees.Add(node);
                    }
                    else if (type == "J")
                    {
                        Jobs.Add(node);
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown record type {type}: record {line}");
                    }
                }
            }
        }
    }
}