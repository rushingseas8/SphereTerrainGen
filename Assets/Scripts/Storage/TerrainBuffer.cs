using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBuffer : SquareBuffer<TerrainTile> {

    public TerrainBuffer(int size) : base(size) {}

    protected override void OnDelete(int x, int y, TerrainTile deleted)
    {
        if (deleted != null)
        {
            deleted.Destroy();
        }
    }

    protected override void OnShift(int x, int y, TerrainTile shifted)
    {
        // Can do some LOD stuff here
    }

    protected override TerrainTile Generate(int x, int y)
    {
        return new TerrainTile(GameManager.XChunk + x, GameManager.ZChunk + y, 3);
    }
}
