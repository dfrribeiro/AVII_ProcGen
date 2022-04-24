using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class World : MonoBehaviour
{
    public Material mat;
    public PhysicMaterial physicMat;
    public const int ColumnHeight = 8; // chunkSize*columnHeight = worldHeight
    public const int ChunkSize = 16;
    public int renderDistance = 4;
    public static ConcurrentDictionary<string, Chunk> RegionData;
    public GameObject player;
    private Vector3 lastBuildPos;
    private List<string> toRemove;
    private bool drawing;

    public static float irregularitySurface = 0.005f; // inverse smooth
    public static float irregularityCave = 20*irregularitySurface;
    public static float irregularityOre = 1.3f*irregularityCave;
    

    public static string CreateChunkName(Vector3 v)
    {
        return "Chunk(" + (int)v.x + ", " + (int)v.y + ", " + (int)v.z + ")";
    }
    
    IEnumerator BuildWorld()
    {
        for (var x = 0; x < renderDistance; x++)
        {
            for (var z = 0; z < renderDistance; z++)
            {
                for (var y = 0; y < ColumnHeight; y++)
                {
                    var origin = new Vector3(x*ChunkSize, y*ChunkSize, z*ChunkSize);
                    var c = new Chunk(origin, mat, physicMat);
                    c.gameObject.transform.parent = gameObject.transform;
            
                    RegionData.TryAdd(c.gameObject.name, c);
                }

                yield return null;
            }
        }
        
        foreach (var pair in RegionData)
        {
            Chunk c = pair.Value;
            c.DrawChunk();
            // yield return null;
        }
    }
    
    Chunk BuildChunkAt(Vector3 chunkPos)
    {
        if (chunkPos.y < 0) { return null; }
        var chunkName = CreateChunkName(chunkPos);
        if (!RegionData.TryGetValue(chunkName, out var c))
        {
            var origin = new Vector3(chunkPos.x, chunkPos.y, chunkPos.z);
            c = new Chunk(origin, mat, physicMat);
            c.gameObject.transform.parent = gameObject.transform;
            
            RegionData.TryAdd(c.gameObject.name, c);
        } // already in dict
        
        return c;
    }
    
    IEnumerator DrawChunks()
    {
        drawing = true;
        foreach (var pair in RegionData)
        {
            Chunk c = pair.Value;
            if (c.status == Chunk.ChunkState.READY)
            {
                c.DrawChunk();
                yield return null;
            }

            var dist = Vector3.Distance(player.transform.position, c.gameObject.transform.position);
            if (dist > ChunkSize * renderDistance)
            {
                Debug.Log(dist);
                toRemove.Add(pair.Key);
            }
        }
        
        drawing = false;
    }

    static Vector3 GetPlayerChunkOrigin(Vector3 playerPos)
    {
        return new Vector3
        {
            x = Mathf.Floor(playerPos.x / ChunkSize) * ChunkSize,
            y = Mathf.Floor(playerPos.y / ChunkSize) * ChunkSize,
            z = Mathf.Floor(playerPos.z / ChunkSize) * ChunkSize
        };
    }
    
    IEnumerator RecursiveBuildWorld(Vector3 chunkPos, int distance)
    {
        BuildChunkAt(chunkPos);

        if (--distance == 0) yield break;
        foreach (var dir in Utils.directions3D)
        {
            StartCoroutine(RecursiveBuildWorld(chunkPos + (dir * ChunkSize), distance));
            yield return null;
        }
    }

    IEnumerator RemoveChunks()
    {
        for (int i = 0; i < toRemove.Count; i++)
        {
            string chunkName = toRemove[i];
            if (RegionData.TryGetValue(chunkName, out var c))
            {
                Destroy(c.gameObject);
                RegionData.TryRemove(chunkName, out c);
                yield return null;
            }
        }
    }
    
    IEnumerator LoadChunks()
    {
        Vector3 playerPos = player.transform.position;
        Vector3 playerChunkOrigin = GetPlayerChunkOrigin(playerPos);
        StartCoroutine(RecursiveBuildWorld(playerChunkOrigin, renderDistance+1));
        // flood-fill
        
        foreach (var pair in RegionData)
        {
            Chunk c = pair.Value;
            while (c.status == Chunk.ChunkState.WAIT)
            { } // not finished building
            
            var builtChunkCenter = c.gameObject.transform.position + Vector3.one * ChunkSize/2f;
            if (Vector3.Distance(playerPos, builtChunkCenter) <=
                ChunkSize * renderDistance) // chunk in range of player
            {
                c.DrawChunk();
                yield return null;
            }
            else if (c.status == Chunk.ChunkState.ACTIVE)
            {
                c.UnloadChunk();
                yield return null;
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        // player.GetComponent<Rigidbody>().useGravity = false;
        player.SetActive(false);
        
        RegionData = new ConcurrentDictionary<string, Chunk>();
        toRemove = new List<string>();

        var t = transform;
        t.position = Vector3.zero;
        t.rotation = Quaternion.identity;

        var spawn = player.transform.position;
        spawn.y = Mathf.Max(Utils.GenerateStoneHeight((int)spawn.x, (int)spawn.z), Utils.GenerateSurfaceHeight((int)spawn.x, (int)spawn.z))+1;
        player.transform.position = spawn;
        lastBuildPos = spawn;
        Chunk c = BuildChunkAt(GetPlayerChunkOrigin(spawn));
        c.DrawChunk();
        
        //StartCoroutine(BuildWorld());
        //StartCoroutine(RecursiveBuildWorld(GetPlayerChunkOrigin(spawn), renderDistance));
        //StartCoroutine(DrawChunks());
        //StartCoroutine(RemoveChunks());
        //StartCoroutine(LoadChunks());
        player.SetActive(true);
    }
    
    // Update is called once per frame
    private Vector3 movement = Vector3.positiveInfinity;
    void Update()
    {
        if (movement.magnitude > ChunkSize/2f) // jogador andou <chunkSize>/2 blocos em relação à última atualização
        {
            lastBuildPos = GetPlayerChunkOrigin(player.transform.position);
            
            //StartCoroutine(RecursiveBuildWorld(lastBuildPos, renderDistance));
            //StartCoroutine(DrawChunks());
            //StartCoroutine(RemoveChunks());
            StartCoroutine(LoadChunks());
        }
        movement = player.transform.position - lastBuildPos;
        //if (!drawing) StartCoroutine(DrawChunks());

        // Utils.Update();
    }
    
}
