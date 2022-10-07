using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace generate_flow_networks
{
    internal class Network
    {
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
                int num_nodes = int.Parse(ReadNextLine(reader));
                int num_links = int.Parse(ReadNextLine(reader));

                // Read the nodes.
                for (int i = 0; i < num_nodes; i++)
                {
                    // Read the next node's values.
                    string[] fields = ReadNextLine(reader).Split(',');
                    double x = double.Parse(fields[0]);
                    double y = double.Parse(fields[1]);
                    string text = fields[2].Trim();

                    // Make the node. (This adds the node to the network.)
                    Node node = new Node(this, new Point(x, y), text);
                }

                // Read the links.
                for (int i = 0; i < num_links; i++)
                {
                    // Read the next link's values.
                    string[] fields = ReadNextLine(reader).Split(',');
                    int index1 = int.Parse(fields[0]);
                    int index2 = int.Parse(fields[1]);
                    double capacitiy = double.Parse(fields[2]);

                    // Make the link. (This adds the link to the network.)
                    Link link = new Link(this, Nodes[index1], Nodes[index2], capacitiy);
                }
            }
        }

        // Read the next non-blank line from the serialization.
        private string ReadNextLine(StringReader reader)
        {
            // Repeat until we get a line or reach the end.
            for (; ; )
            {
                // Get the next line.
                string line = reader.ReadLine();

                // If we've reached the end of the stream, return null.
                if (line == null) return null;

                // Trim comments.
                line = line.Split('#')[0];
                line = line.Trim();

                // If the line is non-blank, return it.
                if (line.Length > 0) return line;
            }
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

        internal Rect GetBounds()
        {
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
            return new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
        }
    }
}
