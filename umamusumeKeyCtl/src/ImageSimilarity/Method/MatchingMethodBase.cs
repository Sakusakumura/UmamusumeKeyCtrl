using System;
using System.Diagnostics;
using OpenCvSharp;
using umamusumeKeyCtl.Annotations;

namespace umamusumeKeyCtl.ImageSimilarity.Method
{
    public abstract class MatchingMethodBase : IDisposable
    {
        public DetectAndComputeResult DetectAndCompute(Mat srcMat, [CanBeNull] InputArray mask)
        {
            try
            {
                return _DetectAndCompute(srcMat, mask);
            }
            catch (Exception e)
            {
                Debug.Write(e.ToString());
                throw;
            }
        }

        public KeyPoint[] Detect(Mat srcMat, [CanBeNull] Mat mask)
        {
            try
            {
                return _Detect(srcMat, mask);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                throw;
            }
        }

        public Mat Compute(Mat srcMat, KeyPoint[] keyPoints, [CanBeNull] InputArray mask)
        {
            try
            {
                return _Compute(srcMat, keyPoints, mask);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                throw;
            }
        }
        
        protected abstract DetectAndComputeResult _DetectAndCompute(Mat srcMat, [CanBeNull] InputArray mask);
        protected abstract KeyPoint[] _Detect(Mat srcMat, [CanBeNull] Mat inputArray);
        protected abstract Mat _Compute(Mat srcMat, KeyPoint[] keyPoints, [CanBeNull] InputArray mask);

        public abstract void Dispose();
    }
}