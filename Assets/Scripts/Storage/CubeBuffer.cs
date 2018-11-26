using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CubeBuffer<T>
{
    // The diameter of the cube buffer; i.e., its side length.
    private readonly int size;

    // The internal buffer we use to keep track of objects.
    private readonly T[,,] cube;

    // The indicies of each face of the cube. Used internally for shifting faces.
    private readonly int[][] faceIndices;

	public CubeBuffer (int size) {
		this.size = size;
		cube = new T[size, size, size];

		// O(size^2), but run only once per game, so not too bad
		int enumLength = Enum.GetValues (typeof(Direction)).Length;
		faceIndices = new int[enumLength][];

		for (int enumCount = 0; enumCount < enumLength; enumCount++) {
			faceIndices [enumCount] = new int[size * size];
			int count = 0;
			for (int i = 0; i < size; i++) {
				for (int j = 0; j < size; j++) {
					switch((Direction)enumCount) {
						case Direction.LEFT:
							faceIndices [enumCount] [count++] = CoordsToIndex (size, 0, i, j);
							break;
						case Direction.RIGHT:
							faceIndices [enumCount] [count++] = CoordsToIndex(size, size - 1, i, j);
							break;
						case Direction.BACK:
							faceIndices [enumCount] [count++] = CoordsToIndex(size, i, j, 0);
							break;
						case Direction.FRONT:
							faceIndices [enumCount] [count++] = CoordsToIndex(size, i, j, size - 1);
							break;
						case Direction.DOWN:
							faceIndices [enumCount] [count++] = CoordsToIndex(size, i, 0, j);
							break;
						case Direction.UP:
							faceIndices [enumCount] [count++] = CoordsToIndex(size, i, size - 1, j);
							break;
					}
				}
			}
		}
	}

    public static int CoordsToIndex(int size, int x, int y, int z)
    {
        return (x * size * size) + (y * size) + z;
    }

    public static Vector3Int IndexToCoords(int size, int i)
    {
        return new Vector3Int((int)((float)i / size / size) % size, (int)((float)i / size) % size, i % size);
    }


    // Access using the combined index
    public T this[int index]
    {
        get { return cube[(index / size / size) % size, (index / size) % size, index % size]; }
        set { cube[(index / size / size) % size, (index / size) % size, index % size] = value; }
    }

	// Access using x,y,z 
	public T this[int x, int y, int z] {
		get { return cube [x, y, z]; }
		set { cube [x, y, z] = value; }
	}

    /// <summary>
    /// Delete the specified face.
    /// </summary>
    /// <param name="face">Face.</param>
	public void Delete (Direction face) {
		//Debug.Log ("Deleting " + face);

		int[] indices = faceIndices [(int)face];
		for (int i = 0; i < indices.Length; i++) {
			Vector3Int pos = IndexToCoords (size, indices[i]);

			if (typeof(GameObject) == typeof(T)) {
				(cube [pos.x, pos.y, pos.z] as GameObject).SetActive (false);
			}
			
			cube [pos.x, pos.y, pos.z] = default(T);
		}
	}

    /// <summary>
    /// Moves the cube one unit in the provided direction.
    /// This deletes the OPPOSITE face, and shifts every element towards the
    /// deleted face. That is to say, if we move LEFT, we delete RIGHT and
    /// shift RIGHT. 
    /// </summary>
    /// <param name="dir">The direction to shift in.</param>
    public void Shift (Direction dir) {
		int xStart = 0;
		int xEnd = size;
		int xDelta = 1;
		int xAmount = 0;

		int yStart = 0;
		int yEnd = size;
		int yDelta = 1;
		int yAmount = 0;

		int zStart = 0;
		int zEnd = size;
		int zDelta = 1;
		int zAmount = 0;

		// Set up the parameters of shifting
		switch (dir) {
			case Direction.LEFT:
				Delete (Direction.RIGHT);

				xStart = size - 1;
				xEnd = 0;
				xDelta = -1;
				xAmount = -1;
				break;
			case Direction.RIGHT:
                Delete(Direction.LEFT);

				xStart = 0;
				xEnd = size - 1;
				xDelta = 1;
				xAmount = 1;
				break;
			case Direction.DOWN:
                Delete(Direction.UP);

				yStart = size - 1;
				yEnd = 0;
				yDelta = -1;
				yAmount = -1;
				break;
			case Direction.UP:
                Delete(Direction.DOWN);

				yStart = 0;
				yEnd = size - 1;
				yDelta = 1;
				yAmount = 1;
				break;
			case Direction.BACK:
                Delete(Direction.FRONT);

				zStart = size - 1;
				zEnd = 0;
				zDelta = -1;
				zAmount = -1;
				break;
			case Direction.FRONT:
                Delete(Direction.BACK);

				zStart = 0;
				zEnd = size - 1;
				zDelta = 1;
				zAmount = 1;
				break;
		}

        // Do the actual shifting operation
        for (int x = xStart; x != xEnd; x += xDelta)
        {
            for (int y = yStart; y != yEnd; y += yDelta)
            {
                for (int z = zStart; z != zEnd; z += zDelta)
                {
                    cube[x, y, z] = cube[x + xAmount, y + yAmount, z + zAmount];
                }
            }
        }
	}
}
