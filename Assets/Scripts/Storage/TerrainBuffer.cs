using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBuffer : SquareBuffer<TerrainTile> {

    public TerrainBuffer(int size) : base(size) {}

    protected override void OnDelete(int x, int y, TerrainTile deleted)
    {
        //base.OnDelete(x, y, deleted);
        if (deleted != null)
        {
            deleted.Destroy();
        }
    }

    protected override void OnShift(int x, int y, TerrainTile shifted)
    {
        //base.OnShift(x, y, shifted);
        //shifted.Destroy();
    }

    protected override TerrainTile Generate(int x, int y)
    {
        //return base.Generate(x, y);
        return new TerrainTile(GameManager.XChunk + x, GameManager.ZChunk + y, 3);
    }
}
