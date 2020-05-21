using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Backdrop : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    Vector3[] vertices = new Vector3[4];
    int[] triangles = new int[6];

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        BuildBackdrop();
    }

    private void BuildBackdrop()
    {
        vertices[0] = new Vector3(0, 0);
        vertices[1] = new Vector3(0, BlockData.worldHeightInBlocks);
        vertices[2] = new Vector3(BlockData.worldWidthInBlocks, 0);
        vertices[3] = new Vector3(BlockData.worldWidthInBlocks, BlockData.worldHeightInBlocks);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 2;
        triangles[4] = 1;
        triangles[5] = 3;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}
