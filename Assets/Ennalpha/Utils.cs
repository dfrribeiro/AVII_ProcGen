using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class Utils
{
    public static int terrainMaxHeight = 76; // max WorldSize
    public static int surfaceMinHeight = 32;
    public static int mountainMaxHeight = 100;
    public static int caveMinHeight = 1;
    public static int octaves = 6;
    public static float persistence = 0.7f;
    public static float offset = 32000; // evita simetria a zero : os resultados para x e -x são iguais
    public static Vector3[] directions3D = {
        Vector3.forward, Vector3.back,
        Vector3.left, Vector3.right,
        Vector3.up, Vector3.down
    };
    public static Vector3[] directions2D = directions3D.Take(4).ToArray();


    public static int GenerateSurfaceHeight(int x, int z)
    {
        return (int)MapToRange(surfaceMinHeight, terrainMaxHeight, 
            0, 1, 
            FractionalBrownianMotion(x * World.irregularitySurface, z * World.irregularitySurface, octaves, persistence));
    }
    
    public static int GenerateStoneHeight(int x, int z)
    {
        return (int)MapToRange(caveMinHeight, Math.Max(caveMinHeight+1, mountainMaxHeight), 
            0, 1, 
            FractionalBrownianMotion(x * 1.2f*World.irregularitySurface, z * 1.2f*World.irregularitySurface, octaves-1, persistence*2f));
    }

    public static float FractionalBrownianMotion3D(Vector3 pos, int octaves, float persistence, float irregularity)
    {
        return FractionalBrownianMotion3D(pos.x, pos.y, pos.z, octaves, persistence, irregularity);
    }
    
    public static float FractionalBrownianMotion3D(float x, float y, float z, int octaves, float persistence, float irregularity)
    {
        float xi = x * irregularity;
        float yi = y * irregularity;
        float zi = z * irregularity;
        float xy = FractionalBrownianMotion(xi, yi, octaves, persistence);
        float yx = FractionalBrownianMotion(yi, xi, octaves, persistence);
        float xz = FractionalBrownianMotion(xi, zi, octaves, persistence);
        float zx = FractionalBrownianMotion(zi, xi, octaves, persistence);
        float yz = FractionalBrownianMotion(yi, zi, octaves, persistence);
        float zy = FractionalBrownianMotion(zi, yi, octaves, persistence);
        return (xy + yx + xz + zx + yz + zy) / 6f; // mean
    }

    private static float t;
    public static void Update()
    {
        t += 1;
        var v = new Vector3(t, 1, 1);
        float m = FractionalBrownianMotion3D(v, 2, 0.3f, World.irregularityCave);
        float n = FractionalBrownianMotion3D(v, 1, 0.01f, 1.3f*World.irregularityCave);
        Grapher.Log(m, "cave", Color.gray);
        Grapher.Log(n, "gold", Color.yellow);
    }

    static float FractionalBrownianMotion(float x, float z, int octaves, float persistence)
    {
        float total = 0;
        float amplitude = 1;
        float frequency = 1;
        float maxValue = 0;
        
        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise((x+offset) * frequency, (z+offset) * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue;
    }

    static float MapToRange(float newmin, float newmax, float omin, float omax, float val)
    {
        float t = Mathf.InverseLerp(omin, omax, val);
        return Mathf.Lerp(newmin, newmax, t);
    }
}