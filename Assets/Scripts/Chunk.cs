using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    MeshFilter meshFilter;
    World world;
    public Vector2 chunkPosition;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private int vertexIndex = 0;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        world = FindObjectOfType<World>();
    }

    public void GenerateChunk()
    {
        for (int x = 0; x < BlockData.chunkWidth; x++)
        {
            for (int y = 0; y < BlockData.chunkHeight; y++)
            {
                AddBlockToMesh(x, y);
            }
        }

        BuildMesh();
    }

    private void AddBlockToMesh(int xPos, int yPos)
    {
        Vector2 blockPosition = new Vector2(xPos, yPos);
        int blockID = world.blockMap[(int)transform.position.x + xPos, (int)transform.position.y + yPos];
        if (blockID != 0)
        {
            vertices.Add(blockPosition + BlockData.BlockVertices[0]);
            vertices.Add(blockPosition + BlockData.BlockVertices[1]);
            vertices.Add(blockPosition + BlockData.BlockVertices[2]);
            vertices.Add(blockPosition + BlockData.BlockVertices[3]);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 3);
            vertexIndex += 4;

            //Add to uv list according to current blockID
            AddTexture(world.blockTypes[blockID].blockTextureID);
        }     
    }

    private void AddTexture(int textureID)
    {
        float y = textureID / BlockData.TextureAtlasWidthInBlocks;
        float x = textureID - (y * BlockData.TextureAtlasWidthInBlocks);

        x *= BlockData.NormalizedBlockTextureSize;
        y *= BlockData.NormalizedBlockTextureSize;

        y = 1f - y - BlockData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + BlockData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + BlockData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + BlockData.NormalizedBlockTextureSize, y + BlockData.NormalizedBlockTextureSize));
    }

    private void BuildMesh()
    {
        if (!meshFilter)
        {
            meshFilter = GetComponent<MeshFilter>();
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        //Reset chunk lists for reuse
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        vertexIndex = 0;
    }
}
