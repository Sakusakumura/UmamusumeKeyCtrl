using OpenCvSharp;

namespace umamusumeKeyCtl.ImageSimilarity.Method
{
    public class Akaze : MatchingMethodBase
    {
        private AKAZE _akaze;

        public Akaze()
        {
            _akaze = AKAZE.Create();
        }

        protected override DetectAndComputeResult _DetectAndCompute(Mat srcMat, InputArray mask)
        {
            DetectAndComputeResult result = new ();
            
            _akaze.DetectAndCompute(srcMat, mask, out result.KeyPoints, result.Mat);

            return result;
        }

        protected override KeyPoint[] _Detect(Mat srcMat, Mat inputArray)
        {
            return _akaze.Detect(srcMat, inputArray);
        }

        protected override Mat _Compute(Mat srcMat, KeyPoint[] keyPoints, InputArray mask)
        {
            var mat = new Mat();
            _akaze.Compute(srcMat, ref keyPoints, mat);
            return mat;
        }

        public override void Dispose()
        {
            _akaze?.Dispose();
        }
    }
}