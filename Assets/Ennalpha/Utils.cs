using System;
using UnityEngine;
using UnityEditor;

public class Utils
{
    public static float irregularitySurface = 0.005f; // inverse smooth
    public static float irregularityCave = 10*irregularitySurface;
    public static int terrainMaxHeight = 64; // max WorldSize
    public static int terrainMinHeight = 0;
    public static int octaves = 6;
    public static float persistence = 0.7f;
    public static float offset = 32000; // evita simetria a zero : os resultados para x e -x são iguais
    

    public static int GenerateSurfaceHeight(int x, int z)
    {
        return (int)MapToRange(terrainMinHeight, terrainMaxHeight, 
            0, 1, 
            FractionalBrownianMotion(x * irregularitySurface, z * irregularitySurface, octaves, persistence));
    }
    
    public static int GenerateStoneHeight(int x, int z)
    {
        return (int)MapToRange(terrainMinHeight, Math.Max(terrainMinHeight+1, terrainMaxHeight-5), 
            0, 1, 
            FractionalBrownianMotion(x * 1.5f*irregularitySurface, z * 1.5f*irregularitySurface, octaves-1, persistence*3f));
    }

    public static float FractionalBrownianMotion3D(Vector3 pos, int octaves, float persistence)
    {
        return FractionalBrownianMotion3D(pos.x, pos.y, pos.z, octaves, persistence);
    }
    
    public static float FractionalBrownianMotion3D(float x, float y, float z, int octaves, float persistence)
    {
        float xi = x * irregularityCave;
        float yi = y * irregularityCave;
        float zi = z * irregularityCave;
        float xy = FractionalBrownianMotion(xi, yi, octaves, persistence);
        float yx = FractionalBrownianMotion(yi, xi, octaves, persistence);
        float xz = FractionalBrownianMotion(xi, zi, octaves, persistence);
        float zx = FractionalBrownianMotion(zi, xi, octaves, persistence);
        float yz = FractionalBrownianMotion(yi, zi, octaves, persistence);
        float zy = FractionalBrownianMotion(zi, yi, octaves, persistence);
        return (xy + yx + xz + zx + yz + zy) / 6f; // mean
    }

    /*void Update()
    {
        t += inc;
        float n = FractionalBrownianMotion(t, 4, 0.6f);
        Grapher.Log(n, "fBm", Color.yellow);
    }*/

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