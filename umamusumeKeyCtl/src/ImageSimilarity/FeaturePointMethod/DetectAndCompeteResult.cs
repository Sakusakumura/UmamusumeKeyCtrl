using System;
using OpenCvSharp;

namespace umamusumeKeyCtl.FeaturePointMethod
{
    public class DetectAndCompeteResult : IDisposable
    {
        public KeyPoint[] KeyPoints;
        public Mat Mat;

        public DetectAndCompeteResult()
        {
            Mat = new Mat();
        }

        public void Dispose()
        {
            Mat?.Dispose();
        }
    }
}