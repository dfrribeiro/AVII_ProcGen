using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material mat;
    public static int columnHeight = 4;
    public static int chunkSize = 16;
    public static int worldSize = 2;
    public static Dictionary<string, Chunk> regionData;

    public static string CreateChunkName(Vector3 v)
    {
        return "Chunk(" + (int)v.x + ", " + (int)v.y + ", " + (int)v.z + ")";
    }

    IEnumerator BuildChunkColumn()
    {
        for (int i = 0; i < columnHeight; i++)
        {
            Vector3 origin = new Vector3(transform.position.x, i * chunkSize, transform.position.z);
            Chunk c = new Chunk(origin, mat);
            c.gameObject.transform.parent = gameObject.transform;
            
            regionData.Add(c.gameObject.name, c);
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
        
        StartCoroutine(BuildChunkColumn());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
