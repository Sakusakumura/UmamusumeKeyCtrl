using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Flann;
using umamusumeKeyCtl.Annotations;
using umamusumeKeyCtl.ImageSimilarity.Factory;
using umamusumeKeyCtl.ImageSimilarity.Method;
using umamusumeKeyCtl.Properties;
using Size = OpenCvSharp.Size;

namespace umamusumeKeyCtl
{
    public class ImageSimilaritySearcher : IDisposable
    {
        // defines are in https://github.com/opencv/opencv/blob/383559c2285baaf3df8cf0088072d104451a30ce/modules/flann/include/opencv2/flann/defines.h#L68
        private const int FLANN_INDEX_LINEAR = 0;
        private const int FLANN_INDEX_KDTREE = 1;
        private const int FLANN_INDEX_KMEANS = 2;
        private const int FLANN_INDEX_COMPOSITE = 3;
        private const int FLANN_INDEX_KDTREE_SINGLE = 4;
        private const int FLANN_INDEX_HIERARCHICAL = 5;
        private const int FLANN_INDEX_LSH = 6;
        private const int FLANN_INDEX_SAVED = 254;
        private const int FLANN_INDEX_AUTOTUNED = 255;

        private DetectorMethod _detectorMethod;
        private DescriptorMethod _descriptorMethod;
        private MatchingMethod _matchingMethod;

        public int Knn_K { get; set; } = 2;

        public ImageSimilaritySearcher(DetectorMethod detectorMethod, DescriptorMethod descriptorMethod, int knn_k = 2)
        {
            Knn_K = knn_k;
            _detectorMethod = detectorMethod;
            _descriptorMethod = descriptorMethod;
            _matchingMethod = new FeaturePointMatchingMethodFactory().Create(_detectorMethod, _descriptorMethod);
        }
        
        public double TemplateMatch(Bitmap srcImage, Bitmap targetImage)
        {
            // 検索対象の画像とテンプレート画像
            using (Mat src = BitmapConverter.ToMat(srcImage))
            using (Mat target = BitmapConverter.ToMat(targetImage))
            using (Mat _result = new Mat())
            {

                // テンプレートマッチ
                Cv2.MatchTemplate(src, target, _result, TemplateMatchModes.CCoeffNormed);

                // 類似度が最大/最小となる画素の位置を調べる
                OpenCvSharp.Point minloc, maxloc;
                double minval, maxval;
                Cv2.MinMaxLoc(_result, out minval, out maxval, out minloc, out maxloc);

                // しきい値で判断
                var threshold = 0.9;
                if (maxval >= threshold)
                {

                    // 最も見つかった場所に赤枠を表示
                    Rect rect = new Rect(maxloc.X, maxloc.Y, target.Width, target.Height);
                    Cv2.Rectangle(src, rect, new OpenCvSharp.Scalar(0, 0, 255), 2);

                    // ウィンドウに画像を表示
                    Cv2.ImShow("template1_show", src);

                    return maxval;
                }
                else
                {
                    return -1;
                }
            }
        }
        
        public DetectAndComputeResult DetectAndCompete(Bitmap source, [CanBeNull] Rect[] maskAreas = null)
        {
            try
            {
                // Convert bitmap to mat.
                using var mat = BitmapConverter.ToMat(source);
                
                // Create mask.
                Mat mask = null;

                if (maskAreas != null)
                {
                    mask = new Mat(new Size(mat.Width, mat.Height), MatType.CV_8UC4, Scalar.Black);
                
                    foreach (var maskArea in maskAreas)
                    {
                        Cv2.Rectangle(mask, maskArea, Scalar.White, thickness: -1);
                    }
                    
                    Cv2.BitwiseAnd(mat, mask, mat);
                }

                // Get result.
                var result = _matchingMethod.DetectAndCompute(mat, mask);
                
                mask?.Dispose();
                
                return result;
            }
            catch (Exception e)
            {   
                Debug.Print(e.ToString());
                throw;
            }
        }
        
