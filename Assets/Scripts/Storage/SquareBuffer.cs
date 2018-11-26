using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareBuffer<T> 
{
    // The diameter of the square buffer; i.e., its side length.
    private readonly int size;

    // The internal buffer we use to keep track of objects.
    private readonly T[,] square;

    // The indicies of each face of the square. Used internally for shifting faces.
    private readonly int[][] faceIndices;

    public SquareBuffer(int size)
    {
        this.size = size;
        square = new T[size, size];

        // O(size^2), but run only once per game, so not too bad
        faceIndices = new int[4][];

        for (int enumCount = 0; enumCount < 4; enumCount++)
        {
            faceIndices[enumCount] = new int[size * size];
            int count = 0;
            for (int i = 0; i < size; i++)
            {
                switch ((Direction)enumCount)
                {
                    case Direction.LEFT:
                        faceIndices[enumCount][count++] = CoordsToIndex(size, 0, i);
                        break;
                    case Direction.RIGHT:
                        faceIndices[enumCount][count++] = CoordsToIndex(size, size - 1, i);
                        break;
                    case Direction.BACK:
                        faceIndices[enumCount][count++] = CoordsToIndex(size, i, 0);
                        break;
                    case Direction.FRONT:
                        faceIndices[enumCount][count++] = CoordsToIndex(size, i, size - 1);
                        break;
                }
            }
        }
    }

    public static int CoordsToIndex(int size, int x, int y)
    {
        return (x * size) + y;
    }

    public static Vector2Int IndexToCoords(int size, int i)
    {
        return new Vector2Int((int)((float)i / size) % size, i % size);
    }

    // Access using the combined index
    public T this[int index]
    {
        get { return square[(index / size) % size, index % size]; }
        set { square[(index / size) % size, index % size] = value; }
    }

    // Access using x,y
    public T this[int x, int y]
    {
        get { return square[x, y]; }
        set { square[x, y] = value; }
    }

    /// <summary>
    /// Delete the specified face.
    /// </summary>
    /// <param name="face">Face.</param>
    public void Delete(Direction face)
    {
        //Debug.Log ("Deleting " + face);

        int[] indices = faceIndices[(int)face];
        for (int i = 0; i < indices.Length; i++)
        {
            Vector2Int pos = IndexToCoords(size, indices[i]);

            if (typeof(GameObject) == typeof(T))
            {
                (square[pos.x, pos.y] as GameObject).SetActive(false);
            }

            square[pos.x, pos.y] = default(T);
        }
    }


    /// <summary>
    /// Moves the cube one unit in the provided direction.
    /// This deletes the OPPOSITE face, and shifts every element towards the
    /// deleted face. That is to say, if we move LEFT, we delete RIGHT and
    /// shift RIGHT. 
    /// </summary>
    /// <param name="dir">The direction to shift in.</param>
    public void Shift(Direction dir)
    {
        int xStart = 0;
        int xEnd = size;
        int xDelta = 1;
        int xAmount = 0;

        int yStart = 0;
        int yEnd = size;
        int yDelta = 1;
        int yAmount = 0;

        // Set up the parameters of shifting
        switch (dir)
        {
            case Direction.LEFT:
                Delete(Direction.RIGHT);

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
            case Direction.BACK:
                Delete(Direction.FRONT);

                yStart = size - 1;
                yEnd = 0;
                yDelta = -1;
                yAmount = -1;
                break;
            case Direction.FRONT:
                Delete(Direction.BACK);

                yStart = 0;
                yEnd = size - 1;
                yDelta = 1;
                yAmount = 1;
                break;
        }

        // Do the actual shifting operation
        for (int x = xStart; x != xEnd; x += xDelta)
        {
            for (int y = yStart; y != yEnd; y += yDelta)
            {
                square[x, y] = square[x + xAmount, y + yAmount];
            }
        }
    }
}
