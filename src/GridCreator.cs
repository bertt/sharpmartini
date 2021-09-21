using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace sharpmartini
{
    public static class GridCreator
    {
        public static float[] MapboxTerrainToGrid(Image<Rgba32> png)
        {
            var gridSize = png.Width + 1;
            var terrain = new float[gridSize * gridSize];
            var tileSize = png.Width;

            for (var y = 0; y < tileSize; y++)
            {
                var pixelRowSpan = png.GetPixelRowSpan(y);
                for (var x = 0; x < tileSize; x++)
                {
                    var s = png[x, y];
                    var pixel = pixelRowSpan[x];
                    var r = pixel.R;
                    var g = pixel.G;
                    var b = pixel.B;
                    var h = (r * 256 * 256 + g * 256.0 + b) / 10.0f - 10000.0;
                    terrain[y * gridSize + x] = (float)h;
                }
            }
            // backfill right and bottom borders
            for (var x = 0; x < gridSize - 1; x++)
            {
                terrain[gridSize * (gridSize - 1) + x] = terrain[gridSize * (gridSize - 2) + x];
            }
            for (var y = 0; y < gridSize; y++)
            {
                terrain[gridSize * y + gridSize - 1] = terrain[gridSize * y + gridSize - 2];
            }

            return terrain;
        }

    }
}
