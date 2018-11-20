using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.Profiling;

using UnityEngine;

namespace FastMarchingCubes
{
    public abstract class Marching : IMarching
    {

        //public float Surface { get; set; }

        //private float[] Cube { get; set; }

		public float Surface;
		private float[] Cube;

        /// <summary>
        /// Winding order of triangles use 2,1,0 or 0,1,2
        /// </summary>
        //protected int[] WindingOrder { get; private set; }
		protected int[] WindingOrder;

        public Marching(float surface = 0f)
        {
            Surface = surface;
            Cube = new float[8];
            WindingOrder = new int[] { 0, 1, 2 };
        }

		public virtual void Generate(float[] voxels, int width, int height, int depth, IList<Vector3> verts, IList<int> indices)
        {
            if (Surface > 0.0f)
            {
                WindingOrder[0] = 0;
                WindingOrder[1] = 1;
                WindingOrder[2] = 2;
            }
            else
            {
                WindingOrder[0] = 2;
                WindingOrder[1] = 1;
                WindingOrder[2] = 0;
            }

            int x, y, z;
			int wh = width * height;

			int[] yw = new int[height];
			for (int i = 0; i < height; i++) { yw [i] = i * width; }

			int[] zwh = new int[depth];
			for (int i = 0; i < depth; i++) { zwh [i] = i * wh; }

            for (x = 0; x < width - 1; x++) {
                for (y = 0; y < height - 1; y++) {
                    for (z = 0; z < depth - 1; z++) {
                        //Get the values in the 8 neighbours which make up a cube
						//Profiler.BeginSample("Neighbor search");

						int baseIndex = x + yw[y] + zwh[z];
						int b1 = baseIndex + 1;
						int b1w = b1 + width;
						int bw = baseIndex + width;

						Cube [0] = voxels[baseIndex];
						Cube [1] = voxels[b1];
						Cube [2] = voxels[b1w];
						Cube [3] = voxels[bw];

						Cube [4] = voxels[baseIndex + wh];
						Cube [5] = voxels[b1 + wh];
						Cube [6] = voxels[b1w + wh];
						Cube [7] = voxels[bw + wh];

						//Profiler.EndSample ();

                        //Perform algorithm
						//Profiler.BeginSample("March");
                        March(x, y, z, Cube, verts, indices);
						//Profiler.EndSample ();
                    }
                }
			}

        }

