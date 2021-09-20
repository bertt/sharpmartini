using System;
using System.Diagnostics;
using System.Drawing;

namespace sharpmartini.benchmarkting
{
    class Program
    {
        static void Main(string[] args)
        {
            var png = new Bitmap(@"fixtures/fuji.png");
            var terrain = GridCreator.MapboxTerrainToGrid(png);
            var sw = new Stopwatch();

            sw.Start();
            var martini = new Martini(png.Width + 1);
            sw.Stop();
            Console.WriteLine("init tileset: " + sw.ElapsedMilliseconds + "ms" );

            sw.Reset();
            sw.Start();
            var tile = martini.CreateTile(terrain);
            sw.Stop();
            Console.WriteLine("create tile: " + sw.ElapsedMilliseconds + "ms");

            sw.Reset();
            sw.Start();
            var mesh = tile.getMesh(30);
            sw.Stop();
            Console.WriteLine("mesh (maxError = 30): " + sw.ElapsedMilliseconds + "ms");

            Console.WriteLine($"vertices: {mesh.vertices.Length / 2}, triangles: {mesh.triangles.Length / 3}");
        }
    }
}

