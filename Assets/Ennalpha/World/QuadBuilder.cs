using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadBuilder : MonoBehaviour
{
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
        
        Vector2 uv00 = new Vector2(0, 0);
        Vector2 uv01 = new Vector2(0, 1);
        Vector2 uv10 = new Vector2(1, 0);
        Vector2 uv11 = new Vector2(1, 1);
        Vector2[] uv = { uv11, uv01, uv00, uv10 };
        
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

        MeshRenderer mr = quad.AddComponent<MeshRenderer>();
        mr.material = mat;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Quad(CubeSide.LEFT);
        Quad(CubeSide.TOP);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
