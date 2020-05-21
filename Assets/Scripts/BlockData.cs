using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockData
{
    public static readonly int chunkWidth = 10;
    public static readonly int chunkHeight = 10;
    public static readonly int worldWidthInChunks = 20;
    public static readonly int worldHeightInChunks = 15;
    
    public static readonly int TextureAtlasWidthInBlocks = 4;
    public static float NormalizedBlockTextureSize
    {
        get { return 1f / TextureAtlasWidthInBlocks; }
    }

    public static int worldWidthInBlocks
    {
        get { return chunkWidth * worldWidthInChunks; }
    }

    public static int worldHeightInBlocks
    {
        get { return chunkHeight * worldHeightInChunks; }
    }

    public static readonly Vector2[] BlockVertices = new Vector2[4]
    {
        new Vector3(0, 0),
        new Vector3(0, 1),
        new Vector3(1, 0),
        new Vector3(1, 1)
    };
}
