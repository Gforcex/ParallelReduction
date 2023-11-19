using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[ExecuteAlways]
public class ParallelReductionTest : MonoBehaviour
{
    public ComputeShader reduction2DCS;
    public ComputeShader reduction1DCS;

    ParallelReduction parallelReduction = new ParallelReduction();

    public void Test(ParallelReduction reduction, bool is2D)
    {
        const float v = 1.0f;
        float sum = 0;
        int w = 2048;
        int h = 2048;

        if (is2D)
        {
            Texture2D dataBuffer = new Texture2D(w, h, TextureFormat.RFloat, false);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    sum += v;
                    dataBuffer.SetPixel(i, j, new Color(v, v, v, 1.0f));
                }
            }
            dataBuffer.Apply();

            SpeedTimer stopwatch = new SpeedTimer("Total Tile");

            float gpuSum = reduction.ExecuteReduction(dataBuffer);

            stopwatch.StopAndLog();
            UnityEngine.Debug.Log(string.Format("GPU Sum Is {0}. And {1} CPU", gpuSum, sum == gpuSum ? "==" : "!="));
        }
        else
        {
            float[,] data = new float[w, h];
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    sum += v;
                    data[i, j] = v;
                }
            }
            ComputeBuffer dataBuffer = new ComputeBuffer(w * h, sizeof(float), ComputeBufferType.Structured);
            dataBuffer.SetData(data);

            SpeedTimer stopwatch = new SpeedTimer("Total Tile");

            float gpuSum = reduction.ExecuteReduction(dataBuffer);

            stopwatch.StopAndLog();
            UnityEngine.Debug.Log(string.Format("GPU Sum Is {0}. And {1} CPU", gpuSum, sum == gpuSum ? "==" : "!="));
        }
    }

    void OnEnable()
    {
        parallelReduction.Create(reduction1DCS, reduction2DCS);
        Test(parallelReduction, true);
        //Test(parallelReduction, false);
    }
}
