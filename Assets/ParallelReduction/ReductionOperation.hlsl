#ifndef __REDUCTION_OPERATION_HLSL__

#ifdef _INT
    #define Value int
#else //_FLOAT
    #define Value float
#endif

#if defined(_OP_SUM)
    #define Operation(a, b) (a + b)
#elif defined(_OP_MAX)
    #define Operation(a, b) max(a, b)
#elif defined(_OP_MIN)
    #define Operation(a, b) min(a, b)
#else
    #define Operation(a, b) (a + b)
#endif

#endif //__REDUCTION_OPERATION_HLSL__