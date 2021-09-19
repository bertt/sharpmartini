using System;

namespace sharpmartini
{
    public class Martini
    {
        public int gridSize;
        public int numTriangles;
        public int numParentTriangles;
        public UInt32[] indices;
        public UInt16[] coords;

        public Martini(int GridSize)
        {
            gridSize = GridSize;

            var tileSize = gridSize - 1;
            // if (tileSize & (tileSize - 1)) throw new Exception($"Expected grid size to be 2 ^ n + 1, got ${ gridSize }.");

            numTriangles = tileSize * tileSize * 2 - 2;
            numParentTriangles = this.numTriangles - tileSize * tileSize;

            indices = new UInt32[this.gridSize * this.gridSize];

            // coordinates for all possible triangles in an RTIN tile
            coords = new UInt16[this.numTriangles * 4];

            // get triangle coordinates from its index in an implicit binary tree
            for (var i = 0; i < this.numTriangles; i++)
            {
                var id = i + 2;
                var ax = 0; var ay = 0; var bx = 0; var by = 0; var cx = 0; var cy = 0;
                if ((id & 1) == 1)
                {
                    bx = by = cx = tileSize; // bottom-left triangle
                }
                else
                {
                    ax = ay = cy = tileSize; // top-right triangle
                }
                while ((id >>= 1) > 1)
                {
                    var mx = (ax + bx) >> 1;
                    var my = (ay + by) >> 1;

                    if ((id & 1) == 1)
                    { // left half
                        bx = ax; by = ay;
                        ax = cx; ay = cy;
                    }
                    else
                    { // right half
                        ax = bx; ay = by;
                        bx = cx; by = cy;
                    }
                    cx = mx; cy = my;
                }
                var k = i * 4;
                coords[k + 0] = (ushort)ax;
                coords[k + 1] = (ushort)ay;
                coords[k + 2] = (ushort)bx;
                coords[k + 3] = (ushort)by;
            }

        }

        public Tile CreateTile(float[] terrain)
        {
            return new Tile(terrain, this);
        }
    }
}
