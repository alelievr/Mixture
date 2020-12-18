// Keep in sync with HistogramView.cs buffer alloc
struct LuminanceData
{
    float minLuminance;
    float maxLuminance;
};

struct HistogramBucket
{
    uint luminance;
    uint r;
    uint g;
    uint b;
};

// Keep in sync with HistogramView.cs buffer alloc
struct HistogramData
{
    uint minBucketCount;
    uint maxBucketCount;
};

RWStructuredBuffer<HistogramBucket> _Histogram;
uint                                _HistogramBucketCount;
RWStructuredBuffer<LuminanceData>   _ImageLuminance;
RWStructuredBuffer<HistogramData>   _HistogramData;
