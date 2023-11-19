using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ParallelReduction 
{
    private ComputeShader reduction2DCS;
    private ComputeShader reduction1DCS;

    public void Create(ComputeShader CS1d, ComputeShader CS2d)
    {
        reduction1DCS = CS1d;
        reduction2DCS = CS2d;
    }

    public float ExecuteReduction(Texture2D dataMaps)
    {
        if (dataMaps == null || dataMaps.width * dataMaps.height <= 0 || reduction2DCS == null) return 0;

        int reductionKernel = reduction2DCS.FindKernel("Reduction");
        int fristReductionKernel = reductionKernel;

        uint groupThreadNumX, groupThreadNumY, gropuThreadNumZ;
        reduction2DCS.GetKernelThreadGroupSizes(reductionKernel, out groupThreadNumX, out groupThreadNumY, out gropuThreadNumZ);
        int groupProcessNumX = (int)groupThreadNumX * 2;
        int groupProcessNumY = (int)groupThreadNumY * 2;

        int restX = dataMaps.width;
        int restY = dataMaps.height;

        int groupCountX = (int)Mathf.Ceil((float)restX / (float)groupProcessNumX);
        int groupCountY = (int)Mathf.Ceil((float)restY / (float)groupProcessNumY);

        RenderTexture resTex = new RenderTexture(groupCountX, groupCountY, 0, RenderTextureFormat.RFloat);
        resTex.filterMode = FilterMode.Point;
        resTex.wrapMode = TextureWrapMode.Clamp;
        resTex.autoGenerateMips = false;
        resTex.enableRandomWrite = true;
        resTex.Create();

        reduction2DCS.SetInts("_InputDataSize", new int[] { restX, restY });

        reduction2DCS.SetTexture(fristReductionKernel, "_InputData", dataMaps);
        reduction2DCS.SetTexture(fristReductionKernel, "_OutputData", resTex);
        reduction2DCS.Dispatch(fristReductionKernel, groupCountX, groupCountY, 1);

        if (groupCountX > 1 || groupCountY > 1)
        {
            RenderTexture swapTex = new RenderTexture((int)Mathf.Ceil((float)groupCountX / (float)groupProcessNumX), 
                (int)Mathf.Ceil((float)groupCountY / (float)groupProcessNumY), 
                0, 
                RenderTextureFormat.RFloat);
            swapTex.filterMode = FilterMode.Point;
            swapTex.wrapMode = TextureWrapMode.Clamp;
            swapTex.autoGenerateMips = false;
            swapTex.enableRandomWrite = true;
            swapTex.Create();

            RenderTexture inputTex = swapTex;
            RenderTexture tempTex = null;

            while (groupCountX > 1 || groupCountY > 1)
            {
                tempTex = inputTex;
                inputTex = resTex;
                resTex = tempTex;

                reduction2DCS.SetInts("_InputDataSize", new int[] { groupCountX, groupCountY });

                groupCountX = (int)Mathf.Ceil((float)groupCountX / (float)groupProcessNumX);
                groupCountY = (int)Mathf.Ceil((float)groupCountY / (float)groupProcessNumY);

                reduction2DCS.SetTexture(reductionKernel, "_InputData", inputTex);
                reduction2DCS.SetTexture(reductionKernel, "_OutputData", resTex);
                reduction2DCS.Dispatch(reductionKernel, groupCountX, groupCountY, 1);
            }

            inputTex.Release();
        }


        Texture2D sumTex = new Texture2D(1, 1, TextureFormat.RFloat, false);
        sumTex.filterMode = FilterMode.Point;
        sumTex.hideFlags = HideFlags.DontSave;

#if UNITY_EDITOR
        SpeedTimer stopwatch = new SpeedTimer("Read Back GPU Data");
#endif

        RenderTexture oldRT = RenderTexture.active;
        RenderTexture.active = resTex;
        sumTex.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
        sumTex.Apply();

        float resValue = sumTex.GetPixel(0, 0).r;
        GameObject.DestroyImmediate(sumTex);
        RenderTexture.active = oldRT;

#if UNITY_EDITOR
        stopwatch.StopAndLog();
#endif

        resTex.Release();
        return resValue;
    }

    public float ExecuteReduction2(Texture2D dataMaps)
    {
        if (dataMaps == null || dataMaps.width * dataMaps.height <= 0 || reduction2DCS == null) return 0;

        int reductionKernel = reduction2DCS.FindKernel("Reduction2");
        int fristReductionKernel = reductionKernel;

        uint groupThreadNumX, groupThreadNumY, gropuThreadNumZ;
        reduction2DCS.GetKernelThreadGroupSizes(reductionKernel, out groupThreadNumX, out groupThreadNumY, out gropuThreadNumZ);
        int groupProcessNumX = (int)groupThreadNumX;
        int groupProcessNumY = (int)groupThreadNumY;

        int restX = dataMaps.width;
        int restY = dataMaps.height;
        int groupCountX;
        int groupCountY;

        Texture inputTex = dataMaps;
        Texture tempTex = null;
        RenderTexture resTex = null;
        bool frist = true;
        List<RenderTexture> rts = new List<RenderTexture>();
        while (restX > 1 || restY > 1)
        {
            if(!frist)
            {
                tempTex = inputTex;
                inputTex = resTex;
                resTex = tempTex as RenderTexture;
            }
            frist = !frist;

            restX = inputTex.width; 
            restY = inputTex.height;

            groupCountX = (int)Mathf.Ceil((float)restX / (float)groupProcessNumX);
            groupCountY = (int)Mathf.Ceil((float)restY / (float)groupProcessNumY);

            resTex = new RenderTexture(groupCountX, groupCountY, 0, RenderTextureFormat.RFloat);
            resTex.filterMode = FilterMode.Point;
            resTex.wrapMode = TextureWrapMode.Clamp;
            resTex.autoGenerateMips = false;
            resTex.enableRandomWrite = true;
            resTex.Create();
            rts.Add(resTex);

            reduction2DCS.SetInts("_InputDataSize", new int[] { restX, restY });
            reduction2DCS.SetTexture(reductionKernel, "_InputData", inputTex);
            reduction2DCS.SetTexture(reductionKernel, "_OutputData", resTex);
            reduction2DCS.Dispatch(reductionKernel, groupCountX, groupCountY, 1);
        }

        Texture2D sumTex = new Texture2D(1, 1, TextureFormat.RFloat, false);
        sumTex.filterMode = FilterMode.Point;
        sumTex.hideFlags = HideFlags.DontSave;

#if UNITY_EDITOR
        SpeedTimer stopwatch = new SpeedTimer("Read Back GPU Data");
#endif

        RenderTexture oldRT = RenderTexture.active;
        RenderTexture.active = inputTex as RenderTexture;
        sumTex.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
        sumTex.Apply();

        float resValue = sumTex.GetPixel(0, 0).r;
        GameObject.DestroyImmediate(sumTex);
        RenderTexture.active = oldRT;

#if UNITY_EDITOR
        stopwatch.StopAndLog();
#endif

        foreach(RenderTexture rt in rts)
            rt.Release();

        return resValue;
    }

    public float ExecuteReduction(ComputeBuffer dataBuffer)
    {
        if (dataBuffer == null || dataBuffer.count <= 0 || reduction1DCS == null) return 0;

        int reductionKernel = reduction1DCS.FindKernel("Reduction");
        int fristReductionKernel = reductionKernel;

        uint groupThreadNumX, groupThreadNumY, gropuThreadNumZ;
        reduction1DCS.GetKernelThreadGroupSizes(reductionKernel, out groupThreadNumX, out groupThreadNumY, out gropuThreadNumZ);

        int groupProcessNum = (int)groupThreadNumX * 2;
        int totalCount = dataBuffer.count;
        int groupCount = (int)Mathf.Ceil((float)totalCount / (float)groupProcessNum);


        //ComputeBufferType.Raw -> ByteAddressBuffer :ComputeBufferType.Raw
        //Buffer and RWBuffer support only 32-bit formats: float, int and uint.

        ComputeBuffer resBuffer = new ComputeBuffer(groupCount, sizeof(float), ComputeBufferType.Structured);
        reduction1DCS.SetInt("_InputDataSize", totalCount);

        reduction1DCS.SetBuffer(fristReductionKernel, "_InputData", dataBuffer);
        reduction1DCS.SetBuffer(fristReductionKernel, "_OutputData", resBuffer);
        reduction1DCS.Dispatch(fristReductionKernel, groupCount, 1, 1);

        if (groupCount > 1)
        {
            ComputeBuffer swapBuffer = new ComputeBuffer((int)Mathf.Ceil((float)groupCount / (float)groupProcessNum), sizeof(float), ComputeBufferType.Structured);
            ComputeBuffer inputBuffer = swapBuffer;
            ComputeBuffer tempBuffer = null;

            while (groupCount > 1)
            {
                tempBuffer = inputBuffer;
                inputBuffer = resBuffer;
                resBuffer = tempBuffer;

                reduction1DCS.SetInt("_InputDataSize", groupCount);

                groupCount = (int)Mathf.Ceil((float)groupCount / (float)groupProcessNum);

                reduction1DCS.SetBuffer(reductionKernel, "_InputData", inputBuffer);
                reduction1DCS.SetBuffer(reductionKernel, "_OutputData", resBuffer);
                reduction1DCS.Dispatch(reductionKernel, groupCount, 1, 1);
            }

            inputBuffer.Release();
        }

#if UNITY_EDITOR
        SpeedTimer stopwatch = new SpeedTimer("Read Back GPU Data");
#endif

        float[] res = new float[1];
        resBuffer.GetData(res);
        float resValue = res[0];

#if UNITY_EDITOR
        stopwatch.StopAndLog();
#endif

        resBuffer.Release();
        return resValue;
    }
}
