using System;
using System.Diagnostics;
using System.Linq;

namespace sharpmartini
{
    public class Tile
    {
        private readonly float[] terrain;
        private readonly Martini martini;
        private int size;
        private float[] errors;
        private UInt16[] vertices;
        private UInt32[] triangles;
        private UInt32[] indices;
        // private int numVertices;
        // private int numTriangles;
        private UInt32 maxError;

        public Tile(float[] Terrain, Martini Martini)
        {
            terrain = Terrain;
            martini = Martini;

            size = martini.gridSize;

            terrain = Terrain;
            martini = Martini;
            errors = new float[terrain.Length];
            indices = new UInt32[terrain.Length];
            update();
        }

        private void update()
        {
            var coords = martini.coords;
            //const { numTriangles, numParentTriangles, coords, gridSize: size} = this.martini;
            ///const { terrain, errors} = this;

            // iterate over all possible triangles, starting from the smallest level
            for (var i = martini.numTriangles - 1; i >= 0; i--)
            {
                var k = i * 4;
                var ax = coords[k + 0];
                var ay = coords[k + 1];
                var bx = coords[k + 2];
                var by = coords[k + 3];
                var mx = (ax + bx) >> 1;
                var my = (ay + by) >> 1;
                var cx = mx + my - ay;
                var cy = my + ax - mx;

                // calculate error in the middle of the long edge of the triangle
                var interpolatedHeight = (terrain[ay * size + ax] + terrain[by * size + bx]) / 2;
                var middleIndex = my * size + mx;
                var middleError = Math.Abs(interpolatedHeight - terrain[middleIndex]);

                errors[middleIndex] = Math.Max(errors[middleIndex], middleError);

                if (i < martini.numParentTriangles)
                { // bigger triangles; accumulate error with children
                    var leftChildIndex = ((ay + cy) >> 1) * size + ((ax + cx) >> 1);
                    var rightChildIndex = ((by + cy) >> 1) * size + ((bx + cx) >> 1);
                    errors[middleIndex] = new[] { errors[middleIndex], errors[leftChildIndex], errors[rightChildIndex] }.Max();
                }
            }
        }

        public (UInt16[] vertices, UInt32[] triangles) getMesh(ushort MaxError = 0)
        {
            maxError = MaxError;
            //const { gridSiz
            //e: size, indices} = this.martini;
            // const { errors} = this;
            var numVertices = 0;
            var numTriangles = 0;
            var max = (ushort)(size - 1);

            // use an index grid to keep track of vertices that were already used to avoid duplication
            // indices = new uint[] { 0 };
            // was: indices.fill(0);

            void countElements(ushort ax, ushort ay, ushort bx, ushort by, ushort cx, ushort cy)
            {
                var mx = (ax + bx) >> 1;
                var my = (ay + by) >> 1;

                Debug.WriteLine($"{ax}, {ay}, {bx}, {by}, {cx}, {cy}");

                if (Math.Abs(ax - cx) + Math.Abs(ay - cy) > 1 && errors[my * size + mx] > maxError)
                {
                    countElements(cx, cy, ax, ay, (ushort)mx, (ushort)my);
                    countElements(bx, by, cx, cy, (ushort)mx, (ushort)my);
                }
                else
                {
                    //if(indices[ay * size + ax] > 0)
                    //{
                    //    (uint)++numVertices;
                    //}
                    indices[ay * size + ax] = indices[ay * size + ax] > 0? indices[ay * size + ax] : (uint)++numVertices;
                    indices[by * size + bx] = indices[by * size + bx] > 0? indices[by * size + bx]: (uint)++numVertices;
                    indices[cy * size + cx] = indices[cy * size + cx] > 0? indices[cy * size + cx]: (uint)++numVertices;
                    numTriangles++;
                }
            }

            countElements(0, 0, max, max, max, 0);
            countElements(max, max, 0, 0, 0, max);

            // numveritces: 46 maar is 225
            // numtriangle: 75 is 75
            var vertices = new UInt16[numVertices * 2];
            var triangles = new UInt32[numTriangles * 3];

            var triIndex = 0;

            void processTriangle(ushort ax, ushort ay, ushort bx, ushort by, ushort cx, ushort cy)
            {
                var mx = (ax + bx) >> 1;
                var my = (ay + by) >> 1;

                if (Math.Abs(ax - cx) + Math.Abs(ay - cy) > 1 && errors[my * size + mx] > maxError)
                {
                    // triangle doesn't approximate the surface well enough; drill down further
                    processTriangle(cx, cy, ax, ay, (ushort)mx, (ushort)my);
                    processTriangle(bx, by, cx, cy, (ushort)mx, (ushort)my);

                }
                else
                {
                    // add a triangle
                    var a = indices[ay * size + ax] - 1;
                    var b = indices[by * size + bx] - 1;
                    var c = indices[cy * size + cx] - 1;

                    vertices[2 * a] = ax;
                    vertices[2 * a + 1] = ay;

                    vertices[2 * b] = bx;
                    vertices[2 * b + 1] = by;

                    vertices[2 * c] = cx;
                    vertices[2 * c + 1] = cy;

                    triangles[triIndex++] = a;
                    triangles[triIndex++] = b;
                    triangles[triIndex++] = c;
                }
            }

            processTriangle(0, 0, max, max, max, 0);
            processTriangle(max, max, 0, 0, 0, max);

            return (vertices, triangles);
        }


        // retrieve mesh in two stages that both traverse the error map:
        // - countElements: find used vertices (and assign each an index), and count triangles (for minimum allocation)
        // - processTriangle: fill the allocated vertices & triangles typed arrays


    }
}
