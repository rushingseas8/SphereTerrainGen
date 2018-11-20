using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace FastMarchingCubes
{
    public interface IMarching
    {

        //float Surface { get; set; }

		void Generate(float[] voxels, int width, int height, int depth, IList<Vector3> verts, IList<int> indices);

    }

}