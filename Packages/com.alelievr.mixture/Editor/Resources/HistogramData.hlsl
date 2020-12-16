// Keep in sync with HistogramView.cs buffer alloc
struct LuminanceData
{
    float minLuminance;
    float maxLuminance;
};

// Keep in sync with HistogramView.cs buffer alloc
struct HistogramData
{
    uint minBucketCount;
    uint maxBucketCount;
};

RWByteAddressBuffer                 _Histogram;
uint                                _HistogramBucketCount;
RWStructuredBuffer<LuminanceData>   _ImageLuminance;
RWStructuredBuffer<HistogramData>   _HistogramData;
