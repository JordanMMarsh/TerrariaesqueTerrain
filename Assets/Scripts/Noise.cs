using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float GetPerlinNoise(Vector2 position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x + 0.1f) / BlockData.chunkWidth * scale + offset, (position.y + 0.1f) / BlockData.chunkHeight * scale + offset);
    }
}