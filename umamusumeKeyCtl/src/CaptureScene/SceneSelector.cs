using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using umamusumeKeyCtl.Factory;
using umamusumeKeyCtl.FeaturePointMethod;

namespace umamusumeKeyCtl.CaptureScene
{
    public class SceneSelector
    {
        private bool _printResult;
        public event Action<Mat> ResultPrinted; 
        public event Action<(Mat Src, Mat Tgt)> SrcTgtImgPrinted; 

        public SceneSelector(bool printResult)
        {
            this._printResult = printResult;
        }

        public async Task<List<MatchingResult>> SelectScene(Bitmap capturedImage)
        {
            // TODO: Returns results for debug.
            return await GetMatchingResults(capturedImage);
        }

        private async Task<List<MatchingResult>> GetMatchingResults(Bitmap capturedImage)
        {
            try
            {
                var rootTask = new List<Task<MatchingResult>>();
                
                // Create task that gets matching result.
                foreach (var scene in SceneHolder.Instance.Scenes.ToArray())
                {
                    var cloned = ((Bitmap) capturedImage.Clone()).PerformScale(300).PerformGrayScale();
                        
                    rootTask.Add(GetMatchingResult(cloned, scene, scene.ScrappedImage.FeaturePoints));
                    
                    if (_printResult)
                    {
                        Mat src = new Mat();
                        Cv2.DrawKeypoints(BitmapConverter.ToMat(scene.ScrappedImage.Image), scene.ScrappedImage.FeaturePoints.KeyPoints, src, Scalar.Green);
                        var targetMask = scene.ScrappedImage.SamplingAreas.Select(val => val.ScrapArea.ToOpenCvRect()).ToArray();
                        var detectAndCompeteResult = ImageSimilaritySearcher.DetectAndCompete(cloned, MatchingFeaturePointMethod.ORB, targetMask);
                        Mat tgt = new Mat();
                        Cv2.DrawKeypoints(BitmapConverter.ToMat(cloned), detectAndCompeteResult.KeyPoints, tgt, Scalar.Green);
                        SrcTgtImgPrinted?.Invoke(new (src, tgt));
                    }
                }
                
                // Run task.
                var resultsList = await Task.WhenAll(rootTask);

                // Sorting list.
                var returns = resultsList.OrderByDescending(val => val.Result).ThenBy(val => val.Score).ToList();
                
                // For debug.
                if (_printResult && returns.Exists(val => val.Result))
                {
                    using var cloned = ((Bitmap) capturedImage.Clone()).PerformScale(300).PerformGrayScale();
                    
                    var scenes = SceneHolder.Instance.Scenes.ToArray().ToList();
                    var scene = scenes.First();
                    
                    var tgtResult = ImageSimilaritySearcher.DetectAndCompete(cloned, MatchingFeaturePointMethod.ORB,
                        scene.ScrappedImage.Setting.ScrapInfos.Select(val => val.ScrapArea.ToOpenCvRect()).ToArray());
                    var srcResult = scene.ScrappedImage.FeaturePoints.KeyPoints;
                    var result = returns.First();

                    using var srcMat = BitmapConverter.ToMat(scene.ScrappedImage.Image);
                    using var tgtMat = BitmapConverter.ToMat(cloned);
                    using Mat output = new Mat();
                    Cv2.DrawMatches(srcMat,
                        srcResult,
                        tgtMat,
                        tgtResult.KeyPoints,
                        result.Matches,
                        output,
                        matchColor: Scalar.Green,
                        flags: DrawMatchesFlags.Default);
                    
                    ResultPrinted?.Invoke(output);
                }

                return returns.ToList();
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                throw;
            }
        }

        private async Task<MatchingResult> GetMatchingResult(Bitmap capturedImage, Scene scene, DetectAndCompeteResult featurePoint)
        {
            return await Task<MatchingResult>.Run(() =>
            {
                var maskArea = scene.ScrappedImage.SamplingAreas.Select(val => val.ScrapArea.ToOpenCvRect()).ToArray();

                var matchingResult = ImageSimilaritySearcher.FeaturePointsMatching(featurePoint, capturedImage, MatchingFeaturePointMethod.ORB, maskArea);

                return matchingResult.WithSceneName(scene.Setting.Name);
            });
        }
    }
}