using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material mat;
    public static int columnHeight = 8; // chunkSize*columnHeight = worldHeight
    public static int chunkSize = 16;
    public static int renderDistance = 2;
    public static Dictionary<string, Chunk> RegionData;
    public GameObject player;
    private Vector3 lastBuildPos;

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
                for (var y = 0; y < columnHeight; y++)
                {
                    var origin = new Vector3(x*chunkSize, y*chunkSize, z*chunkSize);
                    var c = new Chunk(origin, mat);
                    c.gameObject.transform.parent = gameObject.transform;
            
                    RegionData.Add(c.gameObject.name, c);
                }

                yield return null;
            }
        }
    }
    
    void BuildChunkAt(Vector3 chunkPos)
    {
        var chunkName = CreateChunkName(chunkPos);
        if (RegionData.TryGetValue(chunkName, out var c)) return; // already in dict
        
        var origin = new Vector3(chunkPos.x*chunkSize, chunkPos.y*chunkSize, chunkPos.z*chunkSize);
        c = new Chunk(origin, mat);
        c.gameObject.transform.parent = gameObject.transform;
            
        RegionData.Add(c.gameObject.name, c);
    }
    
    void DrawChunks() // coroutine no futuro
    {
        foreach (var pair in RegionData)
        {
            Chunk c = pair.Value;
            if (c.status == Chunk.ChunkState.DRAW) c.DrawChunk();
            // yield return null;
        }
    }

    static Vector3 GetPlayerChunkOrigin(Vector3 playerPos)
    {
        return new Vector3
        {
            x = (int) (playerPos.x / chunkSize) * chunkSize,
            y = (int) (playerPos.y / chunkSize) * chunkSize,
            z = (int) (playerPos.z / chunkSize) * chunkSize
        };
    }
    
    void RecursiveBuildWorld(Vector3 chunkPos, int distance)
    {
        BuildChunkAt(chunkPos);
        if (--distance == 0) return;

        foreach (var dir in new [] {
                         Vector3.back, Vector3.forward,
                         Vector3.left, Vector3.right,
                         Vector3.up, Vector3.down
                     })
        {
            RecursiveBuildWorld(chunkPos+(dir*chunkSize), distance-1);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        player.SetActive(false);
        
        RegionData = new Dictionary<string, Chunk>();

        var t = transform;
        t.position = Vector3.zero;
        t.rotation = Quaternion.identity;

        var spawn = player.transform.position;
        spawn.y = Utils.GenerateSurfaceHeight((int)spawn.x, (int)spawn.z)+1;
        player.transform.position = spawn;
        lastBuildPos = spawn;
        
        //StartCoroutine(BuildWorld());
        RecursiveBuildWorld(GetPlayerChunkOrigin(spawn), renderDistance);
        DrawChunks();
        player.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        var movement = player.transform.position - lastBuildPos;
        if (movement.magnitude > chunkSize) // jogador andou <chunkSize> blocos em relação à última atualização
        {
            lastBuildPos = player.transform.position;
            
            RecursiveBuildWorld(lastBuildPos, renderDistance);
            DrawChunks();
        }
    }
}
