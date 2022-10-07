using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace shortest_paths
{
    internal class Link
    {
        internal Line MyLine { get; set; }
        internal Network Network { get; set; }
        internal Node FromNode { get; set; }
        internal Node ToNode { get; set; }
        internal double Cost { get; set; }

        internal Link(Network network, Node fromNode, Node toNode, double cost)
        {
            Network = network;
            FromNode = fromNode;
            ToNode = toNode;
            Cost = cost;

            Network.AddLink(this);
            FromNode.AddLink(this);
        }

        public override string ToString()
        {
            return string.Format("{0} --> {1} ({2})",
                FromNode, ToNode, Cost);
        }

        private bool isInTree = false;
        internal bool IsInTree
        {
            get { return isInTree; }
            set
            {
                isInTree = value;
                SetLinkAppearance();
            }
        }

        private bool isInPath = false;
        internal bool IsInPath
        {
            get { return isInPath; }
            set
            {
                isInPath = value;
                SetLinkAppearance();
            }
        }

        // Set the node's color appropriately.
        private void SetLinkAppearance()
        {
            if (isInPath)
            {
                MyLine.Stroke = Brushes.Red;
                MyLine.StrokeThickness = 6;
            }
            else if (isInTree)
            {
                MyLine.Stroke = Brushes.Lime;
                MyLine.StrokeThickness = 6;
            }
            else
            {
                MyLine.Stroke = Brushes.Black;
                MyLine.StrokeThickness = 1;
            }
        }

        internal void Draw(Canvas canvas)
        {
            MyLine = canvas.DrawLine(FromNode.Center, ToNode.Center, Brushes.Black, 1);
            MyLine.Tag = this;
        }

        // Draw a label along this link.
        internal void DrawLabel(Canvas canvas)
        {
            const double RADIUS = 10;
            const double DIAMETER = 2 * RADIUS;

            double dx = ToNode.Center.X - FromNode.Center.X;
            double dy = ToNode.Center.Y - FromNode.Center.Y;
            double angle = (Math.Atan2(dy, dx) * 180 / Math.PI) - 0;
            double x = 0.67 * FromNode.Center.X + 0.33 * ToNode.Center.X;
            double y = 0.67 * FromNode.Center.Y + 0.33 * ToNode.Center.Y;
            Rect rect = new Rect(
                x - RADIUS, y - RADIUS,
                DIAMETER, DIAMETER);
            canvas.DrawEllipse(rect, Brushes.White, null, 0);
            canvas.DrawString(Cost.ToString(), DIAMETER, DIAMETER,
                new Point(x, y), angle, 12, Brushes.Black);
        }
    }
}
