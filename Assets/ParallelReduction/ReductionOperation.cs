using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ReductionOperation
{
    ADD, EQUAL, MAX, MIN
};

public enum ReductionValue
{
    FLOAT, INT
}

public static class ReductionOperationUtil
{
    public static void SetReductionOperation(ComputeShader cs, ReductionOperation operation, ReductionValue valueType)
    {
        string[] operationKeyword = new string[]
        {
            "_OP_SUM", "_OP_MAX", "_OP_MIN"
        };

        string[] valueTypeKeyword = new string[]
        {
            "_FLOAT", "_INT"
        };

        cs.EnableKeyword(operationKeyword[(int)operation]);
        //cs.EnableKeyword(valueTypeKeyword[(int)valueType]);
    }
}