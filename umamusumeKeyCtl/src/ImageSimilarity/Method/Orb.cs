using OpenCvSharp;
using umamusumeKeyCtl.Annotations;

namespace umamusumeKeyCtl.ImageSimilarity.Method
{
    public class Orb : MatchingMethodBase
    {
        private ORB _orb;
        public int edgeThreshold = 31;
        public int patchSize = 31;
        public int fastThreshold = 23;
        public int nFeatures = 300;

        public Orb()
        {
            _orb = ORB.Create(nFeatures: nFeatures, edgeThreshold: edgeThreshold, patchSize: patchSize, fastThreshold: fastThreshold);
        }

        protected override DetectAndComputeResult _DetectAndCompute(Mat srcMat, InputArray mask)
        {
            DetectAndComputeResult result = new ();

            _orb.DetectAndCompute(srcMat, mask, out result.KeyPoints, result.Mat);
            return result;
        }

        protected override KeyPoint[] _Detect(Mat srcMat, [CanBeNull] Mat mask = null)
        { 
            return _orb.Detect(srcMat, mask);
        }

        protected override Mat _Compute(Mat srcMat, KeyPoint[] keyPoints, InputArray mask)
        {
            Mat mat = new Mat();
            _orb.Compute(srcMat, ref keyPoints, mat);
            return mat;
        }

        public override void Dispose()
        {
            _orb?.Dispose();
        }
    }
}