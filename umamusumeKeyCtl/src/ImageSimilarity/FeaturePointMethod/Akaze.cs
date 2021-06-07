using OpenCvSharp;

namespace umamusumeKeyCtl.FeaturePointMethod
{
    public class Akaze : MatchingMethodBase
    {
        protected override DetectAndCompeteResult _DetectAndCompute(Mat srcMat, InputArray mask)
        {
            using var akaze = AKAZE.Create();

            DetectAndCompeteResult result = new ();
            
            akaze.DetectAndCompute(srcMat, null, out result.KeyPoints, result.Mat);

            return result;
        }

        protected override DetectAndCompeteResult _Detect(Mat srcMat, Mat mask)
        {
            throw new System.NotImplementedException();
        }
    }
}