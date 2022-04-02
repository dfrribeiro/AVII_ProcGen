using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class QuadBuilder : MonoBehaviour
{
    /*
    static Vector2 Snow = new Vector2(0f, 0f) / 5;
    static Vector2 Stone = new Vector2(1f, 0f) / 5;
    static Vector2 StoneBrick = new Vector2(2f, 0f) / 5;
    static Vector2 Water = new Vector2(3f, 0f) / 5;
    static Vector2 Lava = new Vector2(4f, 0f) / 5;
    
    static Vector2 MossyStoneBrick = new Vector2(0f, 1f) / 5;
    static Vector2 Leaves = new Vector2(1f, 1f) / 5;
    static Vector2 WoodSide = new Vector2(2f, 1f) / 5;
    static Vector2 WoodTop = new Vector2(3f, 1f) / 5;
    static Vector2 Sandstone = new Vector2(4f, 1f) / 5;
    
    static Vector2 GrassTop = new Vector2(0f, 2f) / 5;
    static Vector2 GrassSide = new Vector2(1f, 2f) / 5;
    static Vector2 Gravel = new Vector2(2f, 2f) / 5;
    static Vector2 MossyCobble = new Vector2(3f, 2f) / 5;
    static Vector2 Sand = new Vector2(4f, 2f) / 5;

    static Vector2 Cobble = new Vector2(0f, 3f) / 5;
    static Vector2 Magma = new Vector2(1f, 3f) / 5;
    static Vector2 Dirt = new Vector2(2f, 3f) / 5;
    static Vector2 GoldOre = new Vector2(3f, 3f) / 5;
    static Vector2 Obsidian = new Vector2(4f, 3f) / 5;
    
    static Vector2 Andesite = new Vector2(0f, 4f) / 5;
    static Vector2 Bedrock = new Vector2(1f, 4f) / 5;
    static Vector2 BlueIce = new Vector2(2f, 4f) / 5;
    static Vector2 Bricks = new Vector2(3f, 4f) / 5;
    static Vector2 Planks = new Vector2(4f, 4f) / 5;
    */

    enum BlockType
    {
        SNOW, STONE, STONEBRICK, WATER, LAVA,
        MOSSYSTONEBRICK, LEAVES, WOOD, SANDSTONE,
        GRASS, GRAVEL, MOSSYCOBBLE, SAND,
        COBBLE, MAGMA, DIRT, GOLDORE, OBSIDIAN,
        ANDESITE, BEDROCK, BLUEICE, BRICKS, PLANKS
    }
    [SerializeField]
    BlockType bType;

    // usar IndexOf
    private enum Texture
    {
        SNOW = 0,
        STONE = 1,
        STONEBRICK = 2,
        WATER = 3,
        LAVA = 4,
        MOSSYSTONEBRICK = 5,
        LEAVES = 6,
        WOODSIDE = 7,
        WOODTOP = 8,
        SANDSTONE = 9,
        GRASSTOP = 10,
        GRASSSIDE = 11,
        GRAVEL = 12,
        MOSSYCOBBLE = 13,
        SAND = 14,
        COBBLE = 15,
        MAGMA = 16,
        DIRT = 17,
        GOLDORE = 18,
        OBSIDIAN = 19,
        ANDESITE = 20,
        BEDROCK = 21,
        BLUEICE = 22,
        BRICKS = 23,
        PLANKS = 24
    }

    private static IEnumerable range = Enumerable.Range(0, 5);
    private static IEnumerable<Vector2> left_bottom_corners =
        from int v in range
        from int u in range
        select new Vector2(u/5f, v/5f);

    private Vector2[][] blockUVs = left_bottom_corners.Select(lbc =>
    {
        const float step = 1 / 5f;
        var rbc = new Vector2(lbc.x + step, lbc.y);
        var ltc = new Vector2(lbc.x, lbc.y + step);
        var rtc = new Vector2(rbc.x, ltc.y);
        return new[] { ltc, rtc, rbc, lbc }; // circular para rodar 90 graus
    }).ToArray();

    enum CubeSide { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK }
    public Material mat;
    void Quad(CubeSide side)
    {
        Vector3 v0 = new Vector3(-.5f, -.5f,  .5f);
        Vector3 v1 = new Vector3( .5f, -.5f,  .5f);
        Vector3 v2 = new Vector3( .5f, -.5f, -.5f);
        Vector3 v3 = new Vector3(-.5f, -.5f, -.5f);
        Vector3 v4 = new Vector3(-.5f,  .5f,  .5f);
        Vector3 v5 = new Vector3( .5f,  .5f,  .5f);
        Vector3 v6 = new Vector3( .5f,  .5f, -.5f);
        Vector3 v7 = new Vector3(-.5f,  .5f, -.5f);
        Vector3[] vertices;
        
        /*Vector2 uv00 = new Vector2(0, 0);
        Vector2 uv01 = new Vector2(0, 1);
        Vector2 uv10 = new Vector2(1, 0);
        Vector2 uv11 = new Vector2(1, 1);
        Vector2[] uv = { uv11, uv01, uv00, uv10 };*/
        Vector2[] uv = GetBlockUV(GetTexture(side));

        Vector3 n;
        int[] triangles = { 3, 1, 0, 3, 2, 1 };
        switch (side)
        {
            default:
            case CubeSide.FRONT:
                vertices = new []{v4, v5, v1, v0};
                n = Vector3.forward;
                break;
            case CubeSide.BACK:
                vertices = new []{v6, v7, v3, v2};
                n = Vector3.back;
                break;
            case CubeSide.LEFT:
                vertices = new []{v7, v4, v0, v3};
                n = Vector3.left;
                break;
            case CubeSide.RIGHT:
                vertices = new []{v5, v6, v2, v1};
                n = Vector3.right;
                break;
            case CubeSide.TOP:
                vertices = new[] {v7, v6, v5, v4};
                n = Vector3.up;
                break;
            case CubeSide.BOTTOM:
                vertices = new []{v0, v1, v2, v3};
                n = Vector3.down;
                break;
        }
        Vector3[] normals = {n, n, n, n};
        
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.uv = uv;

        GameObject quad = new GameObject("Quad" + side);
        quad.transform.parent = gameObject.transform;
        
        MeshFilter mf = quad.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        // MeshRenderer mr = quad.AddComponent<MeshRenderer>();
        // mr.material = mat;
    }

    private Vector2[] GetBlockUV(Texture t)
    {
        return blockUVs[(int) t];
    }

    Texture GetTexture(CubeSide side)
    {
        switch (bType)
        {
            case BlockType.GRASS:
                switch (side)
                {
                    case CubeSide.TOP:
                        return Texture.GRASSTOP;
                    case CubeSide.BOTTOM:
                        return Texture.DIRT;
                    default:
                        return Texture.GRASSSIDE;
                }
            case BlockType.WOOD:
                switch (side)
                {
                    case CubeSide.TOP:
                    case CubeSide.BOTTOM:
                        return Texture.WOODTOP;
                    default:
                        return Texture.WOODSIDE;
                }
            case BlockType.STONE:
                return Texture.STONE;
            case BlockType.LAVA:
                return Texture.LAVA;
            case BlockType.SAND:
                return Texture.SAND;
            case BlockType.SANDSTONE:
                return Texture.SANDSTONE;
            case BlockType.SNOW:
                return Texture.SNOW;
            case BlockType.MAGMA:
                return Texture.MAGMA;
            case BlockType.WATER:
                return Texture.WATER;
            case BlockType.BRICKS:
                return Texture.BRICKS;
            case BlockType.COBBLE:
                return Texture.COBBLE;
            case BlockType.MOSSYCOBBLE:
                return Texture.MOSSYCOBBLE;
            case BlockType.GRAVEL:
                return Texture.GRAVEL;
            case BlockType.LEAVES:
                return Texture.LEAVES;
            case BlockType.PLANKS:
                return Texture.PLANKS;
            case BlockType.BEDROCK:
                return Texture.BEDROCK;
            case BlockType.BLUEICE:
                return Texture.BLUEICE;
            case BlockType.GOLDORE:
                return Texture.GOLDORE;
            case BlockType.ANDESITE:
                return Texture.ANDESITE;
            case BlockType.OBSIDIAN:
                return Texture.OBSIDIAN;
            case BlockType.STONEBRICK:
                return Texture.STONEBRICK;
            case BlockType.MOSSYSTONEBRICK:
                return Texture.MOSSYSTONEBRICK;
            default:
            case BlockType.DIRT:
                return Texture.DIRT;
        }
    }

    void CombineQuads()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
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
        renderer.material = mat;
        
        foreach (Transform quad in transform)
        {
            Destroy(quad.gameObject);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        /*foreach (Vector2 lbc in left_bottom_corners)
        {
            Debug.Log(lbc.x*5 + " " + lbc.y*5);
        }*/
        
        foreach (CubeSide side in Enum.GetValues(typeof(CubeSide)))
        {
            Quad(side);
        }
        CombineQuads();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
