using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perlin : MonoBehaviour
{
    //private float t = 0;
    private float tt1, tt2, tt3;
    public float inc1 = 0.001f;
    public float inc2 = 0.005f;
    public float inc3 = 0.01f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        t += Time.deltaTime;
        var h = .5f * (Mathf.Cos(2 * t + 0.4f) + 1); // 0-1
        var h2 = .25f * (Mathf.Cos(4 * t) + 1); // 0-1
        var h3 = .125f * (Mathf.Cos(6 * t - 0.4f) + 1); // 0-1
        var hr = UnityEngine.Random.Range(0f, 1f);
        Grapher.Log(h+h2+h3, "cos", Color.green);
        Grapher.Log(hr, "random", Color.blue);
        */
        
        var hp1 = Mathf.PerlinNoise(tt1, 1); // mais suave
        var hp2 = 0.25f*Mathf.PerlinNoise(tt2, 1);
        var hp3 = 0.125f*Mathf.PerlinNoise(tt3, 1);
        
        tt1 += inc1;
        tt2 += inc2;
        tt3 += inc3;

        Grapher.Log(hp1+hp2+hp3, "perlin", Color.red);
    }
}
