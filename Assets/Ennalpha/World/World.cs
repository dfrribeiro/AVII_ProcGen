using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material mat;
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
                    var c = new Chunk(origin, mat);
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
    
    void BuildChunkAt(Vector3 chunkPos)
    {
        var chunkName = CreateChunkName(chunkPos);
        if (RegionData.TryGetValue(chunkName, out var c)) return; // already in dict
        
        var origin = new Vector3(chunkPos.x, chunkPos.y, chunkPos.z);
        c = new Chunk(origin, mat);
        c.gameObject.transform.parent = gameObject.transform;
            
        RegionData.TryAdd(c.gameObject.name, c);
    }
    
    IEnumerator DrawChunks()
    {
        drawing = true;
        foreach (var pair in RegionData)
        {
            Chunk c = pair.Value;
            if (c.status == Chunk.ChunkState.DRAW)
            {
                c.DrawChunk();
                yield return null;
            }

            if (Vector3.Distance(player.transform.position,
                    c.gameObject.transform.position) > ChunkSize * renderDistance)
            {
                toRemove.Add(pair.Key);
            }
        }

        StartCoroutine(RemoveChunks());
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
        yield return null;

        if (--distance == 0) yield break;
        foreach (var dir in Utils.directions3D)
        {
            StartCoroutine(
                RecursiveBuildWorld(chunkPos + (dir * ChunkSize), distance)
                );
        }
    }

    IEnumerator RemoveChunks()
    {
        foreach (var chunkName in toRemove)
        {
            if (RegionData.TryGetValue(chunkName, out var c))
            {
                Destroy(c.gameObject);
                RegionData.TryRemove(chunkName, out c);
                yield return null;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        player.SetActive(false);
        
        RegionData = new ConcurrentDictionary<string, Chunk>();
        toRemove = new List<string>();

        var t = transform;
        t.position = Vector3.zero;
        t.rotation = Quaternion.identity;

        var spawn = player.transform.position;
        spawn.y = Utils.GenerateSurfaceHeight((int)spawn.x, (int)spawn.z)+1;
        player.transform.position = spawn;
        lastBuildPos = spawn;
        
        //StartCoroutine(BuildWorld());
        StartCoroutine(RecursiveBuildWorld(GetPlayerChunkOrigin(spawn), renderDistance));
        StartCoroutine(DrawChunks());
        player.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        var movement = player.transform.position - lastBuildPos;
        if (movement.magnitude > ChunkSize) // jogador andou <chunkSize> blocos em relação à última atualização
        {
            lastBuildPos = GetPlayerChunkOrigin(player.transform.position);
            
            StartCoroutine(RecursiveBuildWorld(lastBuildPos, renderDistance));
            StartCoroutine(DrawChunks());
        }
        if (!drawing) StartCoroutine(DrawChunks());
        // TODO melhorar este comportamento

        // Utils.Update();
    }
    
}
