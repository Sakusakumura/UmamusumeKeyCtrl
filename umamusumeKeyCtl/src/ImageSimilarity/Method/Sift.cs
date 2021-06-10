using OpenCvSharp;
using OpenCvSharp.Features2D;
using umamusumeKeyCtl.Annotations;

namespace umamusumeKeyCtl.ImageSimilarity.Method
{
    public class Sift : MatchingMethodBase
    {
        private SIFT _sift;

        public Sift()
        {
            _sift = SIFT.Create();
        }

        protected override DetectAndComputeResult _DetectAndCompute(Mat srcMat, InputArray mask)
        {
            DetectAndComputeResult result = new ();
            
            _sift.DetectAndCompute(srcMat, null, out result.KeyPoints, result.Mat);

            return result;
        }

        protected override KeyPoint[] _Detect(Mat srcMat, [CanBeNull] Mat mask = null)
        { 
            return _sift.Detect(srcMat, mask);
        }

        protected override Mat _Compute(Mat srcMat, KeyPoint[] keyPoints, InputArray mask)
        {
            Mat mat = new Mat();
            _sift.Compute(srcMat, ref keyPoints, mat);
            return mat;
        }

        public override void Dispose()
        {
            _sift?.Dispose();
        }
    }
}