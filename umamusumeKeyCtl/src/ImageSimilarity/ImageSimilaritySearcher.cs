using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Flann;
using umamusumeKeyCtl.Factory;
using umamusumeKeyCtl.FeaturePointMethod;
using Size = OpenCvSharp.Size;

namespace umamusumeKeyCtl
{
    public class ImageSimilaritySearcher
    {
        public static double TemplateMatch(Bitmap srcImage, Bitmap targetImage)
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
        
        public static DetectAndCompeteResult DetectAndCompete(Bitmap source, MatchingFeaturePointMethod method, Rect[] maskAreas = null)
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
                var result = new FeaturePointMethodFactory().Create(method).DetectAndCompute(mat, null);
                
                mask?.Dispose();
                
                return result;
            }
            catch (Exception e)
            {   
                Debug.Print(e.ToString());
                throw;
            }
        }
        
        public static MatchingResult FeaturePointsMatching(Bitmap srcImage, Bitmap tgtImage,
            MatchingFeaturePointMethod method, Rect[] sourceImageMask = null, Rect[] targetImageMask = null)
        {
            return FeaturePointsMatching(DetectAndCompete(srcImage, method, sourceImageMask), DetectAndCompete(tgtImage, method, targetImageMask));
        }
        
        public static MatchingResult FeaturePointsMatching(Bitmap srcImage, DetectAndCompeteResult tgtResult,
            MatchingFeaturePointMethod method, Rect[] sourceImageMask = null)
        {
            return FeaturePointsMatching(DetectAndCompete(srcImage, method, sourceImageMask), tgtResult);
        }

        public static MatchingResult FeaturePointsMatching(DetectAndCompeteResult srcResult, Bitmap tgtImage,
            MatchingFeaturePointMethod method, Rect[] targetImageMask = null)
        {
            return FeaturePointsMatching(srcResult, DetectAndCompete(tgtImage, method, targetImageMask));
        }

        public static MatchingResult FeaturePointsMatching(DetectAndCompeteResult srcResult, DetectAndCompeteResult targetResult)
        {
            try
            {
                if (srcResult.KeyPoints.Length <= 2 || targetResult.KeyPoints.Length <= 2)
                {
                    Debug.Print(
                        $"Not enough keypoints.\nsourceKeypoints.Length: {srcResult.KeyPoints.Length}, targetKeypoints.Length: {targetResult.KeyPoints.Length}");
                    return MatchingResult.Fail;
                }

                using var inputParams = new IndexParams();
                inputParams.SetAlgorithm(1);
                inputParams.SetInt("trees", 5);

                using var searchParams = new SearchParams();
                searchParams.SetInt("checks", 50);

                using var flannBasedMatcher = new FlannBasedMatcher(inputParams, searchParams);

                using var convertedSD = new Mat();
                srcResult.Mat.ConvertTo(convertedSD, MatType.CV_32F);

                using var convertedTD = new Mat();
                targetResult.Mat.ConvertTo(convertedTD, MatType.CV_32F);

                var matches = flannBasedMatcher.KnnMatch(convertedSD, convertedTD, 2);

                var goods = new List<DMatch>();

                foreach (var match in matches)
                {
                    if (match[0].Distance < 0.7 * match[1].Distance)
                    {
                        goods.Add(match[0]);
                    }
                }

                if (goods.Count < 80)
                {
                    Debug.Print($"Not enough match: {goods.Count}");
                    return MatchingResult.Fail;
                }

                Debug.Print($"Enough match: {goods.Count}");

                return MatchingResult.SuccessWithScore(goods.ToArray());
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                throw;
            }
        }
    }
}