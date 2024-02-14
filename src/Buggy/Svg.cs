namespace Buggy
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using global::Svg;

    public static class Svg
    {
        public static Icon LoadIcon(string path, int width = 256, int height = 256)
        {
            var bitmap = Load(path);
            var handle = bitmap.GetHicon();
            return Icon.FromHandle(handle);
        }

        public static Bitmap Load(string path, Color? color = null, int width = 256, int height = 256)
        {
            var svgDoc = SvgDocument.Open(path);
            ProcessNodes(svgDoc.Descendants(), new SvgColourServer(color ?? Color.Black));
            return svgDoc.Draw(width, height);
        }

        private static void ProcessNodes(IEnumerable<SvgElement> nodes, SvgPaintServer colorServer)
        {
            foreach (var node in nodes)
            {
                if (node.Fill != SvgPaintServer.None) node.Fill = colorServer;
                if (node.Color != SvgPaintServer.None) node.Color = colorServer;
                if (node.Stroke != SvgPaintServer.None) node.Stroke = colorServer;
                node.StrokeWidth = 34;

                ProcessNodes(node.Descendants(), colorServer);
            }
        }

        public static ToolStripItem Add(this ToolStripItemCollection collection, string text, string svgPath, EventHandler onClick)
        {
            var image = Load(svgPath);
            return collection.Add(text, image, onClick);
        }
    }
}