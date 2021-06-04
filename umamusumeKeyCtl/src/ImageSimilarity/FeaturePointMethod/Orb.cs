using OpenCvSharp;

namespace umamusumeKeyCtl.FeaturePointMethod
{
    public class Orb : MatchingMethodBase
    {
        public int edgeThreshold = 40;
        public int patchSize = 40;
        public int fastThreshold = 20;
        public int nFeatures = 500;

        protected override DetectAndCompeteResult _DetectAndCompute(Mat srcMat, InputArray mask)
        {
            using var orb = ORB.Create(nFeatures: nFeatures, edgeThreshold: edgeThreshold, patchSize: patchSize, fastThreshold: fastThreshold);

            DetectAndCompeteResult result = new ();

            orb.DetectAndCompute(srcMat, mask, out result.KeyPoints, result.Mat);
            
            return result;
        }
    }
}