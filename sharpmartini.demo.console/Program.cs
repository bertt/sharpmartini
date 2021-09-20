using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace sharpmartini.demo.console
{
    class Program
    {
        static void Main(string[] args)
        {
            ushort maxError = 100;
            var fuji = new Bitmap(@"fixtures/fuji.png");

            var sw = new Stopwatch();
            sw.Start();
            var terrain = GridCreator.MapboxTerrainToGrid(fuji);
            var martini = new Martini(fuji.Width + 1);
            var tile = martini.CreateTile(terrain);
            var mesh = tile.getMesh(maxError);

            sw.Stop();
            Console.WriteLine("Elpased: " + sw.Elapsed);

            var vertices1 = new List<Vertice>();
            for (var i = 0; i < mesh.vertices.Length / 2; i++)
            {
                var x = mesh.vertices[i * 2];
                var y = mesh.vertices[i * 2 + 1];
                var z = terrain[y * (fuji.Width + 1) + x];

                vertices1.Add(new Vertice { X = x, Y = y, Z = z }); ;
            }

            var vertices = vertices1.ToArray();

            var graphics = Graphics.FromImage(fuji);
            var blackPen = new Pen(Color.Black, 1);

            for (var j = 0; j < mesh.triangles.Length / 3; j++)
            {
                var v1 = vertices[mesh.triangles[j * 3]];
                var v2 = vertices[mesh.triangles[j * 3 + 1]];
                var v3 = vertices[mesh.triangles[j * 3 + 2]];

                graphics.DrawLine(blackPen, new Point(v1.X, v1.Y), new Point(v2.X, v2.Y));
                graphics.DrawLine(blackPen, new Point(v2.X, v2.Y), new Point(v3.X, v3.Y));
                graphics.DrawLine(blackPen, new Point(v3.X, v3.Y), new Point(v1.X, v1.Y));
            }

            fuji.Save($"test_{maxError}.png", ImageFormat.Png);
        }
    }
}
