using System;
using OpenCvSharp;

namespace umamusumeKeyCtl.ImageSimilarity.Method
{
    public class DetectAndComputeResult : IDisposable
    {
        public KeyPoint[] KeyPoints;
        public Mat Mat;

        public DetectAndComputeResult()
        {
            KeyPoints = new KeyPoint[0];
            Mat = new Mat();
        }

        public DetectAndComputeResult(KeyPoint[] keyPoints, Mat mat)
        {
            KeyPoints = keyPoints;
            Mat = mat;
        }

        public void Dispose()
        {
            Mat?.Dispose();
        }
    }
}