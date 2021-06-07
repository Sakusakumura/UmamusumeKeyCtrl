using System;
using System.Diagnostics;
using OpenCvSharp;
using umamusumeKeyCtl.Annotations;

namespace umamusumeKeyCtl.FeaturePointMethod
{
    public abstract class MatchingMethodBase
    {
        public DetectAndCompeteResult DetectAndCompute(Mat srcMat, [CanBeNull] InputArray mask)
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

        public DetectAndCompeteResult Detect(Mat srcMat, [CanBeNull] Mat mask)
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
        
        protected abstract DetectAndCompeteResult _DetectAndCompute(Mat srcMat, [CanBeNull] InputArray mask);
        protected abstract DetectAndCompeteResult _Detect(Mat srcMat, [CanBeNull] Mat mask);
    }
}