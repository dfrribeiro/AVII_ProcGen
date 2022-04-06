using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Block
{
    public Block(BlockType b, Vector3 p, Chunk parent, Material m)
    {
        bType = b;
        position = p;
        mat = m;
        chunk = parent;
        isSolid = bType != BlockType.AIR && bType != BlockType.WATER && bType != BlockType.LAVA;
    }

    public enum BlockType
    {
        SNOW, STONE, STONEBRICK, WATER, LAVA,
        MOSSYSTONEBRICK, LEAVES, WOOD, SANDSTONE,
        GRASS, GRAVEL, MOSSYCOBBLE, SAND,
        COBBLE, MAGMA, DIRT, GOLDORE, OBSIDIAN,
        ANDESITE, BEDROCK, BLUEICE, BRICKS, PLANKS,
        AIR
    }
    private BlockType bType;
    private Vector3 position;
    private Chunk chunk;
    private Material mat;
    bool isSolid;

    // usar IndexOf
    private enum Texture
    {
        SNOW = 0,
        STONE = 1,
        STONEBRICK = 2,
        WATER = 3,
        LAVA = 4,
        MOSSYSTONEBRICK = 5,
        LEAVES = 6,
        WOODSIDE = 7,
        WOODTOP = 8,
        SANDSTONE = 9,
        GRASSTOP = 10,
        GRASSSIDE = 11,
        GRAVEL = 12,
        MOSSYCOBBLE = 13,
        SAND = 14,
        COBBLE = 15,
        MAGMA = 16,
        DIRT = 17,
        GOLDORE = 18,
        OBSIDIAN = 19,
        ANDESITE = 20,
        BEDROCK = 21,
        BLUEICE = 22,
        BRICKS = 23,
        PLANKS = 24
    }

    private static IEnumerable range = Enumerable.Range(0, 5);
    private static IEnumerable<Vector2> left_bottom_corners =
        from int v in range
        from int u in range
        select new Vector2(u/5f, v/5f);

    private Vector2[][] blockUVs = left_bottom_corners.Select(lbc =>
    {
        const float step = 1 / 5f;
        var rbc = new Vector2(lbc.x + step, lbc.y);
        var ltc = new Vector2(lbc.x, lbc.y + step);
        var rtc = new Vector2(rbc.x, ltc.y);
        return new[] { ltc, rtc, rbc, lbc }; // circular para rodar 90 graus
    }).ToArray();

    enum CubeSide { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK }

    private Vector3 GetFaceNormal(CubeSide side)
    {
        switch (side)
        {
            case CubeSide.BOTTOM:
                return Vector3.down;
            case CubeSide.TOP:
                return Vector3.up;
            case CubeSide.BACK:
                return Vector3.back;
            case CubeSide.FRONT:
                return Vector3.forward;
            case CubeSide.RIGHT:
                return Vector3.right;
            case CubeSide.LEFT:
                return Vector3.left;
        }

        return Vector3.zero;
    }

    private Vector3[] GetFaceVertices(CubeSide side)
    {
        Vector3 v0 = new Vector3(-.5f, -.5f,  .5f);
        Vector3 v1 = new Vector3( .5f, -.5f,  .5f);
        Vector3 v2 = new Vector3( .5f, -.5f, -.5f);
        Vector3 v3 = new Vector3(-.5f, -.5f, -.5f);
        Vector3 v4 = new Vector3(-.5f,  .5f,  .5f);
        Vector3 v5 = new Vector3( .5f,  .5f,  .5f);
        Vector3 v6 = new Vector3( .5f,  .5f, -.5f);
        Vector3 v7 = new Vector3(-.5f,  .5f, -.5f);

        switch (side)
        {
            default:
            case CubeSide.FRONT:
                return new []{v4, v5, v1, v0};
            case CubeSide.BACK:
                return new []{v6, v7, v3, v2};
            case CubeSide.LEFT:
                return new []{v7, v4, v0, v3};
            case CubeSide.RIGHT:
                return new []{v5, v6, v2, v1};
            case CubeSide.TOP:
                return new []{v7, v6, v5, v4};
            case CubeSide.BOTTOM:
                return new []{v0, v1, v2, v3};
        }
    }

    private Vector2[] GetBlockUV(Texture t)
    {
        return blockUVs[(int) t];
    }

    Texture GetTexture(CubeSide side)
    {
        switch (bType)
        {
            case BlockType.GRASS:
                switch (side)
                {
                    case CubeSide.TOP:
                        return Texture.GRASSTOP;
                    case CubeSide.BOTTOM:
                        return Texture.DIRT;
                    default:
                        return Texture.GRASSSIDE;
                }
            case BlockType.WOOD:
                switch (side)
                {
                    case CubeSide.TOP:
                    case CubeSide.BOTTOM:
                        return Texture.WOODTOP;
                    default:
                        return Texture.WOODSIDE;
                }
            case BlockType.STONE:
                return Texture.STONE;
            case BlockType.LAVA:
                return Texture.LAVA;
            case BlockType.SAND:
                return Texture.SAND;
            case BlockType.SANDSTONE:
                return Texture.SANDSTONE;
            case BlockType.SNOW:
                return Texture.SNOW;
            case BlockType.MAGMA:
                return Texture.MAGMA;
            case BlockType.WATER:
                return Texture.WATER;
            case BlockType.BRICKS:
                return Texture.BRICKS;
            case BlockType.COBBLE:
                return Texture.COBBLE;
            case BlockType.MOSSYCOBBLE:
                return Texture.MOSSYCOBBLE;
            case BlockType.GRAVEL:
                return Texture.GRAVEL;
            case BlockType.LEAVES:
                return Texture.LEAVES;
            case BlockType.PLANKS:
                return Texture.PLANKS;
            case BlockType.BEDROCK:
                return Texture.BEDROCK;
            case BlockType.BLUEICE:
                return Texture.BLUEICE;
            case BlockType.GOLDORE:
                return Texture.GOLDORE;
            case BlockType.ANDESITE:
                return Texture.ANDESITE;
            case BlockType.OBSIDIAN:
                return Texture.OBSIDIAN;
            case BlockType.STONEBRICK:
                return Texture.STONEBRICK;
            case BlockType.MOSSYSTONEBRICK:
                return Texture.MOSSYSTONEBRICK;
            default:
            case BlockType.DIRT:
                return Texture.DIRT;
        }
    }
    
    void CreateQuad(CubeSide side)
    {
        /*Vector2 uv00 = new Vector2(0, 0);
        Vector2 uv01 = new Vector2(0, 1);
        Vector2 uv10 = new Vector2(1, 0);
        Vector2 uv11 = new Vector2(1, 1);
        Vector2[] uv = { uv11, uv01, uv00, uv10 };*/
        Vector2[] uv = GetBlockUV(GetTexture(side));
        
        int[] triangles = { 3, 1, 0, 3, 2, 1 };

        Vector3[] vertices = GetFaceVertices(side);
        Vector3 n = GetFaceNormal(side);
        Vector3[] normals = {n, n, n, n};
        
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateBounds();

        GameObject quad = new GameObject("Quad" + side);
        quad.transform.position = position;
        quad.transform.parent = chunk.gameObject.transform;
        
        MeshFilter mf = quad.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        // MeshRenderer mr = quad.AddComponent<MeshRenderer>();
        // mr.material = mat;
    }
    
    bool HasSolidNeighbour(Vector3 pos)
    {
        Block[,,] chunkData;
        
        if (pos.x < 0 || pos.x >= World.CHUNK_SIZE ||
            pos.y < 0 || pos.y >= World.CHUNK_SIZE ||
            pos.z < 0 || pos.z >= World.CHUNK_SIZE)
        {
            // aceder a chunk vizinho
            Vector3 neighChunkPos = chunk.gameObject.transform.position
                                    + new Vector3(
                                        (pos.x-(int)position.x)*World.CHUNK_SIZE,
                                        (pos.y-(int)position.y)*World.CHUNK_SIZE, 
                                        (pos.z-(int)position.z)*World.CHUNK_SIZE);
            string chunkName = World.CreateChunkName(neighChunkPos);

            // convert to local index
            pos.x %= World.CHUNK_SIZE;
            pos.y %= World.CHUNK_SIZE;
            pos.z %= World.CHUNK_SIZE;
            
            Chunk neigh;
            if (World.RegionData.TryGetValue(chunkName, out neigh))
            {
                chunkData = neigh.chunkData;
            }
            else
            {
                return false;
            }
        }
        else
        {
            chunkData = chunk.chunkData; // mesmo chunk
        }
        
        try
        {
            Block b = chunkData[(int) pos.x, (int) pos.y, (int) pos.z];
            return b.isSolid;
        }
        catch (IndexOutOfRangeException ex)
        { }
        
        return false;
    }

    public void Draw()
    {
        if (bType == BlockType.AIR)
        {
            return;
        }
        
        foreach (CubeSide side in Enum.GetValues(typeof(CubeSide)))
        {
            if (!HasSolidNeighbour(position + GetFaceNormal(side)))
            {
                CreateQuad(side);
            }
        }
    }
}
