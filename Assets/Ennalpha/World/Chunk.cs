using System.Linq;
using UnityEngine;

public class Chunk
{
    Material material;
    // public int blocksPerFrame = 100;
    public Block[,,] chunkData;
    public GameObject gameObject;

    public Chunk(Vector3 pos, Material mat)
    {
        gameObject = new GameObject(World.CreateChunkName(pos));
        gameObject.transform.position = pos;
        material = mat;
        
        BuildChunk();
    }
    
    void BuildChunk()
    {
        chunkData = new Block[World.chunkSize, World.chunkSize, World.chunkSize];
        
        var range = Enumerable.Range(0, World.chunkSize);
        var allPositions =
            from x in range
            from y in range
            from z in range
            select new Vector3(x, y, z);
        
        foreach (var pos in allPositions.ToList())
        {
            chunkData[(int)pos.x, (int)pos.y, (int)pos.z] =
                new Block(pickBlockType(), pos, this, material);
        }
    }

    public void DrawChunk()
    {
        foreach (var b in chunkData)
        {
            b.Draw();
        }
        CombineQuads();
    }

    Block.BlockType pickBlockType()
    {
        float rd = Random.Range(0f, 1f);
        if (rd <= .2f)
            return Block.BlockType.ANDESITE;
        if (rd <= .4f)
            return Block.BlockType.GRAVEL;
        //if (rd <= .6f)
        //    return Block.BlockType.AIR;
        return Block.BlockType.STONE;
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

        MeshFilter newFilter = gameObject.AddComponent<MeshFilter>();
        newFilter.mesh = new Mesh();

        newFilter.mesh.CombineMeshes(combine);

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = material;
        
        foreach (Transform quad in gameObject.transform)
        {
            GameObject.Destroy(quad.gameObject);
        }
    }
}
