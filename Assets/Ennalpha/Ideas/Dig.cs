using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dig : MonoBehaviour
{
    public Camera cam;
    public Material mat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            
            if (Physics.Raycast(ray, out var hit, Mathf.Sqrt(World.ChunkSize)))
            {
                var camPos = cam.transform;
                Debug.DrawRay(camPos.position, camPos.forward * (hit.distance * 10), Color.red, 3f);
                Transform chunkHit = hit.transform;
                string chunkName = chunkHit.name;
                Vector3Int globalHitPos = Vector3Int.FloorToInt(hit.point); // TODO possível bug por arranjar

                if (World.RegionData.TryGetValue(chunkName, out var chunkObj))
                {
                    // converter global para local
                    Vector3Int localHitPos =
                        globalHitPos - Vector3Int.FloorToInt(chunkObj.gameObject.transform.position);

                    Debug.Log(globalHitPos + " -> " + chunkName + "" + localHitPos);

                    try
                    {
                        chunkObj.chunkData[localHitPos.x, localHitPos.y, localHitPos.z] =
                            new Block(Block.BlockType.AIR, localHitPos, chunkObj, mat);
                        
                        // reload chunk
                        chunkObj.UnloadChunk();
                        chunkObj.status = Chunk.ChunkState.READY;
                        chunkObj.DrawChunk();
                    }
                    catch (System.IndexOutOfRangeException ex)
                    {
                        Debug.Log("Wrong chunk raycast hit");
                    }
                    // Problema: o chunk é um conjunto: tem de se determinar o bloco pela posição onde a camera interseta
                }
            }
        }
    }
}
