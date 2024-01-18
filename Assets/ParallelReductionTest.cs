using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[ExecuteAlways]
public class ParallelReductionTest : MonoBehaviour
{
    public ComputeShader reduction2DCS;
    public ComputeShader reduction1DCS;

    ParallelReduction parallelReduction = new ParallelReduction();

    const float Epsilon = 1000.0f;
    float GetPixelValue()
    {
        //return Mathf.FloorToInt(UnityEngine.Random.value * Epsilon) / Epsilon;
        return 1.0f;
    }

    public void Test(ParallelReduction reduction, bool is2D)
    {
        float sum = 0;
        int w = 4096;
        int h = 4096;

        if (is2D)
        {
            Texture2D dataBuffer = new Texture2D(w, h, GraphicsFormat.R32_SFloat, TextureCreationFlags.None);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    float v = GetPixelValue();
                    sum += v;
                    dataBuffer.SetPixel(i, j, new Color(v, v, v, 1.0f));
                }
            }
            dataBuffer.Apply();

            SpeedTimer stopwatch = new SpeedTimer("Total Tile");

            float gpuSum = reduction.ExecuteReduction(dataBuffer, ReductionOperation.ADD);

            stopwatch.StopAndLog();
            UnityEngine.Debug.Log(string.Format("GPU Difference Is {0}. And {1} CPU", Mathf.Abs(gpuSum - sum), Mathf.Abs(gpuSum - sum) <= 1.0f / Epsilon ? "==" : "!="));
        }
        else
        {
            float[,] data = new float[w, h];
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    float v = GetPixelValue();
                    sum += v;
                    data[i, j] = v;
                }
            }
            ComputeBuffer dataBuffer = new ComputeBuffer(w * h, sizeof(float), ComputeBufferType.Structured);
            dataBuffer.SetData(data);

            SpeedTimer stopwatch = new SpeedTimer("Total Tile");

            float gpuSum = reduction.ExecuteReduction(dataBuffer, ReductionOperation.ADD);

            stopwatch.StopAndLog();
            UnityEngine.Debug.Log(string.Format("GPU Difference Is {0}. And {1} CPU", Mathf.Abs(gpuSum - sum), Mathf.Abs(gpuSum - sum) <= 1.0f/ Epsilon ? "==" : "!="));
        }
    }

    void OnEnable()
    {
        parallelReduction.Create(reduction1DCS, reduction2DCS);
        Test(parallelReduction, true);
        Test(parallelReduction, false);
    }
}