        public DetectAndComputeResult DetectAndCompete(Mat source, [CanBeNull] Rect[] maskAreas = null)
        {
            try
            {
                // Create mask.
                Mat mask = null;

                if (maskAreas != null)
                {
                    mask = new Mat(new Size(source.Width, source.Height), MatType.CV_8UC4, Scalar.Black);
                
                    foreach (var maskArea in maskAreas)
                    {
                        Cv2.Rectangle(mask, maskArea, Scalar.White, thickness: -1);
                    }
                    
                    Cv2.BitwiseAnd(source, mask, source);
                }

                // Get result.
                var result = _matchingMethod.DetectAndCompute(source, mask);
                
                mask?.Dispose();
                
                return result;
            }
            catch (Exception e)
            {   
                Debug.Print(e.ToString());
                throw;
            }
        }

        public KeyPoint[] Detect(Bitmap source, [CanBeNull] Rect[] maskAreas = null)
        {
            try
            {
                // Convert bitmap to mat.
                using var mat = BitmapConverter.ToMat(source);
                
                // Create mask.
                if (maskAreas != null)
                {
                    using Mat mask = new Mat(new Size(mat.Width, mat.Height), MatType.CV_8UC4, Scalar.Black);

                    foreach (var maskArea in maskAreas)
                    {
                        Cv2.Rectangle(mask, maskArea, Scalar.White, thickness: -1);
                    }

                    Cv2.BitwiseAnd(mat, mask, mat);
                    
                    return _matchingMethod.Detect(mat, mask);
                }

                // Get result.
                var result = _matchingMethod.Detect(mat, null);

                return result;
            }
            catch (Exception e)
            {   
                Debug.Write(e);
                throw;
            }
        }

        public MatchingResult KnnMatch(DetectAndComputeResult srcResult, DetectAndComputeResult targetResult)
        {
            try
            {
                if (srcResult.KeyPoints.Length <= 2 || targetResult.KeyPoints.Length <= 2)
                {
                    //Debug.Print($"Not enough keypoints.\nsourceKeypoints.Length: {srcResult.KeyPoints.Length}, targetKeypoints.Length: {targetResult.KeyPoints.Length}");
                    return MatchingResult.FailWithScore(new ());
                }

                using var indexParams = new IndexParams();
                indexParams.SetAlgorithm(FLANN_INDEX_LSH);
                indexParams.SetInt("table_number", 6);
                indexParams.SetInt("key_size", 12);
                indexParams.SetInt("multi_probe_level", 1);

                using var searchParams = new SearchParams();
                searchParams.SetInt("checks", 50);

                using var flannBasedMatcher = new FlannBasedMatcher(indexParams, searchParams);

                if (!srcResult.Mat.IsContinuous() || !targetResult.Mat.IsContinuous() || srcResult.Mat.Rows < 2 || targetResult.Mat.Rows < 2)
                {
                    return MatchingResult.FailWithScore(new ());
                }

                var matches = flannBasedMatcher.KnnMatch(srcResult.Mat, targetResult.Mat, Knn_K);
                
                var goods = new List<DMatch>();
                foreach (var match in matches)
                {
                    var flag = false;
                    for (int i = 0; i < match.Length - 1; i++)
                    {
                        if (match[i].Distance < 0.7 * match[i + 1].Distance)
                        {
                            flag = true;
                        }
                    }
                    
                    if (flag)
                    {
                        goods.Add(match[0]);
                    }
                }

                if (_detectorMethod == DetectorMethod.FAST)
                {
                    if (goods.Count < srcResult.KeyPoints.Length * 0.5)
                    {
                        //Debug.Print($"Not enough match: {goods.Count}");
                        return MatchingResult.FailWithScore(goods);
                    }
                }
                else
                {
                    if (goods.Count < srcResult.KeyPoints.Length * 0.35 && goods.Count < 60)
                    {
                        //Debug.Print($"Not enough match: {goods.Count}");
                        return MatchingResult.FailWithScore(goods);
                    }
                }

                //Debug.Print($"Enough match: {goods.Count}");

                return MatchingResult.SuccessWithScore(goods).WithKnnMatchResult(matches);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                throw;
            }
        }

        public void Dispose()
        {
            _matchingMethod?.Dispose();
        }
    }
}