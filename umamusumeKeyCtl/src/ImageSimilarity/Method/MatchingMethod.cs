using System;
using OpenCvSharp;
using umamusumeKeyCtl.Annotations;

namespace umamusumeKeyCtl.ImageSimilarity.Method
{
    public class MatchingMethod : IDisposable
    {
        public readonly MatchingMethodBase DetectorMethod;
        public readonly MatchingMethodBase DescriberMethod;

        public MatchingMethod(MatchingMethodBase detectorMethod, MatchingMethodBase describerMethod)
        {
            DetectorMethod = detectorMethod;
            DescriberMethod = describerMethod;
        }

        public DetectAndComputeResult DetectAndCompute(Mat mat, [CanBeNull] Mat mask = null)
        {
            var keyPoints = DetectorMethod.Detect(mat, mask);
            var descriptor = DescriberMethod.Compute(mat, keyPoints, mask);
            
            return new DetectAndComputeResult(keyPoints, descriptor);
        }

        public KeyPoint[] Detect(Mat srcMat, [CanBeNull] Mat mask) => DetectorMethod.Detect(srcMat, mask);
        public Mat Compute(Mat srcMat, KeyPoint[] keyPoints, [CanBeNull] InputArray mask) => DescriberMethod.Compute(srcMat, keyPoints, mask);

        public void Dispose()
        {
            DetectorMethod?.Dispose();
            DescriberMethod?.Dispose();
        }
    }
}