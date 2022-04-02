using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldBuilder : MonoBehaviour
{
    public GameObject block;
    public int size; // x = y = z
    
    //public float targetFrameRate = 60.0f;
    public int blocksPerFrame = 100;
    //float maximumTimePerFrame;
    private int counter;

    IEnumerator BuildWorld()
    {
        Vector3 playerPos = Vector3.zero; // TODO
        /*for (int z = 0; z < size; z++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    CreateVoxel(x, y, z)
                }
            }
            yield return null;
        }*/
        
        /*
        
        var sum = 0;
        for (int r = 0; r < size*2; r++)
        {
            // dimensions = 3
            int combinations = (int) Math.Pow(3, r);
            
            foreach (var pos in result.Skip(sum).Take(combinations))
            {
                CreateVoxel(pos);
                yield return null;
            }
            sum += combinations;
            //yield return null;
        }
        r = 0 : 0,0
        r = 1 : 1,0,0 -1,0,0 0,1,0 0,-1,0 0,0,1 0,0,-1
        r = 2 : 
        */
        var range = Enumerable.Range(0, size);
        var allPositions =
            from x in range
            from y in range
            from z in range
            select new Vector3(x, y, z);

        foreach (var radiusGroup in allPositions.GroupBy(pos => pos.x + pos.y + pos.z).ToList())
        {
            foreach (var pos in radiusGroup.ToList())
            {
                CreateVoxel(pos);
                
                // repetir até ao fim da frame
                if (++counter > blocksPerFrame) // Time.realtimeSinceStartup > timeStamp + maximumTimePerFrame
                {
                    counter = 0;
                    yield return null; // avança frame
                    // timeStamp = Time.realtimeSinceStartup;
                }
            }
            // yield return new WaitForFixedUpdate();
            // yield return new WaitForSeconds(1); DEBUG
        }
    }

    void CreateVoxel(Vector3 pos)
    {
        GameObject cube = Instantiate(block, pos, Quaternion.identity);
        cube.transform.parent = gameObject.transform;
        cube.name = String.Concat("x", pos.x, "_y", pos.y, "_z", pos.z);

        if (Convert.ToBoolean(Random.Range(0, 1)))
        {
            cube.GetComponent<MeshRenderer>().material.color = Color.red;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        //maximumTimePerFrame = 1.0f / targetFrameRate;
        
        StartCoroutine(BuildWorld());
    }
}
