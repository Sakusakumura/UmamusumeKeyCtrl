using System;
using System.Diagnostics;
using OpenCvSharp;
using OpenCvSharp.XFeatures2D;
using umamusumeKeyCtl.Annotations;

namespace umamusumeKeyCtl.ImageSimilarity.Method
{
    public class Brief : MatchingMethodBase
    {
        private BriefDescriptorExtractor _brief;

        public Brief()
        {
            _brief = BriefDescriptorExtractor.Create();
        }

        protected override DetectAndComputeResult _DetectAndCompute(Mat srcMat, InputArray mask)
        {
            Debug.Print($"[{this.GetType().Name}] DetectAndCompute is not supported in BRIEF");
            throw new NotSupportedException();
        }

        protected override KeyPoint[] _Detect(Mat srcMat, [CanBeNull] Mat mask = null)
        { 
            Debug.Print($"[{this.GetType().Name}] Detect is not supported in BRIEF");
            throw new NotSupportedException();
        }

        protected override Mat _Compute(Mat srcMat, KeyPoint[] keyPoints, InputArray mask)
        {
            Mat mat = new Mat();
            _brief.Compute(srcMat, ref keyPoints, mat);
            return mat;
        }

        public override void Dispose()
        {
            _brief?.Dispose();
        }
    }
}