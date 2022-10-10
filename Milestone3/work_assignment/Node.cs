using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace work_assignment
{
    internal class Node
    {
        internal List<Link> Links = new List<Link>();
        internal Network Network { get; set; }
        internal Point Center { get; set; }
        internal string Text { get; set; }
        internal int Index { get; set; }
        internal Ellipse? MyEllipse { get; set; }
        internal Label? MyLabel { get; set; }
        internal double TotalCost { get; set; }
        internal bool IsInPath { get; set; }
        internal Link? ShortestPathLink { get; set; }
        internal bool Visited { get; set; }
        internal List<Link> BackLinks { get; set; }
        internal Node FromNode { get; set; }
        internal Link FromLink { get; set; }
        internal Node(Network network, Point center, string text)
        {
            Network = network;
            Center = center;
            Text = text;
            Index = -1;

            Network.AddNode(this);
        }

        public override string ToString()
        {
            return string.Format("[{0}]", Text);
        }

        private bool isStartNode = false;

        internal bool IsStartNode
        {
            get { return isStartNode; }

            set
            {
                isStartNode = value;
                SetNodeAppearance();
            }
        }

        private bool isEndNode = false;

        internal bool IsEndNode
        {
            get { return isEndNode; }

            set
            {
                isEndNode = value;
                SetNodeAppearance();
            }
        }

        // Set the node's color appropriately.
        private void SetNodeAppearance()
        {
            if (IsStartNode)
            {
                MyEllipse.Fill = Brushes.Pink;
                MyEllipse.Stroke = Brushes.Red;
                MyEllipse.StrokeThickness = 2;
            }
            else if (IsEndNode)
            {
                MyEllipse.Fill = Brushes.LightGreen;
                MyEllipse.Stroke = Brushes.Green;
                MyEllipse.StrokeThickness = 2;
            }
            else
            {
                MyEllipse.Fill = Brushes.White;
                MyEllipse.Stroke = Brushes.Black;
                MyEllipse.StrokeThickness = 1;
            }
        }

        internal void AddLink(Link link)
        {
            Links.Add(link);
        }

        internal void Draw(Canvas canvas, bool drawLabels)
        {
            const double LARGE_RADIUS = 10;
            const double SMALL_RADIUS = 3;
            double radius;

            if (drawLabels) radius = LARGE_RADIUS;
            else radius = SMALL_RADIUS;

            double diameter = 2 * radius;

            Rect rect = new Rect(Center.X - radius, Center.Y - radius, diameter, diameter);

            MyEllipse = canvas.DrawEllipse(rect, Brushes.White, Brushes.Black, 1);
            MyEllipse.Tag = this;
            MyEllipse.MouseDown += Network.ellipse_MouseDown;

            if (!drawLabels)
            {
                MyLabel = null;
            }
            else
            {
                MyLabel = canvas.DrawString(Text, diameter, diameter, Center, 0, 12, Brushes.Blue);

                MyLabel.Tag = this;
                MyLabel.MouseDown += Network.label_MouseDown;
            }
        }
    }
}
