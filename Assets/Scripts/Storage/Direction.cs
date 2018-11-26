using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a given direction. Used by CubeBuffer to determine which direction to
/// shift in, and which face to delete.
/// </summary>
public enum Direction
{
    LEFT = 0,
    RIGHT = 1,
    FRONT = 2,
    BACK = 3,
    UP = 4,
    DOWN = 5,
    NONE = -1
}
