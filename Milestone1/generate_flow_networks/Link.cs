using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace generate_flow_networks
{
    internal class Link
    {
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
            Flow = 0;
            network.AddLink(this);
            fromNode.AddLink(this);
        }

        public override string ToString()
        {
            return string.Format("{0} --> {1} ({2})", FromNode, ToNode, Capacity);
        }

        internal void Draw(Canvas canvas)
        {
            canvas.DrawLine(FromNode.Center, ToNode.Center, Brushes.Green, 1);
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
            canvas.DrawString($"{Flow.ToString()}/{Capacity.ToString()}", DIAMETER, DIAMETER,
                new Point(x, y), angle, 12, Brushes.Black);
        }

    }
}
