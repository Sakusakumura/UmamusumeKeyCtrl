using OpenCvSharp;
using OpenCvSharp.Features2D;

namespace umamusumeKeyCtl.FeaturePointMethod
{
    public class Sift : MatchingMethodBase
    {
        protected override DetectAndCompeteResult _DetectAndCompute(Mat srcMat, InputArray mask)
        {
            using var sift = SIFT.Create();

            DetectAndCompeteResult result = new ();
            
            sift.DetectAndCompute(srcMat, null, out result.KeyPoints, result.Mat);

            return result;
        }
    }
}