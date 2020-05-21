using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
/// <summary>
/// Note: All view distance variables have been commented out as there is no player controller. 
/// I kept the code here, however, to show how easily it could be implemented
/// </summary>
public class World : MonoBehaviour
{
    public int seed;
    public Transform player;
    Vector2 playerChunkPosition;
    Vector2 lastPlayerChunkPosition;

    public int viewDistanceRadiusInChunks = 2;
    public Chunk chunkPrefab;
    public BlockType[] blockTypes;
    public int[,] blockMap = new int[BlockData.worldWidthInBlocks, BlockData.worldHeightInBlocks];
    Dictionary<Vector2, Chunk> chunks = new Dictionary<Vector2, Chunk>();

    //Biome variables (could be pulled into a biome class to change based on biome)
    //What is the max level of terrain
    int maxTerrainHeight = 120;

    //Constant to add to our heightmap received from perlin. Generally, this will be the level that the ground is flat
    int terrainSolidGroundHeight = 90;

    //Scale for manipulating the perlin for this biome
    float terrainScale = 0.25f;

    private void Start()
    {
        GenerateWorld();  
    }

    private void Update()
    {
        //View distance updating. Only required with active player controller
        /*playerChunkPosition = new Vector2(Mathf.FloorToInt(player.position.x) / BlockData.chunkWidth, Mathf.FloorToInt(player.position.y) / BlockData.chunkHeight);
        if (playerChunkPosition != lastPlayerChunkPosition)
        {
            lastPlayerChunkPosition = playerChunkPosition;
            //UpdateViewDistance();
        }*/
    }

