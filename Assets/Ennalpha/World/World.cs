using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material mat;
    public static int columnHeight = 4;
    public static int chunkSize = 16;
    public static int renderDistance = 2;
    public static Dictionary<string, Chunk> regionData;

    public static string CreateChunkName(Vector3 v)
    {
        return "Chunk(" + (int)v.x + ", " + (int)v.y + ", " + (int)v.z + ")";
    }

    IEnumerator BuildWorld()
    {
        for (int x = 0; x < renderDistance; x++)
        {
            for (int z = 0; z < renderDistance; z++)
            {
                for (int y = 0; y < columnHeight; y++)
                {
                    Vector3 origin = new Vector3(x*chunkSize, y*chunkSize, z*chunkSize);
                    Chunk c = new Chunk(origin, mat);
                    c.gameObject.transform.parent = gameObject.transform;
            
                    regionData.Add(c.gameObject.name, c);
                }
            }
        }
        
        foreach (KeyValuePair<string, Chunk> c in regionData)
        {
            c.Value.DrawChunk();
            yield return null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        regionData = new Dictionary<string, Chunk>();
        
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        
        StartCoroutine(BuildWorld());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