        public void MarchBlock(float[] voxels, int width, int height, int depth, IList<Vector3> vertList, IList<int> indexList) 
        {
            //float[] cubes = new float[(width - 1) * (height - 1) * (depth - 1)];
            //Profiler.BeginSample("Setup");
            Vector3[] EdgeVertex = new Vector3[12];

            int x, y, z;
            int wh = width * height;

            int[] yw = new int[height];
            for (int i = 0; i < height; i++) { yw[i] = i * width; }

            int[] zwh = new int[depth];
            for (int i = 0; i < depth; i++) { zwh[i] = i * wh; }

            //Profiler.EndSample();

            //Profiler.BeginSample("Less than array");
            int[] lessThan = new int[width * height * depth];
            for (x = 0; x < width; x++) {
                for (y = 0; y < height; y++) {
                    for (z = 0; z < depth; z++) {
                        int baseIndex = x + yw[y] + zwh[z];
                        lessThan[baseIndex] = voxels[baseIndex] <= 0 ? 1 : 0;
                    }
                }
            }
            //Profiler.EndSample();
            x = y = z = 0;

            //Profiler.BeginSample("Marching over the array");
            for (x = 0; x < width - 1; x++)
            {
                for (y = 0; y < height - 1; y++)
                {
                    for (z = 0; z < depth - 1; z++)
                    {
                        //Get the values in the 8 neighbours which make up a cube
                        //Profiler.BeginSample("Index computation");

                        int baseIndex = x + yw[y] + zwh[z];
                        int b1 = baseIndex + 1;
                        int b1w = b1 + width;
                        int bw = baseIndex + width;

                        //Profiler.EndSample();

                        //Profiler.BeginSample("Voxel surface computation");

                        int flagIndex = 
                            lessThan[baseIndex] | 
                            (lessThan[b1] << 1) | 
                            (lessThan[b1w] << 2) |
                            (lessThan[bw] << 3) |
                            (lessThan[baseIndex + wh] << 4) |
                            (lessThan[b1 + wh] << 5) |
                            (lessThan[b1w + wh] << 6) |
                            (lessThan[bw + wh] << 7);

                        //cubes[baseIndex] = flagIndex;

                        int edgeFlags = MarchingCubes.CubeEdgeFlags[flagIndex];

                        //Profiler.EndSample();

                        //If the cube is entirely inside or outside of the surface, then there will be no intersections
                        if (edgeFlags == 0)
                        {
                            continue;
                        }
                        int i, j, vert, idx;
                        float offset = 0.0f;

                        //Profiler.BeginSample("March intersection");
                        //Find the point of intersection of the surface with each edge
                        for (i = 0; i < 12; i++)
                        {
                            //if there is an intersection on this edge
                            if ((edgeFlags & (1 << i)) != 0)
                            {
                                int ec1 = MarchingCubes.EdgeConnection[i, 0];
                                int ec2 = MarchingCubes.EdgeConnection[i, 1];
                                float p1, p2;

                                // Flip the 1 bit if the 2 bit is set. If you see the cube mapping we use, the x coord is flipped
                                // for 2, 3, 6, and 7. As a bonus, we can add a constant (0 or 1) to the y value rather than recomputing
                                // (ec1 & 2) >> 1.
                                if ((ec1 & 2) == 0)
                                {
                                    p1 = voxels[(x + (ec1 & 1)) + (y * width) + ((z + ((ec1 & 4) >> 2)) * wh)];
                                } else 
                                {
                                    p1 = voxels[(x + (1 - (ec1 & 1))) + ((y + 1) * width) + ((z + ((ec1 & 4) >> 2)) * wh)];
                                }
                                if ((ec2 & 2) == 0)
                                {
                                    p2 = voxels[(x + (ec2 & 1)) + (y * width) + ((z + ((ec2 & 4) >> 2)) * wh)];
                                } else 
                                {
                                    p2 = voxels[(x + (1 - (ec2 & 1))) + ((y + 1) * width) + ((z + ((ec2 & 4) >> 2)) * wh)];
                                }


                                float delta = p2 - p1;
                                offset = delta == 0f ? 0f : (-p1) / delta;

                                EdgeVertex[i].x = x + (VertexOffset[MarchingCubes.EdgeConnection[i, 0], 0] + offset * MarchingCubes.EdgeDirection[i, 0]);
                                EdgeVertex[i].y = y + (VertexOffset[MarchingCubes.EdgeConnection[i, 0], 1] + offset * MarchingCubes.EdgeDirection[i, 1]);
                                EdgeVertex[i].z = z + (VertexOffset[MarchingCubes.EdgeConnection[i, 0], 2] + offset * MarchingCubes.EdgeDirection[i, 2]);
                            }
                        }
                        //Profiler.EndSample();

                        //Profiler.BeginSample("March triangles");
                        //Save the triangles that were found. There can be up to five per cube
                        for (i = 0; i < 15; i += 3)
                        {
                            if (MarchingCubes.TriangleConnectionTable[flagIndex, i] < 0) break;

                            idx = vertList.Count;

                            for (j = 0; j < 3; j++)
                            {
                                vert = MarchingCubes.TriangleConnectionTable[flagIndex, i + j];
                                indexList.Add(idx + WindingOrder[j]);
                                vertList.Add(EdgeVertex[vert]);
                            }
                        }
                        //Profiler.EndSample();

                    }
                }
            }
            //Profiler.EndSample();
        }

         /// <summary>
        /// MarchCube performs the Marching algorithm on a single cube
        /// </summary>
        protected abstract void March(float x, float y, float z, float[] cube, IList<Vector3> vertList, IList<int> indexList);

        /// <summary>
        /// GetOffset finds the approximate point of intersection of the surface
        /// between two points with the values v1 and v2
        /// </summary>
        protected virtual float GetOffset(float v1, float v2)
        {
            float delta = v2 - v1;
            return (delta == 0.0f) ? Surface : (Surface - v1) / delta;
        }

        /// <summary>
        /// VertexOffset lists the positions, relative to vertex0, 
        /// of each of the 8 vertices of a cube.
        /// vertexOffset[8][3]
        /// </summary>
        protected static readonly int[,] VertexOffset = new int[,]
	    {
	        {0, 0, 0},{1, 0, 0},{1, 1, 0},{0, 1, 0},
	        {0, 0, 1},{1, 0, 1},{1, 1, 1},{0, 1, 1}
	    };

    }

}
