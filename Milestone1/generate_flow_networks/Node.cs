using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace generate_flow_networks
{
    internal class Node
    {
        internal List<Link> Links = new List<Link>();
        internal Network Network { get; set; }
        internal Point Center { get; set; }
        internal string Text { get; set; }
        internal int Index { get; set; }

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

            Rect rect = new Rect(
                Center.X - radius,
                Center.Y - radius,
                diameter,
                diameter);
            canvas.DrawEllipse(rect, Brushes.White, Brushes.Black, 1);

            if (drawLabels)
                canvas.DrawString(Text, diameter, diameter,
                    Center, 0, 12, Brushes.Blue);
        }
    }
}
