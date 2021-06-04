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
                Debug.Print(e.ToString());
                throw;
            }
        }
        protected abstract DetectAndCompeteResult _DetectAndCompute(Mat srcMat, [CanBeNull] InputArray mask);
    }
}