    private void UpdateViewDistance()
    {
        //Check all chunks within view distance of player + buffer (+ 1 for centered player chunk and + 1 for chunks in either direction of where player came from)
        for (int x = (int)playerChunkPosition.x - viewDistanceRadiusInChunks - 1; x < (int)playerChunkPosition.x + viewDistanceRadiusInChunks + 2; x++)
        {
            for (int y = (int)playerChunkPosition.y - viewDistanceRadiusInChunks - 1; y < (int)playerChunkPosition.y + viewDistanceRadiusInChunks + 2; y++)
            {
                //Get distance in chunks to player for x/y
                float xDistanceToPlayer = Mathf.Abs(x - playerChunkPosition.x);
                float yDistanceToPlayer = Mathf.Abs(y - playerChunkPosition.y);

                //Then check if current checked chunk is in view distance. If so, enable it. If not, disable it.
                Chunk chunk;
                if (xDistanceToPlayer <= viewDistanceRadiusInChunks && yDistanceToPlayer <= viewDistanceRadiusInChunks)
                {
                    if (chunks.TryGetValue(new Vector2(x * BlockData.chunkWidth, y * BlockData.chunkHeight), out chunk))
                    {
                        chunk.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (chunks.TryGetValue(new Vector2(x * BlockData.chunkWidth, y * BlockData.chunkHeight), out chunk))
                    {
                        chunk.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    //Populates a map of IDs for the world
    private void GenerateWorld()
    {  
        for (int x = 0; x < BlockData.worldWidthInBlocks; x++)
        {
            for (int y = 0; y < BlockData.worldHeightInBlocks; y++)
            {
                int terrainHeight = Mathf.FloorToInt(maxTerrainHeight * Noise.GetPerlinNoise(new Vector2(seed + x, seed + y), 500, terrainScale)) + terrainSolidGroundHeight;
                int blockID = 0;

                //If bottom edge of world, unbreakable block
                if (y == 0)
                {
                    blockID = 1;
                }
                //Else if y is greater than perlin value, we have air
                else if (y > terrainHeight)
                {
                    blockID = 0;
                }
                //At approx terrain height, top dirt
                else if (y == terrainHeight) {
                    blockID = 4;
                }
                //For a few levels below the top, regular dirt
                else if (y < terrainHeight && y >= terrainHeight - 4)
                {
                    blockID = 3;
                }
                //Otherwise, stone
                else
                {
                    blockID = 2;
                }

                blockMap[x, y] = blockID;

                //Every ChunkWidth and ChunkHeight blocks we create a new chunk.
                if (x % BlockData.chunkWidth == 0 && y % BlockData.chunkHeight == 0)
                {
                    chunks.Add(new Vector2(x, y), CreateChunk(x, y));
                }
            }
        }
              
        player.position = new Vector3(BlockData.worldWidthInBlocks / 2, BlockData.worldHeightInBlocks - 50, -10);
        //playerChunkPosition = new Vector2(Mathf.FloorToInt(player.position.x) / BlockData.chunkWidth, Mathf.FloorToInt(player.position.y) / BlockData.chunkHeight);
        //lastPlayerChunkPosition = playerChunkPosition;
        GenerateCaves();
        GenerateChunks();
    }

    //Use cellular automata to generate a cave system below solid ground height
    private void GenerateCaves()
    {
        System.Random caveRandom = new System.Random(seed);

        //Determines at what random integer from 0-100 sets blocks to stone as opposed to air
        int caveFillThreshold = 68;

        //Loop across entire width of map
        for (int x = 0; x < BlockData.worldWidthInBlocks; x++)
        {
            //But only from just above bottom layer (what would be unbreakable) to top of solid ground
            for (int y = 1; y < terrainSolidGroundHeight; y++)
            {
                if (x == 0 || x == BlockData.worldWidthInBlocks - 1 || y == 1 || y == terrainSolidGroundHeight)
                {
                    blockMap[x, y] = 2;
                }
                else
                {
                    blockMap[x, y] = (caveRandom.Next(0, 100) < caveFillThreshold) ? 2 : 0;
                }
            }
        }

        SmoothCaveData(5);
    }

    //Smooth cave using cellular automata
    //Operates over x iterations where the threshold determines how many nearby walls there must be for the current cell to be a wall
    private void SmoothCaveData(int iterations, int threshold = 4)
    {
        for (int i = 0; i < iterations; i++)
        {
            //Loop across entire width of map
            for (int x = 0; x < BlockData.worldWidthInBlocks; x++)
            {
                //But only from just above bottom layer (what would be unbreakable) to top of solid ground
                for (int y = 1; y < terrainSolidGroundHeight; y++)
                {
                    //If current block has more than threshold walls surrounding it, make it a wall. Otherwise, air.
                    blockMap[x, y] = (GetSurroundingSolids(x, y) > threshold) ? 2 : 0;
                }
            }
        }
    }

    private int GetSurroundingSolids(int checkXPos, int checkYPos)
    {
        int wallCount = 0;
        //Check a 3/3 area with the center being checkXPos, checkYPos
        for (int x = checkXPos - 1; x <= checkXPos + 1; x++)
        {
            for (int y = checkYPos - 1; y <= checkYPos + 1; y++)
            {
                //Ensure current checked location is a valid spot in blockMap
                if (x >= 0 && x < BlockData.worldWidthInBlocks && y >= 0 && y < terrainSolidGroundHeight)
                {
                    //If we are at any point other than the origin
                    if (x != checkXPos || y != checkYPos)
                    {
                        //Add to the wall count if block is solid, or nothing if its air
                        wallCount += (blockMap[x, y] == 0) ? 0 : 1;
                    }
                }
                //If not, it's at an edge of the cave bounds and has to be a wall
                else
                {
                    wallCount += 1;
                }
            }
        }

        return wallCount;
    }

    //Generate meshes for each chunk
    //Experimenting with creating all chunks since we are in a finite world, then turn them on/off as needed
    //In an infinite world, this setup obviously wouldn't work and these would have to be generated when discovered by player
    private void GenerateChunks()
    {
        //Loop through all chunks and turn them off
        foreach (Chunk c in chunks.Values)
        {
            //Tell chunk to generate its mesh
            c.GenerateChunk();
            //c.gameObject.SetActive(false);
        }

        //All chunks have been generated and turned off. Now we turn the ones in view distance back on
       // UpdateViewDistance();
    }

    //Create a chunk at x/y 
    private Chunk CreateChunk(int xPos, int yPos)
    {
        Chunk newChunk = Instantiate(chunkPrefab, new Vector2(xPos, yPos), Quaternion.identity, transform);
        newChunk.name = "Chunk " + xPos + ", " + yPos;
        newChunk.chunkPosition = new Vector2(xPos, yPos);
        return newChunk;
    }

    //When player clicks a block to change it
    public void InteractWithChunk(Vector3 position, int id) 
    {
        //Get the position of the block in int space
        Vector2Int blockPos = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));

        //Ensure the request is valid
        if (blockPos.x >= 0 && blockPos.x < BlockData.worldWidthInBlocks && blockPos.y >= 0 && blockPos.y < BlockData.worldHeightInBlocks && id <= blockTypes.Length)
        {
            Vector2 blockChunk = new Vector2(blockPos.x / BlockData.chunkWidth * BlockData.chunkWidth, blockPos.y / BlockData.chunkHeight * BlockData.chunkHeight);

            //Then update the blockmap and tell the chunk to redraw itself
            //(can only place blocks on air or replace any block with air)
            if (blockMap[blockPos.x, blockPos.y] == 0 || id == 0)
            {
                blockMap[blockPos.x, blockPos.y] = id;
            }

            Chunk chunk;
            if (chunks.TryGetValue(blockChunk, out chunk))
            {
                chunk.GenerateChunk();
            }
        }
    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public int blockTextureID;
}
