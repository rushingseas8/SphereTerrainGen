using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareBuffer<T> 
{
    // The diameter of the square buffer; i.e., its side length.
    protected readonly int size;

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
            faceIndices[enumCount] = new int[size];
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
    protected void Delete(Direction face)
    {
        //Debug.Log ("Deleting " + face);

        int[] indices = faceIndices[(int)face];
        for (int i = 0; i < indices.Length; i++)
        {
            // Grab the index
            Vector2Int pos = IndexToCoords(size, indices[i]);

            // Delete the object using its callback
            OnDelete(pos.x, pos.y, square[pos.x, pos.y]);

            // Set the value to the default. Null for reference types,
            // and the closest thing to a zero for value types.
            square[pos.x, pos.y] = default(T);
        }
    }

    /// <summary>
    /// Callback for when an object in the buffer is deleted.
    /// Use this callback for e.g. setting a GameObject to inactive, saving
    /// chunks to disk, etc. By default this tries to work with GameObjects.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="deleted">Deleted.</param>
    protected virtual void OnDelete(int x, int y, T deleted)
    {
        if (typeof(GameObject) == typeof(T))
        {
            (deleted as GameObject).SetActive(false);
        }
    }

    /// <summary>
    /// Moves the cube one unit in the provided direction.
    /// This deletes the OPPOSITE face, and shifts every element towards the
    /// deleted face. That is to say, if we move LEFT, we delete RIGHT and
    /// shift RIGHT. 
    /// </summary>
    /// <param name="dir">The direction to shift in.</param>
    public void Shift(Direction face)
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
        switch (face)
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
                OnShift(x, y, square[x, y]);
            }
        }

        // Regenerate the other side, if needed.

        int[] indices = faceIndices[(int)face];
        Debug.Log("Generating " + indices.Length + " new terrain.");
        for (int i = 0; i < indices.Length; i++)
        {
            Vector2Int pos = IndexToCoords(size, indices[i]);
            square[pos.x, pos.y] = Generate(pos.x, pos.y);
        }
    }

    /// <summary>
    /// Called when the object is shifted. The index returned is the new one;
    /// the object returned is the one at the new index.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="shifted">Shifted object.</param>
    protected virtual void OnShift(int x, int y, T shifted)
    {
        // Do nothing by default.
    }

    /// <summary>
    /// Generate a new object at the specified index.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    protected virtual T Generate(int x, int y)
    {
        // Do the null or zero value by default.
        return default(T);
    }
}
