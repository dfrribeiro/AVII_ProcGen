using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class Chunk
{
    Material material;
    // public int blocksPerFrame = 100;
    public Block[,,] chunkData;
    public GameObject gameObject;
    public float caveRatio = 0.42f;
    public float oreRatio = 0.32f;
    public enum ChunkState { WAIT, READY, ACTIVE, SLEEP }
    public ChunkState status = ChunkState.WAIT;
    public const int WaterLevel = 50;
    public PhysicMaterial physicMat;

    public Chunk(Vector3 pos, Material mat, PhysicMaterial physMat)
    {
        gameObject = new GameObject(World.CreateChunkName(pos));
        gameObject.transform.position = pos;
        material = mat;
        physicMat = physMat;
        
        BuildChunk();
    }

    void BuildChunk()
    {
        chunkData = new Block[World.ChunkSize, World.ChunkSize, World.ChunkSize];
        
        var range = Enumerable.Range(0, World.ChunkSize);
        var allPositions =
            from x in range
            from y in range
            from z in range
            select new Vector3(x, y, z);
        
        foreach (var pos in allPositions.ToList())
        {
            Vector3 worldPos = gameObject.transform.position + pos;
            
            chunkData[(int)pos.x, (int)pos.y, (int)pos.z] =
                new Block(PickBlockType(worldPos), pos, this, material);
        }

        status = ChunkState.READY;
    }

    public void DrawChunk()
    {
        if (status == ChunkState.READY)
        {
            foreach (var oldQuadFilter in gameObject.GetComponentsInChildren<MeshFilter>())
            {
                GameObject.DestroyImmediate(oldQuadFilter);
            }
            foreach (var b in chunkData)
            {
                b.Draw();
            }

            CombineQuads();

            if (gameObject.TryGetComponent<MeshCollider>(out var prevCollider))
            {
                GameObject.DestroyImmediate(prevCollider);
            }
            MeshCollider collider = gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = gameObject.GetComponent<MeshFilter>().mesh;
            collider.material = physicMat;
        }
        // if sleep, gameObject is ready but inactive
        
        status = ChunkState.ACTIVE;
        gameObject.SetActive(true);
    }

    public void UnloadChunk()
    {
        status = ChunkState.SLEEP;
        gameObject.SetActive(false);
    }

    Block.BlockType PickBlockType(Vector3 worldPos)
    {
        int x = (int) worldPos.x;
        int y = (int) worldPos.y;
        int z = (int) worldPos.z;
        int surfaceLevel = Utils.GenerateSurfaceHeight(x, z);
        int stoneLevel = Utils.GenerateStoneHeight(x, z);

        if (y == 0)
            return Block.BlockType.BEDROCK;
        if (y <= stoneLevel)
        {
            if (Utils.FractionalBrownianMotion3D(worldPos, 2, 0.3f, World.irregularityCave) < caveRatio)
            {
                // cave generation
                return Block.BlockType.AIR;
            }
            
            if (y <= WaterLevel && Utils.FractionalBrownianMotion3D(worldPos, 1, 0.01f, World.irregularityOre) < oreRatio)
            {
                return Block.BlockType.GOLDORE;
            }
            return Block.BlockType.STONE;
        }
        
        if (y == surfaceLevel)
        {
            // TODO bool localMaxima = CheckNeighbours(x, z, (neighbourHeight) => neighbourHeight > surfaceLevel);
            // TODO save maxima, if on top, chance to spawn tree instead of air
            // TODO spawn leaves around tree top
            return Block.BlockType.GRASS;
        }
        if (y < surfaceLevel)
        {
            return Block.BlockType.DIRT;
        }
        // TODO if (worldPos.y < WaterLevel) return Block.BlockType.WATER;
        //problem: compile meshes com agua
        return Block.BlockType.AIR;
    }
    
    void CombineQuads()
    {
        MeshFilter[] filters = gameObject.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[filters.Length];
        
        for (var i = 0; i < filters.Length; i++)
        {
            combine[i].mesh = filters[i].sharedMesh;
            combine[i].transform = filters[i].transform.localToWorldMatrix;
        }

        if (gameObject.TryGetComponent<MeshFilter>(out var prevFilter))
        {
            GameObject.DestroyImmediate(prevFilter);
        }
        MeshFilter newFilter = gameObject.AddComponent<MeshFilter>();
        newFilter.mesh = new Mesh();
        
        newFilter.mesh.indexFormat = IndexFormat.UInt32;
        newFilter.mesh.CombineMeshes(combine);

        if (gameObject.TryGetComponent<MeshRenderer>(out var prevRenderer))
        {
            GameObject.DestroyImmediate(prevRenderer);
        }
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = material;
        
        foreach (Transform quad in gameObject.transform)
        {
            GameObject.Destroy(quad.gameObject);
        }
    }
    
    private static bool CheckNeighbours(int x, int z, Func<int, bool> failCondition)
    {

        foreach (var dir in Utils.directions2D)
        {
            int neighborX = x + (int)dir.x;
            int neighborZ = z + (int)dir.z;

            /*if (neighborX < 0 || neighborX >= World.ChunkSize || neighborZ < 0 ||
                neighborZ >= World.ChunkSize)
            {
                continue;
            }*/

            if (failCondition(Utils.GenerateSurfaceHeight(neighborX, neighborZ)))
            {
                return false;
            }
        }
        return true;
    }
}
