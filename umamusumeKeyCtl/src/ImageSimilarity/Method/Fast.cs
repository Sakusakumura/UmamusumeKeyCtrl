using System;
using System.Diagnostics;
using OpenCvSharp;

namespace umamusumeKeyCtl.ImageSimilarity.Method
{
    public class Fast : MatchingMethodBase
    {
        private FastFeatureDetector _fast;
        public int Threshold = 10;

        public Fast()
        {
            _fast = FastFeatureDetector.Create(Threshold);
        }

        protected override DetectAndComputeResult _DetectAndCompute(Mat srcMat, InputArray mask)
        {
            Debug.Print($"[{this.GetType().Name}] DetectAndCompete is not valid with FAST.");
            throw new NotSupportedException();
        }

        protected override KeyPoint[] _Detect(Mat srcMat, Mat mask)
        {
            return _fast.Detect(srcMat, mask);
        }

        protected override Mat _Compute(Mat srcMat, KeyPoint[] keyPoints, InputArray mask)
        {
            Debug.Print($"[{this.GetType().Name}] Compute is not valid with FAST.");
            throw new NotSupportedException();
        }

        public override void Dispose()
        {
            _fast?.Dispose();
        }
    }
}