using System.Diagnostics;
using System.Threading;
using OpenCvSharp;
using umamusumeKeyCtl.Annotations;

namespace umamusumeKeyCtl.FeaturePointMethod
{
    public class Orb : MatchingMethodBase
    {
        public int edgeThreshold = 40;
        public int patchSize = 40;
        public int fastThreshold = 16;
        public int nFeatures = 200;

        protected override DetectAndCompeteResult _DetectAndCompute(Mat srcMat, InputArray mask)
        {
            using var orb = ORB.Create(nFeatures: nFeatures, edgeThreshold: edgeThreshold, patchSize: patchSize, fastThreshold: fastThreshold);

            DetectAndCompeteResult result = new ();

            orb.DetectAndCompute(srcMat, mask, out result.KeyPoints, result.Mat);
            return result;
        }

        protected override DetectAndCompeteResult _Detect(Mat srcMat, [CanBeNull] Mat mask = null)
        {
            using var orb = ORB.Create(nFeatures: nFeatures, edgeThreshold: edgeThreshold, patchSize: patchSize, fastThreshold: fastThreshold);

            DetectAndCompeteResult result = new ();

            var keyPoints = orb.Detect(srcMat, mask);

            result.KeyPoints = keyPoints;
            
            return result;
        }
    }
}