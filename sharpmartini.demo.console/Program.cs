using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace sharpmartini.demo.console
{
    class Program
    {
        static void Main(string[] args)
        {
            ushort maxError = 100;
            var fuji = Image.Load<Rgba32>(@"fixtures/fuji.png");

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
            var pen = Pens.Solid(Color.Black, 1);

            fuji.Mutate(imageContext =>
            {
                for (var j = 0; j < mesh.triangles.Length / 3; j++)
                {
                    var v1 = vertices[mesh.triangles[j * 3]];
                    var v2 = vertices[mesh.triangles[j * 3 + 1]];
                    var v3 = vertices[mesh.triangles[j * 3 + 2]];


                    var line = new List<PointF>();
                    line.Add(new PointF(v1.X, v1.Y));
                    line.Add(new PointF(v2.X, v2.Y));
                    line.Add(new PointF(v3.X, v3.Y));
                    line.Add(new PointF(v1.X, v1.Y));

                    imageContext.DrawLines(pen,line.ToArray());
                }
            });

            using (var file = File.Create("DrawLinesTest.png"))
            {
                fuji.SaveAsPng(file);
            }
        }
    }
}
