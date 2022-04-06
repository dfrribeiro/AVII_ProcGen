using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material mat;
    public const int COLUMN_HEIGHT = 4; // chunkSize*columnHeight = worldHeight
    public const int CHUNK_SIZE = 16;
    public int renderDistance = 2;
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
                for (var y = 0; y < COLUMN_HEIGHT; y++)
                {
                    var origin = new Vector3(x*CHUNK_SIZE, y*CHUNK_SIZE, z*CHUNK_SIZE);
                    var c = new Chunk(origin, mat);
                    c.gameObject.transform.parent = gameObject.transform;
            
                    RegionData.Add(c.gameObject.name, c);
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
            x = (int) (playerPos.x / CHUNK_SIZE) * CHUNK_SIZE,
            y = (int) (playerPos.y / CHUNK_SIZE) * CHUNK_SIZE,
            z = (int) (playerPos.z / CHUNK_SIZE) * CHUNK_SIZE
        };
    }
    
    void RecursiveBuildWorld(Vector3 chunkPos, int distance)
    {
        BuildChunkAt(chunkPos);

        if (--distance == 0) return;
        var directions = new[]
        {
            Vector3.forward, Vector3.back,
            Vector3.left, Vector3.right,
            Vector3.up, Vector3.down
        };
        foreach (var dir in directions)
        {
            RecursiveBuildWorld(chunkPos + (dir * CHUNK_SIZE), distance);
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
        spawn.y = Utils.GenerateSurfaceHeight((int)spawn.x, (int)spawn.z)+100;
        player.transform.position = spawn;
        lastBuildPos = spawn;
        
        StartCoroutine(BuildWorld());
        //RecursiveBuildWorld(GetPlayerChunkOrigin(spawn), renderDistance);
        //DrawChunks();
        player.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        /*var movement = player.transform.position - lastBuildPos;
        if (movement.magnitude > CHUNK_SIZE) // jogador andou <chunkSize> blocos em relação à última atualização
        {
            lastBuildPos = GetPlayerChunkOrigin(player.transform.position);
            
            RecursiveBuildWorld(lastBuildPos, renderDistance);
            DrawChunks();
        }*/
    }
}
