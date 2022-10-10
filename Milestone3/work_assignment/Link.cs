using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace work_assignment
{
    internal class Link
    {
        internal Line? MyLine { get; set; }
        internal Label? MyLabel { get; set; }
        internal Network Network { get; set; }
        internal Node FromNode { get; set; }
        internal Node ToNode { get; set; }
        internal double Capacity { get; set; }
        internal double Flow { get; set; }

        internal Link(Network network, Node fromNode, Node toNode, double capacity)
        {
            Network = network;
            FromNode = fromNode;
            ToNode = toNode;
            Capacity = capacity;

            Network.AddLink(this);
            FromNode.AddLink(this);
        }

        public override string ToString()
        {
            return string.Format("{0} --> {1} ({2})", FromNode, ToNode, Capacity);
        }

        // Set the link's text, color, and thickness appropriately.
        internal void SetLinkAppearance()
        {
            if (Flow > 0) MyLine!.Stroke = Brushes.Red;
            else MyLine!.Stroke = Brushes.Black;

            MyLine.StrokeThickness = 2 * Flow + 1;

            if (MyLabel != null)
            {
                string text = string.Format("{0}/{1}", Flow, Capacity);
                MyLabel.Content = text;
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

            Rect rect = new Rect(x - RADIUS, y - RADIUS, DIAMETER, DIAMETER);
            canvas.DrawEllipse(rect, Brushes.White, null, 0);

            string text = string.Format("{0}/{1}", Flow, Capacity);
            MyLabel = canvas.DrawString(text, DIAMETER, DIAMETER, new Point(x, y), angle, 12, Brushes.Black);
        }
    }
}
